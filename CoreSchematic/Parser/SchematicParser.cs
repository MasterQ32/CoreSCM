using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Token = CoreSchematic.Parser.SchematicToken;
using TType = CoreSchematic.Parser.SchematicTokens;

namespace CoreSchematic.Parser
{
	public sealed class SchematicParser
	{
		private readonly Queue<Token> tokens = new Queue<Token>();
		private Token lastToken;

		private SchematicTokenizer tokenizer;

		public event EventHandler<ResolveComponentEventArgs> ResolveComponent;

		public SchematicParser()
		{

		}

		private void EnsureTokenAvailable()
		{
			if (this.tokens.Count >= 1)
				return;
			var tok = this.tokenizer.Read();
			if (tok == null)
				return;
			this.tokens.Enqueue(tok);
		}

		private Token Peek()
		{
			EnsureTokenAvailable();
			return this.tokens.Count > 0 ? this.tokens.Peek() : null;
		}

		private Token Next()
		{
			EnsureTokenAvailable();
			return lastToken = (this.tokens.Count > 0 ? this.tokens.Dequeue() : null);
		}

		private Token Next(params TType[] options)
		{
			var tok = Next();
			if (tok == null)
				throw new ParserException("Unexpected end of text!");
			if (options.Contains(tok.Type))
				return tok;
			throw new ParserException(tok, $"Expected any of {string.Join(", ", options)}.");
		}

		private string NextKeyword(params string[] options)
		{
			var tok = Next(TType.KEYWORD);
			var kw = tok.Value.ToUpper();
			if (options.Contains(kw))
				return kw;
			throw new ParserException(tok, $"Expected any keyword of {string.Join(", ", options)}.");
		}

		private void NextSemicolon()
		{
			Next(TType.SEMICOLON);
		}

		private string NextIdentifier()
		{
			return Next(TType.IDENTIFIER).Value;
		}

		private string[] NextSymbol()
		{
			var names = new List<string>();
			names.Add(NextIdentifier());
			while (Peek().Type == TType.DOT)
			{
				Next(TType.DOT);
				names.Add(NextIdentifier());
			}
			return names.ToArray();
		}

		public void Parse(TextReader sr)
		{
			try
			{
				this.tokenizer = new SchematicTokenizer(sr);
				this.ParseDocument();
			}
			finally { this.tokenizer = null; }
		}

		private void ParseDocument()
		{
			while (!tokenizer.EndOfText)
			{
				var kw = NextKeyword("IMPORT", "SCHEMATIC");
				switch (kw)
				{
					case "IMPORT":
						this.ParseImport();
						break;
					case "SCHEMATIC":
						this.ParseSchematic();
						break;
				}
			}
		}

		private void ParseImport()
		{
			var path = new List<string>();
			do
			{
				path.Add(NextIdentifier());
			} while (Next(TType.DOT, TType.SEMICOLON).Type != TType.SEMICOLON);
		}

		private void ParseSchematic()
		{
			var name = NextIdentifier();
			var schematic = new Schematic(name);
			Next(TType.BRACE_OP);
			while (Peek().Type != TType.BRACE_CL)
			{
				if (Peek().Type == TType.KEYWORD)
				{
					switch (NextKeyword("DEVICE", "SIGNAL", "BUS"))
					{
						case "DEVICE":
							ParseDeviceDef(schematic);
							break;
						case "SIGNAL":
							ParseSignalDef(schematic);
							break;
						case "BUS":
							ParseBusDef(schematic);
							break;
					}
				}
				else
				{
					ParseConnector(schematic);
				}
			}
			Next(TType.BRACE_CL);
		}

