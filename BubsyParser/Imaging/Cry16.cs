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
	internal struct Cry16 : IPixel<Cry16>
	{

		public byte Color { get; set; }
		public byte Intensity { get; set; }

		public Cry16(byte color, byte intensity) => (Color, Intensity) = (color, intensity);

		public readonly Vector3 ToVector3()
		{
			return new Vector3(
				CrToRed[Color] * Intensity / (float)byte.MaxValue / byte.MaxValue,
				CrToGreen[Color] * Intensity / (float)byte.MaxValue / byte.MaxValue,
				CrToBlue[Color] * Intensity / (float)byte.MaxValue / byte.MaxValue
			);
		}

		public Vector4 ToVector4() => new(ToVector3(), 1F);
		public Vector4 ToScaledVector4() => ToVector4();
		public void FromVector4(Vector4 vector)
		{
			throw new NotSupportedException();
		}

		public void FromScaledVector4(Vector4 vector) => FromVector4(vector);

		public readonly PixelOperations<Cry16> CreatePixelOperations() => new PixelOperations();


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


		public bool Equals(Cry16 other) => Color == other.Color && Intensity == other.Intensity;

		public override bool Equals([NotNullWhen(true)] object? other) => other is Cry16 pixel && Equals(pixel);

		public override int GetHashCode() => HashCode.Combine(Color, Intensity);

		public static bool operator ==(Cry16 left, Cry16 right) => left.Equals(right);

		public static bool operator !=(Cry16 left, Cry16 right) => !left.Equals(right);


		internal class PixelOperations : PixelOperations<Cry16>
		{
			private static readonly Lazy<PixelTypeInfo> LazyInfo =
				new Lazy<PixelTypeInfo>(() => new PixelTypeInfo(16, PixelAlphaRepresentation.None), true);

			/// <inheritdoc />
			public override PixelTypeInfo GetPixelTypeInfo() => LazyInfo.Value;
		}

		private static readonly byte[] CrToRed =
		{
			  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
			 34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  19,   0,
			 68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  64,  43,  21,   0,
			102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102,  95,  71,  47,  23,   0,
			135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 130, 104,  78,  52,  26,   0,
			169, 169, 169, 169, 169, 169, 169, 169, 169, 170, 141, 113,  85,  56,  28,   0,
			203, 203, 203, 203, 203, 203, 203, 203, 203, 183, 153, 122,  91,  61,  30,   0,
			237, 237, 237, 237, 237, 237, 237, 237, 230, 197, 164, 131,  98,  65,  32,   0,
			255, 255, 255, 255, 255, 255, 255, 255, 247, 214, 181, 148, 115,  82,  49,  17,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 235, 204, 173, 143, 112,  81,  51,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 198, 170, 141, 113,  85,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 223, 197, 171, 145, 119,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 248, 224, 200, 177, 153,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 252, 230, 208, 187,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 240, 221,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		};

		private static readonly byte[] CrToGreen =
		{
			  0,  17,  34,  51,  68,  85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255,
			  0,  19,  38,  57,  77,  96, 115, 134, 154, 173, 192, 211, 231, 250, 255, 255,
			  0,  21,  43,  64,  86, 107, 129, 150, 172, 193, 215, 236, 255, 255, 255, 255,
			  0,  23,  47,  71,  95, 119, 142, 166, 190, 214, 238, 255, 255, 255, 255, 255,
			  0,  26,  52,  78, 104, 130, 156, 182, 208, 234, 255, 255, 255, 255, 255, 255,
			  0,  28,  56,  85, 113, 141, 170, 198, 226, 255, 255, 255, 255, 255, 255, 255,
			  0,  30,  61,  91, 122, 153, 183, 214, 244, 255, 255, 255, 255, 255, 255, 255,
			  0,  32,  65,  98, 131, 164, 197, 230, 255, 255, 255, 255, 255, 255, 255, 255,
			  0,  32,  65,  98, 131, 164, 197, 230, 255, 255, 255, 255, 255, 255, 255, 255,
			  0,  30,  61,  91, 122, 153, 183, 214, 244, 255, 255, 255, 255, 255, 255, 255,
			  0,  28,  56,  85, 113, 141, 170, 198, 226, 255, 255, 255, 255, 255, 255, 255,
			  0,  26,  52,  78, 104, 130, 156, 182, 208, 234, 255, 255, 255, 255, 255, 255,
			  0,  23,  47,  71,  95, 119, 142, 166, 190, 214, 238, 255, 255, 255, 255, 255,
			  0,  21,  43,  64,  86, 107, 129, 150, 172, 193, 215, 236, 255, 255, 255, 255,
			  0,  19,  38,  57,  77,  96, 115, 134, 154, 173, 192, 211, 231, 250, 255, 255,
			  0,  17,  34,  51,  68,  85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255,
		};

		private static readonly byte[] CrToBlue =
		{
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 240, 221,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 252, 230, 208, 187,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 248, 224, 200, 177, 153,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 223, 197, 171, 145, 119,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 198, 170, 141, 113,  85,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 235, 204, 173, 143, 112,  81,  51,
			255, 255, 255, 255, 255, 255, 255, 255, 247, 214, 181, 148, 115,  82,  49,  17,
			237, 237, 237, 237, 237, 237, 237, 237, 230, 197, 164, 131,  98,  65,  32,   0,
			203, 203, 203, 203, 203, 203, 203, 203, 203, 183, 153, 122,  91,  61,  30,   0,
			169, 169, 169, 169, 169, 169, 169, 169, 169, 170, 141, 113,  85,  56,  28,   0,
			135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 130, 104,  78,  52,  26,   0,
			102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102,  95,  71,  47,  23,   0,
			 68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  68,  64,  43,  21,   0,
			 34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  34,  19,   0,
			  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
		};

	}
}
