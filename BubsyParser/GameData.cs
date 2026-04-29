using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace BubsyParser
{
	public class GameData
	{
		private const int MapCount = 15;
		private const int GameTxtOffset = 0x4000;
		private const int GameDtaNameOffset = 0x2C2A8;



		public ImmutableArray<MapData> Maps { get; }

		private GameData(IEnumerable<MapData> maps)
		{
			Maps = maps.ToImmutableArray();
		}



		public static GameData FromData(ReadOnlySpan<byte> gameTxt, ReadOnlySpan<byte> gameDta)
		{
			var maps = new MapData[MapCount];

			var mapDataAddrReader = new SpanReader(gameTxt[0x7D36..]);
			for (int i = 0; i < MapCount; i++)
			{
				checked
				{
					var mapDataAddr = (int)mapDataAddrReader.ReadUInt32BigEndian() - GameTxtOffset;

					var mapDataReader = new SpanReader(gameTxt[mapDataAddr..]);

					int paletteNameAddr = (int)mapDataReader.ReadUInt32BigEndian() - GameDtaNameOffset;
					string paletteName = ReadNullTerminatedAscii(gameDta[paletteNameAddr..]);
					int ctlNameAddr = (int)mapDataReader.ReadUInt32BigEndian() - GameDtaNameOffset;
					string ctlName = ReadNullTerminatedAscii(gameDta[ctlNameAddr..]);
					int musicNameAddr = (int)mapDataReader.ReadUInt32BigEndian() - GameDtaNameOffset;
					string musicName = ReadNullTerminatedAscii(gameDta[musicNameAddr..]);

					mapDataReader.Skip(sizeof(uint) * 3);

					var entitiesAddr = (int)mapDataReader.ReadUInt32BigEndian() - GameTxtOffset;
					var entities = ReadEntities(gameTxt[entitiesAddr..]);

					var doorAddr = (int)mapDataReader.ReadUInt32BigEndian() - GameTxtOffset;
					var doors = ReadDoors(gameTxt[doorAddr..]);

					mapDataReader.Skip(sizeof(uint) * 3);

					short spawnX = mapDataReader.ReadInt16BigEndian();
					short spawnY = mapDataReader.ReadInt16BigEndian();

					maps[i] = new MapData(i, paletteName, ctlName, new Point(spawnX, spawnY), doors.ToImmutableArray(), entities.ToImmutableArray());
				}
			}


			return new GameData(maps);
		}

		public static GameData FromFile(string unpackedPath)
		{
			return FromData(
				File.ReadAllBytes(Path.Combine(unpackedPath, "GAME.TXT")),
				File.ReadAllBytes(Path.Combine(unpackedPath, "GAME.DTA"))
			);
		}


		private static List<Door> ReadDoors(ReadOnlySpan<byte> doorData)
		{
			var reader = new SpanReader(doorData);

			var doors = new List<Door>();
			while (true)
			{
				var entranceX = reader.ReadInt16BigEndian();
				if (entranceX < 0)
					break;
				var entranceY = reader.ReadInt16BigEndian();
				var exitX = reader.ReadInt16BigEndian();
				var exitY = reader.ReadInt16BigEndian();

				doors.Add(new Door(new(entranceX, entranceY), new(exitX, exitY)));
			}

			return doors;
		}
		private static List<Entity> ReadEntities(ReadOnlySpan<byte> data)
		{
			var reader = new SpanReader(data);

			var items = new List<Entity>();
			while (true)
			{
				short listEnd = reader.Clone().ReadInt16BigEndian();
				if (listEnd < 0)
					break;

				var a0 = reader.ReadByte();
				var a1 = reader.ReadByte();
				var jumpAddrA = reader.ReadUInt32BigEndian();
				var jumpAddrB = reader.ReadUInt32BigEndian();
				var jumpAddrC = reader.ReadUInt32BigEndian();
				var e0 = reader.ReadByte();
				var e1 = reader.ReadByte();
				var e2 = reader.ReadUInt16BigEndian();
				var f = reader.ReadByte();
				var g = reader.ReadByte();

				items.Add(new Entity(a0, a1, jumpAddrA, jumpAddrB, jumpAddrC, e0, e1, e2, f, g));
			}

			return items;
		}

		private static string ReadNullTerminatedAscii(ReadOnlySpan<byte> data)
		{
			var length = data.IndexOf<byte>(0);
			if (length < 0)
				throw new InvalidDataException("No null terminator found.");
			var str = Encoding.ASCII.GetString(data[0..length]);
			return str;
		}

	}

	public record MapData(int index, string paletteName, string ctlName, Point Spawn, ImmutableArray<Door> Doors, ImmutableArray<Entity> Entities);
	public record Door(Point Entrance, Point Exit);
	public record Entity(byte A0, byte A1, uint JumpAddrA, uint JumpAddrB, uint JumpAddrC, byte E0, byte E1, ushort E2, byte F, byte G);
}
