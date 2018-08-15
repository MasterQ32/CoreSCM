using System;

namespace CoreSchematic
{
	/// <summary>
	/// A combination of a part and a pin name.
	/// </summary>
	public sealed class PinAttachment : IEquatable<PinAttachment>
	{
		public PinAttachment(Part part, string pin)
		{
			this.Part = part ?? throw new ArgumentNullException(nameof(part));
			this.Pin = pin ?? throw new ArgumentNullException(nameof(pin));
		}

		public Part Part { get; }

		public string Pin { get; }

		public override bool Equals(object obj) => Equals(obj as PinAttachment);

		public bool Equals(PinAttachment other)
		{
			return (other != null)
				&& Equals(other.Pin, this.Pin)
				&& Equals(other.Part, this.Part);
		}

		public override int GetHashCode() => Part.GetHashCode() ^ Pin.GetHashCode();

		public override string ToString() => $"{Part.Name}.{Pin}";
	}
}