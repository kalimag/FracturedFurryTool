using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static BubsyParser.Tile;

namespace BubsyParser
{
	public class OverlayRenderer
	{

		private readonly GameData _gameData;
		private readonly Image _spawnGlyph;
		private readonly List<Image> _doorGlyphs;
		private readonly List<Image> _switchGlyphs;

		public Size DoorOffset { get; set; } = new Size(0, -100);
		public Size SwitchOffset { get; set; } = new Size(0, -48);
		public Size WallOffset { get; set; } = new Size(22, -48);
		public Size SpawnOffset { get; set; } = new Size(0, -32);
		//public float Radius { get; set; } = 32;
		//public Font Font { get; set; } = SystemFonts.CreateFont("Neo Sans", 64);



		public OverlayRenderer(GameData gameData, IEnumerable<Image> doorGlyphs, IEnumerable<Image> switchGlyphs, Image spawnGlyph)
		{
			_gameData = gameData;
			_spawnGlyph = spawnGlyph;
			_doorGlyphs = doorGlyphs.ToList();
			_switchGlyphs = switchGlyphs.ToList();
		}


		public void RenderOverlay(Image image, Map map, MapData mapData)
		{
			RenderDoors(image, mapData);
			RenderSwitches(image, map, mapData);
			RenderSpawn(image, mapData);
		}

		public Image<Rgba32> RenderOverlay(Map map, MapData mapData)
		{
			var image = CreateImage(map);
			RenderOverlay(image, map, mapData);
			return image;
		}



		public void RenderDoors(Image image, MapData mapData)
		{
			var visitedPoints = new HashSet<Point>();

			var balancedPairs = mapData.Doors.All(door1 => mapData.Doors.Count(door2 => door1.Entrance == door2.Exit) == 1);
			if (!balancedPairs)
			{
				Console.WriteLine($"{mapData.ctlName} has unbalanced doors");
				return;
			}

			int doorIndex = 0;
			foreach (var door in mapData.Doors)
			{
				if (!visitedPoints.Contains(door.Entrance))
				{
					RenderDoor(image, door.Entrance, doorIndex);
					RenderDoor(image, door.Exit, doorIndex);
					visitedPoints.Add(door.Entrance);
					visitedPoints.Add(door.Exit);
					doorIndex++;
				}
			}
		}

		public Image<Rgba32> RenderDoors(Map map, MapData mapData)
		{
			var image = CreateImage(map);
			RenderDoors(image, mapData);
			return image;
		}

		private void RenderDoor(Image image, Point position, int index)
		{
			RenderGlyph(image, _doorGlyphs[index], position + DoorOffset);
		}


		public void RenderSwitches(Image image, Map map, MapData mapData)
		{
			const uint SwitchJumpAddr = 0x188F6;

			var wallsToSwitchIndex = new Dictionary<uint, int>
			{
				[0x0001e296] = 0,
				[0x0001e2d4] = 1,
				[0x0001e2ec] = 2,
				[0x0001e304] = 3,
			};

			var walls = new List<(MapEntity wall, int switchIndex)>();

			foreach (var mapEntity in map.Entities)
			{
				var data = mapData.Entities[mapEntity.Id];

				if (data.JumpAddrA == SwitchJumpAddr)
					RenderSwitch(image, mapEntity, data);
				else if (wallsToSwitchIndex.TryGetValue(data.JumpAddrA, out var switchIndex))
					walls.Add((mapEntity, switchIndex));
			}

			foreach (var wallGroup in walls.GroupBy(wall => wall.switchIndex))
				RenderWalls(image, wallGroup.Select(w => w.wall), wallGroup.Key);
		}

		public Image<Rgba32> RenderSwitches(Map map, MapData mapData)
		{
			var image = CreateImage(map);
			RenderSwitches(image, map, mapData);
			return image;
		}

		private void RenderSwitch(Image image, MapEntity entity, Entity data)
		{
			int index = 0;
			uint flag = data.E0;
			while ((flag >>= 1) != 0)
				index++;

			RenderGlyph(image, _switchGlyphs[index], new Point(entity.X, entity.Y) + SwitchOffset);
		}

		private void RenderWalls(Image image, IEnumerable<MapEntity> walls, int switchIndex)
		{
			//var x = (int)Math.Round(walls.Average(w => w.X));
			//var y = (int)Math.Round(walls.Average(w => w.Y));

			var x = walls.Min(w => w.X);
			var y = walls.Min(w => w.Y);

			var glyph = _switchGlyphs[switchIndex];

			RenderGlyph(image, glyph, new Point(x, y) + WallOffset);
		}

		public void RenderSpawn(Image image, MapData mapData)
		{
			RenderGlyph(image, _spawnGlyph, mapData.Spawn + SpawnOffset);
		}

		private static void RenderGlyph(Image image, Image glyph, Point point)
		{
			var glyphOffset = glyph.Size() / -2;
			image.Mutate(dest => dest.DrawImage(glyph, point + glyphOffset, opacity: 1f));
		}


		private static Image<Rgba32> CreateImage(Map map) => new Image<Rgba32>(map.Width * TileWidth, map.Height * TileHeight);

	}
}
