using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace Oggy
{
	public class MapFactory
	{
		/// <summary>
		/// create a set of block info from .tmx file
		/// </summary>
		/// <param name="tmx path">path of tmx path</param>
		/// <returns>the created set of block info</returns>
		/// <remarks>
		/// .tmx file is the output format of Tiled Map Editor (http://www.mapeditor.org/)
		/// block info represents 
		/// </remarks>
		public static BlockInfo[,] CreateBlockInfoMap(string tmxPath)
		{
			// create map block
			{
				var doc = XElement.Load(tmxPath);
				int mapBlockWidth = int.Parse(doc.Attribute("tilewidth").Value);
				int mapBlockHeight = int.Parse(doc.Attribute("tileheight").Value);
				var tileList = doc.Descendants("layer").Descendants("data").Descendants("tile").ToList();

				var blockInfoMap = new BlockInfo[mapBlockHeight, mapBlockWidth];

				// create blockInfo(s)
				for (int tileIndex = 0; tileIndex < tileList.Count(); ++tileIndex)
				{
					var tile = tileList[tileIndex];
					int gid = int.Parse(tile.Attribute("gid").Value);

					int i = tileIndex / mapBlockHeight;
					int j = tileIndex % mapBlockHeight;

					BlockInfo.BlockTypes type = BlockInfo.BlockTypes.None;

					if (gid == 3)
					{
						// wall
						type = BlockInfo.BlockTypes.Wall;
					}
					else if (gid == 9)
					{
						// floor
						type = BlockInfo.BlockTypes.Floor;
					}
					else if (gid == 49)
					{
						// closed gate
						type = BlockInfo.BlockTypes.ClosedGate;
					}
					else if (gid == 57)
					{
						// start point
						type = BlockInfo.BlockTypes.StartPoint;
					}

					blockInfoMap[i, j] = new BlockInfo(j, i, type);
				}

				// connect 
				for (int i = 0; i < mapBlockHeight; ++i)
				{
					for (int j = 0; j < mapBlockWidth; ++j)
					{
						if (i > 0)
						{
							blockInfoMap[i, j].North = blockInfoMap[i - 1, j];
						}

						if (i < mapBlockHeight - 1)
						{
							blockInfoMap[i, j].South = blockInfoMap[i + 1, j];
						}

						if (j > 0)
						{
							blockInfoMap[i, j].West = blockInfoMap[i, j - 1];
						}

						if (j < mapBlockWidth - 1)
						{
							blockInfoMap[i, j].East = blockInfoMap[i, j + 1];
						}
					}
				}

				return blockInfoMap;
			}
		}

		public struct LayoutInfo
		{
			public int ModelId;
			public Matrix Layout;
		}

		public static List<LayoutInfo> CreateMapLayout(BlockInfo[,] blockInfoMap, Vector3 origin, float blockSize)
		{
			var closedGateLayout = _LoadLayout(9000);
			var floorLayout1 = _LoadLayout(9010);// 1 way
			var floorLayout2s = _LoadLayout(9020);// 2 way (straight)
			var floorLayout2b = _LoadLayout(9030);// 2 way (bent)
			var floorLayout3 = _LoadLayout(9040);// 3 way
			var floorLayout = _LoadLayout(9050);// floor


			float halfBlockSize = blockSize * 0.5f;
			var layoutList = new List<LayoutInfo>();

			foreach (var block in BlockInfo.ToFlatArray(blockInfoMap))
			{
				var basePosition = new Vector3(block.BlockX * blockSize + origin.X, origin.Y, -block.BlockY * blockSize + origin.Z);

				switch (block.Type)
				{
					case BlockInfo.BlockTypes.Floor:
					case BlockInfo.BlockTypes.StartPoint:
						{
							bool bNorth = block.North.CanWalkThrough() || block.North.CanWalkHalf();
							bool bSouth = block.South.CanWalkThrough() || block.South.CanWalkHalf();
							bool bEast = block.East.CanWalkThrough() || block.East.CanWalkHalf();
							bool bWest = block.West.CanWalkThrough() || block.West.CanWalkHalf();


							// 1 way floor
							if (!bNorth && bSouth && !bEast && !bWest)
							{
								var offset = Matrix.Translation(basePosition);
								_AppendParts(floorLayout1, offset, layoutList);
							}
							else if (bNorth && !bSouth && !bEast && !bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout1, offset, layoutList);
							}
							else if (!bNorth && !bSouth && bEast && !bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout1, offset, layoutList);
							}
							else if (!bNorth && !bSouth && !bEast && bWest)
							{
								var offset = Matrix.RotationY(-MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout1, offset, layoutList);
							}

							// 2 way floor
							if (!bNorth && !bSouth && bEast && bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout2s, offset, layoutList);
							}
							else if (!bNorth && bSouth && !bEast && bWest)
							{
								var offset = Matrix.Translation(basePosition);
								_AppendParts(floorLayout2b, offset, layoutList);
							}
							else if (!bNorth && bSouth && bEast && !bWest)
							{
								var offset = Matrix.RotationY(-MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout2b, offset, layoutList);
							}
							else if (bNorth && !bSouth && !bEast && bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout2b, offset, layoutList);
							}
							else if (bNorth && !bSouth && bEast && !bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout2b, offset, layoutList);
							}
							else if (bNorth && bSouth && !bEast && !bWest)
							{
								var offset = Matrix.Translation(basePosition);
								_AppendParts(floorLayout2s, offset, layoutList);
							}

							// 3 way floor
							if (!bNorth && bSouth && bEast && bWest)
							{
								var offset = Matrix.Translation(basePosition);
								_AppendParts(floorLayout3, offset, layoutList);
							}
							else if (bNorth && !bSouth && bEast && bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout3, offset, layoutList);
							}
							else if (bNorth && bSouth && !bEast && bWest)
							{
								var offset = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout3, offset, layoutList);
							}
							else if (bNorth && bSouth && bEast && !bWest)
							{
								var offset = Matrix.RotationY(-MathUtil.PI / 2) * Matrix.Translation(basePosition);
								_AppendParts(floorLayout3, offset, layoutList);
							}

							if (bNorth && bSouth && bEast && bWest)
							{
								var offset = Matrix.Translation(basePosition);
								_AppendParts(floorLayout, offset, layoutList);
							}
						}
						break;

					case BlockInfo.BlockTypes.ClosedGate:
						if (block.East.CanWalkThrough())
						{
							// x-axis direction gate
							var offset = Matrix.RotationY(MathUtil.PI * 0.5f) * Matrix.Translation(basePosition);
							_AppendParts(closedGateLayout, offset, layoutList);
						}
						else
						{
							// y-axis direction gate
							var offset = Matrix.Translation(basePosition);
							_AppendParts(closedGateLayout, offset, layoutList);
						}
						break;

					case BlockInfo.BlockTypes.Wall:
					case BlockInfo.BlockTypes.None:
					default:
						break;
				}
			}

			return layoutList;
		}

		private static MapLayoutResource _LoadLayout(int id)
		{
			var pathFormat = "Map/m{0}/m{0}.blend";
			var scene = BlenderScene.FromFile(String.Format(pathFormat, id));
			return MapLayoutResource.FromScene("parts", scene);
		}

		/// <summary>
		/// append map parts to layout list
		/// </summary>
		/// <param name="res"></param>
		/// <param name="offset"></param>
		/// <param name="targetList"></param>
		private static void _AppendParts(MapLayoutResource res, Matrix offset, List<LayoutInfo> targetList)
		{
			foreach (var entry in res.Entries)
			{
				targetList.Add(new LayoutInfo()
				{
					ModelId = entry.ModelId,
					Layout = entry.Layout * offset
				});
			}
		}
	}
}
