using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CoreSchematic.Parser
{
	public enum SchematicTokens
	{
		COMMENT = 1,
		KEYWORD = 2,
		INLINE_PART = 3,
		STRING = 4,
		NUMBER = 5,
		CONNECTOR = 6,
		IDENTIFIER = 7,
		DOT = 8,
		COMMA = 9,
		COLON = 10,
		SEMICOLON = 11,
		BRACE_OP = 12,
		BRACE_CL = 13,
		PAREN_OP = 14,
		PAREN_CL = 15,
		BRACKET_OP = 16,
		BRACKET_CL = 17,
		WHITESPACE = 18,
	}

	public sealed class SchematicToken
	{
		public SchematicToken(SchematicTokens type, Regex regex, Match match, int lineno)
		{
			this.Type = type;
			this.Value = match.Value;
			this.Offset = match.Index;
			this.Length = match.Length;
			this.LineNumber = lineno;
			foreach (var group in regex.GetGroupNames())
			{
				this.Groups[group] = match.Groups[group].Value;
			}
		}

		public SchematicTokens Type { get; }
		public string Value { get; }
		public int Offset { get; }
		public int Length { get; }
		public int LineNumber { get; }
		public NameValueCollection Groups { get; } = new NameValueCollection();

		public override string ToString() => $"{LineNumber} {Type} {Offset}:{Length} '{Value}'";
	}
	public sealed class SchematicTokenizer
	{
		private static readonly Tuple<SchematicTokens,bool,Regex>[] patterns = new []
		{
			Tuple.Create(SchematicTokens.COMMENT, false, new Regex(@"\/\*.*?\*\/", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline)),
			Tuple.Create(SchematicTokens.COMMENT, false, new Regex(@"\/\/.*$", RegexOptions.Compiled | RegexOptions.Multiline)),
			Tuple.Create(SchematicTokens.KEYWORD, true, new Regex(@"\b(?<id>device|schematic|signal|import|bus)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)),
			Tuple.Create(SchematicTokens.INLINE_PART, true, new Regex(@"\[(?<type>[RCL])\:(?<value>\d+(?:\.\d+)?)(?<unit>[pnµmdDkMGT]?)\]", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.STRING, true, new Regex(@"""(?<value>[^""]+)""", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline)),
			Tuple.Create(SchematicTokens.NUMBER, true, new Regex(@"\b(?<value>(?<magnitude>\d+(?:\.\d+)?)(?<unit>[pnµmdDkMGT]?))", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.CONNECTOR, true, new Regex(@"\-\-", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.IDENTIFIER, true, new Regex(@"(?<value>[\w_\-\+]+(?:\[[\d\,\.]+\][\w_\-\+]*)*)", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.DOT, true, new Regex(@"\.", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.COMMA, true, new Regex(@"\,", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.COLON, true, new Regex(@"\:", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.SEMICOLON, true, new Regex(@"\;", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.BRACE_OP, true, new Regex(@"\{", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.BRACE_CL, true, new Regex(@"\}", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.PAREN_OP, true, new Regex(@"\(", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.PAREN_CL, true, new Regex(@"\)", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.BRACKET_OP, true, new Regex(@"\[", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.BRACKET_CL, true, new Regex(@"\]", RegexOptions.Compiled)),
			Tuple.Create(SchematicTokens.WHITESPACE, false, new Regex(@"\s+", RegexOptions.Compiled)),
		};

		private readonly TextReader source;
		private bool endOfStream = false;
		private string code;
		private int cursor;
		private int[] lineoffset;
		
		public SchematicTokenizer(TextReader source)
		{
			this.source = source ?? throw new ArgumentNullException(nameof(source));
			this.lineoffset = new int[0];
			this.code = "";
		}

		public SchematicToken Read()
		{
			SchematicToken token;
			var startcursor = this.cursor;
			do
			{
				if (!endOfStream)
				{
					var buffer = new char[512];
					int len = source.Read(buffer, 0, buffer.Length);
					if (len > 0)
					{
						int prelen = this.code.Length;

						this.code += new string(buffer, 0, len);

						Array.Resize(ref this.lineoffset, this.code.Length);
						for (int i = Math.Max(1, prelen); i < this.code.Length; i++)
						{
							if (code[i - 1] == '\n')
								lineoffset[i] = lineoffset[i - 1] + 1;
							else
								lineoffset[i] = lineoffset[i - 1];
						}
					}
					else
						this.endOfStream = true;
				}

				token = null;
				var emit = false;
				for (int i = 0; i < patterns.Length; i++)
				{
					var match = patterns[i].Item3.Match(this.code, startcursor);

					// No match or the match doesn't start right at our
					// cursor
					if (!match.Success || match.Index != startcursor)
						continue;

					token = new SchematicToken(patterns[i].Item1, patterns[i].Item3, match, lineoffset[match.Index] + 1);
					cursor = startcursor + token.Length;
					emit = patterns[i].Item2;
					break;
				}

				// We didn't find a token
				if (token == null && endOfStream)
					return null;
				else if (token == null)
					continue;

				// We found a token, but it's possible that there may
				// be an earlier match but we were out of data
				if (!endOfStream && (token.Offset + token.Length) >= this.code.Length)
					continue;

				if (!emit)
				{
					startcursor = cursor;
					token = null;
				}

			} while (token == null);
			return token;
		}

		public bool EndOfText => ((this.cursor >= this.code.Length) && endOfStream);

		public static IEnumerable<SchematicToken> Tokenize(TextReader source)
		{
			var tn = new SchematicTokenizer(source);
			while (!tn.EndOfText)
				yield return tn.Read();
		}
	}
}
