using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BubsyParser.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BubsyParser
{
	public sealed class SpriteSet : IDisposable
	{

		public ImmutableArray<Image<Rgba32>> Images { get; private set; }



		private SpriteSet(IEnumerable<Image<Rgba32>> images)
		{
			Images = images.ToImmutableArray();
		}



		public static SpriteSet FromPP(ReadOnlySpan<byte> data, Palette? palette = null)
		{
			const int HeaderSize = 32;

			var count = (int)BinaryPrimitives.ReadUInt32BigEndian(data) / HeaderSize;

			var images = new List<Image<Rgba32>>(count);
			var offsetIndexes = new Dictionary<uint, int>(count);

			var reader = new SpanReader(data);
			for (int i = 0; i < count; i++)
			{
				uint offset = reader.ReadUInt32BigEndian();
				reader.ReadUInt32BigEndian();
				reader.ReadUInt32BigEndian();
				reader.ReadUInt32BigEndian();
				reader.ReadUInt16BigEndian();
				ushort width = reader.ReadUInt16BigEndian();
				ushort height = reader.ReadUInt16BigEndian();
				reader.Skip(10);

				Image<Rgba32> rgbaImage;
				if (palette == null)
				{
					using var cryImage = Image.LoadPixelData<Cry16Alpha>(data.Slice((int)offset, width * height * sizeof(ushort)), width, height);
					rgbaImage = cryImage.CloneAs<Rgba32>(cryImage.GetConfiguration());
				}
				else
				{
					rgbaImage = palette.CreateIndexedImage<Rgba32>(data.Slice((int)offset, width * height), width, height);
				}

				if (offsetIndexes.TryGetValue(offset, out int index))
				{
					images[index] = rgbaImage;
				}
				else
				{
					images.Add(rgbaImage);
					offsetIndexes.Add(offset, images.Count - 1);
				}
			}

			return new SpriteSet(images);
		}

		public static SpriteSet FromPP(string path, Palette? palette = null)
		{
			return FromPP(File.ReadAllBytes(path), palette);
		}


		public static SpriteSet FromJhdJsp(ReadOnlySpan<byte> jhdData, ReadOnlySpan<byte> jspData, Palette? palette = null)
		{
			var images = new List<Image<Rgba32>>();
			var offsetIndexes = new Dictionary<uint, int>();

			var jhdReader = new SpanReader(jhdData);
			while(jhdReader.Remaining > 0)
			{
				uint offset = jhdReader.ReadUInt32BigEndian();
				ushort width = jhdReader.ReadUInt16BigEndian();
				ushort height = jhdReader.ReadUInt16BigEndian();
				jhdReader.Skip(8);

				Image<Rgba32> rgbaImage;
				if (palette == null)
				{
					using var cryImage = Image.LoadPixelData<Cry16Alpha>(jspData.Slice((int)offset, width * height * sizeof(ushort)), width, height);
					rgbaImage = cryImage.CloneAs<Rgba32>(cryImage.GetConfiguration());
				}
				else
				{
					rgbaImage = palette.CreateIndexedImage<Rgba32>(jspData.Slice((int)offset, width * height), width, height);
				}

				if (offsetIndexes.TryGetValue(offset, out int index))
				{
					images[index] = rgbaImage;
				}
				else
				{
					images.Add(rgbaImage);
					offsetIndexes.Add(offset, images.Count - 1);
				}
			}

			return new SpriteSet(images);
		}

		public static SpriteSet FromJhdJsp(string path, Palette? palette = null)
		{
			var directory = Path.GetDirectoryName(path);
			var name = Path.GetFileNameWithoutExtension(path);
			return FromJhdJsp(
				File.ReadAllBytes(Path.Combine(directory, name + ".jhd")),
				File.ReadAllBytes(Path.Combine(directory, name + ".jsp")),
				palette
			);
		}



		public void Dispose()
		{
			foreach (var image in Images)
				image.Dispose();
			Images = ImmutableArray<Image<Rgba32>>.Empty;
		}
	}
}
