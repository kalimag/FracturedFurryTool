using System.Collections.Immutable;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BubsyParser
{
	public record Tile(int Id, Image<Rgba32> Image, TileFlag Flag, short Acceleration, ImmutableArray<byte> Heights)
	{
		public const int TileWidth = 16;
		public const int TileHeight = TileWidth;
	}

}
