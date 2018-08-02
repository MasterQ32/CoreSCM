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

		public event EventHandler<ResolveImportEventArgs> ResolveImport;

		private List<Component> importedComponents = new List<Component>();

		private List<Schematic> schematics = new List<Schematic>();

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
			var ea = new ResolveImportEventArgs(path.ToArray());
			this.ResolveImport?.Invoke(this, ea);
			if (ea.Library == null)
				throw new ParserException(lastToken, $"Could not find library {string.Join(".", path)}");
			else
				this.importedComponents.AddRange(ea.Library);
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
			this.schematics.Add(schematic);
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
				switch (socket.Type)
				{
					case TType.IDENTIFIER:
						var items = new List<ResourcePath>(Range.Unwrap(socket.Value).Select(s => new ResourcePath(s)));
						while (Peek().Type == TType.DOT)
						{
							Next(TType.DOT);
							var more = Range.Unwrap(Next(TType.IDENTIFIER).Value);
							var next = new List<ResourcePath>();
							foreach (var add in more)
							{
								foreach (var item in items)
								{
									var x = item.Clone();
									x.Add(add);
									next.Add(x);
								}
							}
							items = next;
						}

						if (items.Count == 0)
							throw new ParserException(socket, "Requires at least a single specification!");

						foreach (var item in items)
							sequence.Add(item);

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

							sequence.Add(new InlineComponent(comp, socket.Groups["value"]));
							break;
						}
					default:
						throw new ParserException(socket, "Unexpected token!");
				}

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
			var bus = new List<ISocket[]>();

			// Validate stream here
			if (stream.Any(x => x.Count != 1))
			{
				int max = stream.Max(x => x.Count);
				
				// all elements have "max" count
				for (int i = 0; i < max; i++)
				{
					var substream = new ISocket[stream.Count];
					for (int j = 0; j < substream.Length; j++)
					{
						if (stream[j].Count == 1)
						{
							substream[j] = stream[j][0];
						}
						else
						{
							substream[j] = stream[j][i];
						}
					}
					bus.Add(substream);
				}
			}
			else
			{
				// We only have a 
				bus.Add(stream.Select(x => x.Single()).ToArray());
			}

			foreach (var childnodes in bus)
			{
				var nodes = childnodes.Select(x =>
				{
					Function function = null;
					Signal signal = null;
					ComponentInstance inline = null;
					if (x is InlineComponent inl)
					{
						var name = string.Format("{0}{1}",
							inl.Type.Name,
							schematic.Components.Count(c => c.Component == inl.Type) + 1);
						var inst = schematic.AddInstance(name, inl.Type);
						inst.AddAttribute(new Attribute("value", inl.Value));
						return new { Signal = signal, Function = function, Component = inst };
					}
					else
					{
						GetResource(schematic, x as ResourcePath, out signal, out function);
						return new { Signal = signal, Function = function, Component = inline };
					}
				}).ToArray();

				// Now connect "left" and "right" sides of our stream components:
				for (int i = 0; i < nodes.Length - 1; i++)
				{
					var left = nodes[i + 0];
					var right = nodes[i + 1];
					if (i == 0 && left.Component != null)
						throw new ParserException(lastToken, "Inline component not between two signals");
					if (i == nodes.Length - 2 && right.Component != null)
						throw new ParserException(lastToken, "Inline component not between two signals");
					if (left.Signal != null && right.Signal != null)
						throw new ParserException(lastToken, "Cannot connect two signals");

					var signal = left.Signal ?? right.Signal ?? schematic.AddAnonymousSignal();
					if (left.Signal == null)
					{
						if (left.Component != null)
							signal.Attach(left.Component.GetFunction("[0]"));
						else if (left.Function != null)
							signal.Attach(left.Function);
						else
							throw new ParserException(lastToken, "Incomplete attachment");
					}
					if (right.Signal == null)
					{
						if (right.Component != null)
							signal.Attach(right.Component.GetFunction("[1]"));
						else if (right.Function != null)
							signal.Attach(right.Function);
						else
							throw new ParserException(lastToken, "Incomplete attachment");
					}
				}
			}
		}

		private NodeType GetResource(Schematic schematic, ResourcePath path, out Signal signal, out Function function)
		{
			if (path.Count == 1)
				signal = schematic.GetSignal(path[0]);
			else
				signal = null;

			if (path.Count == 2)
			{
				var component = schematic.GetInstance(path[0]);
				if (component == null)
					throw new ParserException(lastToken, $"Could not find component {path[0]}.");
				function = component.GetFunction(path[1]);
				if (function == null)
					throw new ParserException(lastToken, $"Component {path[0]} ({component.Component.Name}) does not contain a function {path[1]}");
			}
			else
			{
				function = null;
			}
			if (signal == null && function == null)
				throw new ParserException(lastToken, $"Unrecognized symbol: {string.Join(".", path)}");
			else if (signal != null && function == null)
				return NodeType.Signal;
			else if (signal == null && function != null)
				return NodeType.Function;
			else
				throw new ParserException(lastToken, "Ambigious symbol definition");

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
			if (name.Length == 1)
			{
				var c = this.importedComponents.SingleOrDefault(x => x.Name == name[0]);
				if (c != null)
					return c;
			}

			var ea = new ResolveComponentEventArgs(name);
			this.ResolveComponent?.Invoke(this, ea);
			if (ea.Component == null)
				throw new ParserException(lastToken, $"Could not resolve the symbol {string.Join(".", name)}");
			return ea.Component;
		}

		public IReadOnlyCollection<Schematic> Schematics => this.schematics;

		private interface ISocket
		{

		}

		private class ResourcePath : ISocket, IList<string>, ICloneable
		{
			private readonly List<string> items;

			public string this[int index] { get => items[index]; set => items[index] = value; }

			public int Count => items.Count;

			public bool IsReadOnly => ((IList<string>)items).IsReadOnly;

			public ResourcePath()
			{
				this.items = new List<string>();
			}

			public ResourcePath(params string[] items)
			{
				this.items = new List<string>(items);
			}

			public ResourcePath Clone() => new ResourcePath(this.items.ToArray());

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

			object ICloneable.Clone() => this.Clone();
		}

		private class InlineComponent : ISocket
		{
			public InlineComponent(Component inst, string value)
			{
				this.Type = inst ?? throw new ArgumentNullException(nameof(inst));
				this.Value = value ?? throw new ArgumentNullException(nameof(value));
			}

			public Component Type { get; }
			public string Value { get; }
		}

		private enum NodeType
		{
			Unknown = 0,
			Signal = 1,
			Function = 2,
		}
	}
}
