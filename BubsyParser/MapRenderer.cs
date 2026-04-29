using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static BubsyParser.Tile;

namespace BubsyParser
{
	public class MapRenderer
	{
		private static readonly ConditionalWeakTable<Tile, Image<Rgba32>?> _collisionGlyphs = new();

		public Image<Rgba32> RenderComposite(Map map)
		{
			var image = CreateImage(map);
			RenderTiles(image, map, foreground: false);
			RenderTiles(image, map, foreground: true);
			return image;
		}

		private static Image<Rgba32> CreateImage(Map map) => new Image<Rgba32>(map.Width * TileWidth, map.Height * TileHeight);

		public void RenderTiles(Image image, Map map, bool foreground, float opacity = 1f)
		{
			image.Mutate(dest =>
			{
				for (int y = 0; y < map.Height; y++)
				{
					for (int x = 0; x < map.Width; x++)
					{
						var tile = foreground ? map[x, y].Foreground : map[x, y].Background;
						if (tile is not null)
							dest.DrawImage(tile.Image, new Point(x * TileWidth, y * TileHeight), opacity);
					}
				}
			});
		}

		public Image<Rgba32> RenderTiles(Map map, bool foreground, float opacity = 1f)
		{
			var image = CreateImage(map);
			RenderTiles(image, map, foreground, opacity);
			return image;
		}


		public void RenderCollision(Image image, Map map, float opacity = 1f)
		{
			image.Mutate(dest =>
			{
				for (int y = 0; y < map.Height; y++)
				{
					for (int x = 0; x < map.Width; x++)
					{
						var glyph = GetCollisionGlyph(map[x, y].Foreground);
						if (glyph is not null)
							dest.DrawImage(glyph, new Point(x * TileWidth, y * TileHeight), opacity);
					}
				}
			});
		}
		public Image<Rgba32> RenderCollision(Map map, float opacity = 1f)
		{
			var image = CreateImage(map);
			RenderCollision(image, map, opacity);
			return image;
		}

		private Image<Rgba32>? GetCollisionGlyph(Tile? tile)
		{
			if (tile is null)
				return null;
			else
				return _collisionGlyphs.GetValue(tile, CreateCollisionGlyph);
		}

		private Image<Rgba32>? CreateCollisionGlyph(Tile tile)
		{
			if (tile.Heights.Any(height => height > 0))
			{
				var image = new Image<Rgba32>(TileWidth, TileHeight);

				for (int tx = 0; tx < 16; tx++)
				{
					int height = tile.Heights[tx];

					var color = tile.Flag switch
					{
						TileFlag.None => Color.Gray,
						TileFlag.Solid => Color.Black,
						TileFlag.Water => Color.Blue,
						//TileFlag.Bounce => Color.Yellow,
						_ => throw new InvalidDataException($"Tile has collision and flag {tile.Flag}"),
					};

					if (height > 0)
						image[tx, TileHeight - height] = Color.HotPink;

					while (--height > 0)
						image[tx, TileHeight - height] = color;
				}

				return image;
			}
			else if (tile.Flag == TileFlag.Solid)
			{
				var image = new Image<Rgba32>(TileWidth, TileHeight);
				image.Mutate(dest => dest.Fill(Color.Black));
				return image;
			}
			else if (tile.Flag == TileFlag.Water)
			{
				var image = new Image<Rgba32>(TileWidth, TileHeight);
				image.Mutate(dest => dest.Fill(Color.Blue));
				return image;
			}
			else if (tile.Flag == TileFlag.Death)
			{
				var image = new Image<Rgba32>(TileWidth, TileHeight);
				image.Mutate(dest => dest.Fill(Color.Red));
				return image;
			}
			else if (tile.Flag == TileFlag.Bounce)
			{
				var image = new Image<Rgba32>(TileWidth, TileHeight);
				image.Mutate(dest => dest.Fill(Color.Yellow));
				return image;
			}
			else if (tile.Flag != TileFlag.None)
			{
				throw new InvalidDataException($"Unexpected tile flag {tile.Flag}");
			}
			else
			{
				return null;
			}
		}


	}
}
