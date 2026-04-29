using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubsyParser
{
	internal ref struct SpanReader
	{
		private ReadOnlySpan<byte> _span;

		public int Remaining => _span.Length;

		public SpanReader(ReadOnlySpan<byte> span)
		{
			_span = span;
		}

		[DebuggerHidden]
		private void Advance(int count)
		{
			_span = _span.Slice(count);
		}

		[DebuggerHidden]
		private unsafe T AdvancePast<T>(T value)
			where T : unmanaged
		{
			Advance(sizeof(T));
			return value;
		}


		public Byte ReadByte() => AdvancePast(_span[0]);
		public SByte ReadSByte() => AdvancePast(unchecked((sbyte)_span[0]));
		public Int16 ReadInt16BigEndian() => AdvancePast(BinaryPrimitives.ReadInt16BigEndian(_span));
		public UInt16 ReadUInt16BigEndian() => AdvancePast(BinaryPrimitives.ReadUInt16BigEndian(_span));
		public Int32 ReadInt32BigEndian() => AdvancePast(BinaryPrimitives.ReadInt32BigEndian(_span));
		public UInt32 ReadUInt32BigEndian() => AdvancePast(BinaryPrimitives.ReadUInt32BigEndian(_span));

		public ReadOnlySpan<byte> ReadBytes(int count)
		{
			var bytes = _span.Slice(0, count);
			Advance(count);
			return bytes;
		}

		public void Skip(int count) => Advance(count);

		public SpanReader Clone() => this;

	}
}
