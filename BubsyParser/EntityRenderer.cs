using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using IO = System.IO;
using static BubsyParser.Tile;

namespace BubsyParser
{
	public class EntityRenderer : IEnumerable<(int id, Image image, Size offset)>
	{

		private static readonly Font DebugFont = SystemFonts.CreateFont("CG pixel 3x5", 10);



		private Dictionary<int, EntityInfo> _entities = new();
		private HashSet<int> _unknownIds = new();


		public EntityRenderer()
		{
			_entities = new();
		}
		public EntityRenderer(EntityRenderer original)
		{
			_entities = new(original._entities);
		}



		public void Add(int id, Image image, int offsetX = 0, int offsetY = 0, string? name = null)
		{
			_entities.Add(id, new(id, image, new Size(offsetX, offsetY), name));
		}

		public void Add(int id, string path, int offsetX = 0, int offsetY = 0)
		{
			var image = Image.Load<Rgba32>(path);
			Add(id, image, offsetX, offsetY, IO.Path.GetFileNameWithoutExtension(path));
		}



		public void RenderEntities(Image image, Map map, float opacity = 1f)
		{
			_unknownIds.Clear();
			Debug.Print($"Render entities for {map.Name}...");
			foreach (var entity in map.Entities)
				RenderEntity(image, entity, opacity);
		}

		public Image<Rgba32> RenderEntities(Map map, float opacity = 1f)
		{
			var image = CreateImage(map);
			RenderEntities(image, map, opacity);
			return image;
		}

		public void RenderEntity(Image image, MapEntity entity, float opacity = 1f)
		{
			var position = new Point(entity.X, entity.Y);

			if (_entities.TryGetValue(entity.Id, out var entityImage))
				image.Mutate(dest => dest.DrawImage(entityImage.Image, position + entityImage.Offset, opacity));
			//else if (_unknownIds.Add(entity.Id))
			//	Debug.Print($"Unknown entity id: {entity.Id}");
		}



		public void RenderEntityIds(Image image, Map map)
		{
			Debug.Print($"Render entities for {map.Name}...");
			foreach (var entity in map.Entities)
				RenderEntityId(image, entity);
		}

		public Image<Rgba32> RenderEntityIds(Map map)
		{
			var image = CreateImage(map);
			RenderEntityIds(image, map);
			return image;
		}

		public void RenderEntityId(Image image, MapEntity entity)
		{
			var position = new Point(entity.X, entity.Y);
			var color = _entities.ContainsKey(entity.Id) ? Color.Blue : Color.Red;
			image.Mutate(dest => dest.DrawText(entity.Id.ToString(), DebugFont, color, position));
		}



		private static Image<Rgba32> CreateImage(Map map) => new Image<Rgba32>(map.Width * TileWidth, map.Height * TileHeight);

		public string? GetName(int id)
		{
			if (_entities.TryGetValue(id, out var entity) && !string.IsNullOrEmpty(entity.Name))
				return entity.Name;
			else
				return $"Unknown [{id}]";
		}

		public bool IsDefined(int id) => _entities.ContainsKey(id);


		IEnumerator<(int id, Image image, Size offset)> IEnumerable<(int id, Image image, Size offset)>.GetEnumerator()
		{
			foreach (var entity in _entities.Values)
				yield return (entity.Id, entity.Image, entity.Offset);
		}
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<(int id, Image image, Size offset)>)this).GetEnumerator();


		private record EntityInfo(int Id, Image Image, Size Offset, string? Name = null);
	}
}
