using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BubsyParser
{
	public class Tileset : IReadOnlyList<Tile>
	{

		const int TileWidth = 16;
		const int TileHeight = 16;



		private readonly Tile[] _tiles;



		public string Name { get; }
		public int Count => _tiles.Length;
		public Tile this[int index] => _tiles[index];
		public Palette Palette { get; }



		private Tileset(string name, Tile[] tiles, Palette palette)
		{
			Name = name;
			_tiles = tiles;
			Palette = palette;
		}



		public static Tileset FromFiles(string name, string path)
		{
			var palette = Palette.FromFile(Path.Combine(path, name), TransparencyMode.FirstInPalette);

			var tileImages = ReadTiles(Path.Combine(path, name + ".DAT"), palette);
			var btr = ReadBtr(Path.Combine(path, name + ".BTR"));

			var tiles = new Tile[tileImages.Length];
			for (int i = 0; i < tiles.Length; i++)
			{
				var image = tileImages[i];
				var btrMapping = btr[i];			

				tiles[i] = new Tile(i, image, btrMapping.Flags, btrMapping.Record.Acceleration, btrMapping.Record.Heights.ToImmutableArray());
			}

			return new Tileset(name, tiles, palette);
		}



		private static Image<Rgba32>[] ReadTiles(ReadOnlySpan<byte> data, Palette palette)
		{
			const int TileSize = TileWidth * TileHeight;

			if (data.Length % TileSize != 0)
				throw new InvalidDataException("Tile set file is not a multiple of tile size");

			int count = data.Length / TileSize;

			var reader = new SpanReader(data);

			var tiles = new Image<Rgba32>[count];

			for (int i = 0; i < count; i++)
			{
				var tilePaletteIndexes = reader.ReadBytes(TileSize);
				tiles[i] = palette.CreateIndexedImage<Rgba32>(tilePaletteIndexes, TileWidth, TileHeight);
			}

			return tiles;
		}

		private static Image<Rgba32>[] ReadTiles(string path, Palette palette) => ReadTiles(File.ReadAllBytes(path), palette);



		private static List<BtrTileMapping> ReadBtr(ReadOnlySpan<byte> data)
		{
			var reader = new SpanReader(data);

			ushort count = reader.ReadUInt16BigEndian();

			var records = new BtrRecord[count];

			for (int i = 0; i < count; i++)
			{
				var heights = reader.ReadBytes(16).ToArray();
				var accel = reader.ReadInt16BigEndian();
				Debug.Assert(heights.All(h => h >= 0 && h <= 16), "Unexpected height values in BTR");
				records[i] = new(heights, accel);
			}

			var mappings = new List<BtrTileMapping>();
			for (int i = 0; i < count && reader.Remaining > 0; i++)
			{
				var value = reader.ReadUInt16BigEndian();
				var index = value & 0x3FF;
				var flags = (TileFlag)(value & ~0x3FF);
				Debug.Assert(Enum.IsDefined(flags), $"Unexpected tile flags: {flags}");
				mappings.Add(new(records[index], flags));
			}

			// 0097A2	cls.r (A1)
			mappings[0] = new(records[0], TileFlag.None);

			return mappings;
		}
		private static List<BtrTileMapping> ReadBtr(string path)
		{
			return ReadBtr(File.ReadAllBytes(path));
		}

		public IEnumerator<Tile> GetEnumerator()
		{
			return ((IEnumerable<Tile>)_tiles).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _tiles.GetEnumerator();
		}

		private record struct BtrRecord(byte[] Heights, short Acceleration);
		private record struct BtrTileMapping(BtrRecord Record, TileFlag Flags);
	}
}
