using System.Collections;
using System.Runtime.CompilerServices;
using BubsyParser.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BubsyParser
{
	public sealed class Palette : IReadOnlyList<Color>
	{

		private readonly Color[] _colors;


		public Color this[int index] => _colors[index];
		public int Count => _colors.Length;


		public Palette(Color[] colors)
		{
			_colors = colors;
		}



		public static Palette FromBytes(ReadOnlySpan<byte> data, bool cryFormat, TransparencyMode transparency)
		{
			Image src;

			if (!cryFormat)
			{
				if (data.Length != 512)
					throw new InvalidDataException($"Length of palette data is incorrect. Expected 512 bytes, got {data.Length}");

				src = Image.LoadPixelData<Rbg556>(data, 256, 1);
			}
			else
			{
				if (data.Length == 514)
					data = data.Slice(2);
				else if (data.Length != 512)
					throw new InvalidDataException($"Length of palette data is incorrect. Expected 512 or 514 bytes, got {data.Length}");

				if (transparency.HasFlag(TransparencyMode.Cry0))
					src = Image.LoadPixelData<Cry16Alpha>(data, 256, 1);
				else
					src = Image.LoadPixelData<Cry16>(data, 256, 1);
			}

			using (src)
			{
				using var rgba32 = src.CloneAs<Rgba32>(src.GetConfiguration());

				if (transparency.HasFlag(TransparencyMode.FirstInPalette))
					rgba32[0, 0] = rgba32[0, 0] with { A = 0 };

				var colors = new Color[256];
				for (int i = 0; i < colors.Length; i++)
					colors[i] = rgba32[i, 0];

				return new Palette(colors);
			}
		}
		public static Palette FromBytes(ReadOnlySpan<byte> data, string name, TransparencyMode transparency)
		{
			bool cryFormat = String.Equals(Path.GetExtension(name), ".cry", StringComparison.OrdinalIgnoreCase);
			return FromBytes(data, cryFormat, transparency);
		}

		public static Palette FromFile(string path, TransparencyMode transparency)
		{
			string rgbPath = path + ".RGB";
			string cryPath = path + ".CRY";

			string palettePath;
			if (File.Exists(path))
				palettePath = path;
			else if (File.Exists(rgbPath))
				palettePath = rgbPath;
			else if (File.Exists(cryPath))
				palettePath = cryPath;
			else
				throw new FileNotFoundException($"Palette file \"{path}\" not found.");

			var data = File.ReadAllBytes(palettePath);
			return FromBytes(data, palettePath, transparency);
		}


		[SkipLocalsInit]
		public Image<TPixel> CreateIndexedImage<TPixel>(ReadOnlySpan<byte> data, int width, int height)
			where TPixel : unmanaged, IPixel<TPixel>
		{
			Span<TPixel> imageData = stackalloc TPixel[width * height];

			for (int i = 0; i < imageData.Length; i++)
				imageData[i] = _colors[data[i]].ToPixel<TPixel>();

			return Image.LoadPixelData<TPixel>(imageData, width, height);
		}



		public IEnumerator<Color> GetEnumerator() => ((IEnumerable<Color>)_colors).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _colors.GetEnumerator();
	}
}