		private void ParseConnector(Schematic schematic)
		{
			Token first = null;
			var stream = new List<List<ISocket>>();

			var sequence = new List<ISocket>();
			stream.Add(sequence);
			do
			{
				var socket = Next(TType.IDENTIFIER, TType.INLINE_PART);
				first = first ?? socket;
				ISocket sock;
				switch (socket.Type)
				{
					case TType.IDENTIFIER:
						var path = new ResourcePath { socket.Value };
						while (Peek().Type == TType.DOT)
						{
							Next(TType.DOT);
							path.Add(Next(TType.IDENTIFIER).Value);
						}
						
						// TODO: Expand the range specifiers here
						
						sock= path;
						break;
					case TType.INLINE_PART:
						{
							Component comp;
							switch (socket.Groups["type"])
							{
								case "R": comp = Component.Resistor; break;
								case "C": comp = Component.UnipolarCapacitor; break;
								case "L": comp = Component.Inductivity; break;
								default:
									throw new ParserException(socket, "Unkown inline component!");
							}

							var name = string.Format("{0}{1}",
								socket.Groups["type"],
								schematic.Components.Count(c => c.Component == comp) + 1);

							var inst = schematic.AddInstance(name, comp);
							inst.AddAttribute(new Attribute("value", socket.Groups["value"]));
							sock = new InlineComponent(inst);
							break;
						}
					default:
						throw new ParserException(socket, "Unexpected token!");
				}
				sequence.Add(sock);

				switch (Next(TType.CONNECTOR, TType.SEMICOLON, TType.COMMA).Type)
				{
					case TType.SEMICOLON:
						// Semicolon finalizes the stream
						goto stream_done;
					case TType.COMMA:
						// Comma separates elements in the current sequence, 
						// so don't do anything here
						break;
					case TType.CONNECTOR:
						// Connector separates sequences in a stream
						// So a new connector means start a new sequence
						sequence = new List<ISocket>();
						stream.Add(sequence);
						break;
				}

			} while (true);
		stream_done:
			// Validate stream here
			if (stream.Any(x => x.Count != 1))
			{
				int max = stream.Max(x => x.Count);
				if (stream.Any(x => x.Count != max))
				{
					if (!stream.All(x => x.Count == max || x.Count == 1))
						throw new ParserException(first, $"Count mismatch: Not all sockets have 1 or {max} elements!");
					// at least one element has max count and 1 count
				}
				else
				{
					// all elements have "max" count
				}
			}
			else
			{

			}
		}

		private void ParseSignalDef(Schematic schematic)
		{
			do
			{
				schematic.AddSignal(NextIdentifier());
			} while (Next(TType.COMMA, TType.SEMICOLON).Type == TType.COMMA);
		}

		private void ParseDeviceDef(Schematic schematic)
		{
			var names = new List<string>();
			do
			{
				names.Add(NextIdentifier());
			} while (Next(TType.COLON, TType.COMMA).Type == TType.COMMA);

			var type = DoResolveComponent(NextSymbol());

			var attribs = new Dictionary<string, Attribute>();
			if (Next(TType.SEMICOLON, TType.BRACE_OP).Type == TType.BRACE_OP)
			{
				while (Peek().Type != TType.BRACE_CL)
				{
					var attrib = new Attribute(NextIdentifier());
					Next(TType.PAREN_OP);
					do
					{
						attrib.Values.Add(Next(TType.IDENTIFIER, TType.STRING, TType.NUMBER).Groups["value"]);
					} while (Next(TType.COMMA, TType.PAREN_CL).Type != TType.PAREN_CL);
					NextSemicolon();
					attribs.Add(attrib.Name, attrib);
				}
				Next(TType.BRACE_CL);
			}

			foreach (var name in names)
			{
				var inst = schematic.AddInstance(name, type);
				foreach (var a in attribs)
					inst.AddAttribute(a.Value);
			}
		}

		private void ParseBusDef(Schematic schematic)
		{
			throw new NotImplementedException();
		}

		private Component DoResolveComponent(string[] name)
		{
			var ea = new ResolveComponentEventArgs(name);
			this.ResolveComponent?.Invoke(this, ea);
			if (ea.Component == null)
				throw new ParserException(lastToken, $"Could not resolve the symbol {string.Join(".", name)}");
			return ea.Component;
		}


		private interface ISocket
		{

		}

		private class ResourcePath : ISocket, IList<string>
		{
			private readonly List<string> items = new List<string>();

			public string this[int index] { get => items[index]; set => items[index] = value; }

			public int Count => items.Count;

			public bool IsReadOnly => ((IList<string>)items).IsReadOnly;

			public void Add(string item)
			{
				items.Add(item);
			}

			public void Clear()
			{
				items.Clear();
			}

			public bool Contains(string item)
			{
				return items.Contains(item);
			}

			public void CopyTo(string[] array, int arrayIndex)
			{
				items.CopyTo(array, arrayIndex);
			}

			public IEnumerator<string> GetEnumerator()
			{
				return ((IList<string>)items).GetEnumerator();
			}

			public int IndexOf(string item)
			{
				return items.IndexOf(item);
			}

			public void Insert(int index, string item)
			{
				items.Insert(index, item);
			}

			public bool Remove(string item)
			{
				return items.Remove(item);
			}

			public void RemoveAt(int index)
			{
				items.RemoveAt(index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IList<string>)items).GetEnumerator();
			}
		}

		private class InlineComponent : ISocket
		{
			public InlineComponent(ComponentInstance inst)
			{
			}
		}
	}
}
