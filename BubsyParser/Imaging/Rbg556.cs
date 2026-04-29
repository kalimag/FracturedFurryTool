using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace BubsyParser.Imaging
{
	[DebuggerTypeProxy(typeof(DebugView))]
	internal struct Rbg556 : IPixel<Rbg556>
	{

		public ushort PackedValue { get; set; }

		public Rbg556(float x, float y, float z)
			: this(new Vector3(x, y, z))
		{ }

		public Rbg556(Vector3 vector) => PackedValue = Pack(ref vector);

		private static ushort Pack(ref Vector3 vector)
		{
			vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);

			var packedValue = (ushort)(
				((int)Math.Round(vector.X * 31F) & 0x1F) << 11 |
				((int)Math.Round(vector.Z * 31F) & 0x1F) << 6 |
				 (int)Math.Round(vector.Y * 63F) & 0x3F
			);

			if (BitConverter.IsLittleEndian)
				return BinaryPrimitives.ReverseEndianness(packedValue);
			else
				return packedValue;
		}

		public readonly Vector3 ToVector3()
		{
			var packedValue = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(PackedValue) : PackedValue;

			return new(
			   (packedValue >> 11 & 0x1F) * (1F / 31F),
			   (packedValue & 0x3F) * (1F / 63F),
			   (packedValue >> 6 & 0x1F) * (1F / 31F)
			);
		}

		public Vector4 ToVector4() => new(ToVector3(), 1F);
		public Vector4 ToScaledVector4() => ToVector4();
		public void FromVector4(Vector4 vector)
		{
			var vector3 = new Vector3(vector.X, vector.Y, vector.Z);
			PackedValue = Pack(ref vector3);
		}

		public void FromScaledVector4(Vector4 vector) => FromVector4(vector);

		public readonly PixelOperations<Rbg556> CreatePixelOperations() => new PixelOperations();


		public void ToRgba32(ref Rgba32 dest) => dest.FromScaledVector4(ToScaledVector4());
		public void FromArgb32(Argb32 source) => FromVector4(source.ToVector4());
		public void FromBgra5551(Bgra5551 source) => FromVector4(source.ToVector4());
		public void FromBgr24(Bgr24 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromBgra32(Bgra32 source) => FromVector4(source.ToVector4());
		public void FromAbgr32(Abgr32 source) => FromVector4(source.ToVector4());
		public void FromL8(L8 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromL16(L16 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromLa16(La16 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromLa32(La32 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromRgb24(Rgb24 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromRgba32(Rgba32 source) => FromVector4(source.ToVector4());
		public void FromRgb48(Rgb48 source) => FromScaledVector4(source.ToScaledVector4());
		public void FromRgba64(Rgba64 source) => FromScaledVector4(source.ToScaledVector4());


		public bool Equals(Rbg556 other) => other.PackedValue == PackedValue;

		public override bool Equals([NotNullWhen(true)] object? other) => other is Rbg556 pixel && Equals(pixel);

		public override int GetHashCode() => PackedValue;

		public static bool operator ==(Rbg556 left, Rbg556 right) => left.Equals(right);

		public static bool operator !=(Rbg556 left, Rbg556 right) => !left.Equals(right);


		internal class PixelOperations : PixelOperations<Rbg556>
		{
			private static readonly Lazy<PixelTypeInfo> LazyInfo =
				new Lazy<PixelTypeInfo>(() => new PixelTypeInfo(16, PixelAlphaRepresentation.None), true);

			/// <inheritdoc />
			public override PixelTypeInfo GetPixelTypeInfo() => LazyInfo.Value;
		}

		internal class DebugView
		{

			private readonly Rbg556 value;

			public string Hex => value.PackedValue.ToString("X4");

			public DebugView(Rbg556 value) => this.value = value;
		}
	}
}
