using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubsyParser
{
	public class Map : IEnumerable<(MapTile tile, int x, int y)>
	{

		private readonly MapTile[] _tiles;


		public string Name { get; }

		public int Width { get; }
		public int Height { get; }

		public ImmutableArray<MapEntity> Entities { get; }

		public MapTile this[int index] => _tiles[index];
		public MapTile this[int x, int y] => _tiles[y * Width + x];



		private Map(string name, MapTile[] tiles, int width, int height, MapEntity[] entities)
		{
			Name = name;
			_tiles = tiles;
			Width = width;
			Height = height;
			Entities = ImmutableArray.Create(entities);
		}



		public static Map FromFiles(string name, string path, Tileset tileset)
		{
			var mpr = ReadMpr(Path.Combine(path, name + ".MPR"), tileset);
			var mapData = ReadMap(Path.Combine(path, name + ".MAP"), mpr);
			var entities = ReadCtl(Path.Combine(path, name + ".CTL"));

			return new Map(name, mapData.tiles, mapData.width, mapData.height, entities);
		}



		private static MapTile[] ReadMpr(ReadOnlySpan<byte> data, Tileset tileset)
		{
			var reader = new SpanReader(data);

			int count = reader.ReadUInt16BigEndian();

			var records = new MapTile[count];

			for (int i = 0; i < count; i++)
			{
				int foregroundIndex = reader.ReadUInt16BigEndian();
				int backgroundIndex = reader.ReadUInt16BigEndian();
				Tile? foreground = foregroundIndex != 0 ? tileset[foregroundIndex] : null;
				Tile? background = backgroundIndex != 0 ? tileset[backgroundIndex] : null;
				records[i] = new(foreground, background);
			}

			return records;
		}

		private static MapTile[] ReadMpr(string path, Tileset tileset) => ReadMpr(File.ReadAllBytes(path), tileset);



		private static (int width, int height, MapTile[] tiles) ReadMap(ReadOnlySpan<byte> data, MapTile[] mpr)
		{
			var reader = new SpanReader(data);

			int width = reader.ReadUInt16BigEndian();
			int height = reader.ReadUInt16BigEndian();

			var tiles = new MapTile[width * height];

			for (int i = 0; i < tiles.Length; i++)
				tiles[i] = mpr[reader.ReadUInt16BigEndian()];

			return (width, height, tiles);
		}

		private static (int width, int height, MapTile[] tiles) ReadMap(string path, MapTile[] mpr) => ReadMap(File.ReadAllBytes(path), mpr);



		private static MapEntity[] ReadCtl(ReadOnlySpan<byte> data)
		{
			var reader = new SpanReader(data);

			int count = reader.ReadUInt16BigEndian();

			var entities = new MapEntity[count];

			for (int i = 0; i < entities.Length; i++)
			{
				var x = reader.ReadInt16BigEndian();
				var y = reader.ReadInt16BigEndian();
				Debug.Assert(x >= 0 && y >= 0, "Entity with negative coordinates");
				entities[i] = new(x * 16, y * 16, reader.ReadUInt16BigEndian());
			}

			return entities;
		}

		private static MapEntity[] ReadCtl(string path) => ReadCtl(File.ReadAllBytes(path));

		IEnumerator<(MapTile tile, int x, int y)> IEnumerable<(MapTile tile, int x, int y)>.GetEnumerator()
		{
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
					yield return (this[x, y], x, y);
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<(MapTile, int, int)>)this).GetEnumerator();

	}
}
