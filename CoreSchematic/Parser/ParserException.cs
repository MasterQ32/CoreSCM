using System;
using System.Runtime.Serialization;

namespace CoreSchematic.Parser
{
	[Serializable]
	internal class ParserException : Exception
	{
		public ParserException()
		{
		}

		public ParserException(string message) : base(message)
		{
		}

		public ParserException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ParserException(SchematicToken tok, string message) : this(FormatMsg(tok, message))
		{
			this.Context = tok;
		}

		public ParserException(SchematicToken tok, string message, Exception innerException) : this(FormatMsg(tok, message), innerException)
		{
			this.Context = tok;
		}

		protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		
		public SchematicToken Context {get; set; }
		
		static string FormatMsg(SchematicToken tok, string message) => $"Error in line {tok.LineNumber}: {message}";
	}
}