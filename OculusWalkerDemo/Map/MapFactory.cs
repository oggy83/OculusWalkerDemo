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

							var northBlock = new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.Translation(basePosition + new Vector3(0, 0, halfBlockSize))
							};
							var southBlock = new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(MathUtil.PI) * Matrix.Translation(basePosition + new Vector3(0, 0, -halfBlockSize))
							};
							var eastBlock = new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition + new Vector3(halfBlockSize, 0, 0))
							};
							var westBlock = new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(-MathUtil.PI / 2) * Matrix.Translation(basePosition + new Vector3(-halfBlockSize, 0, 0))
							};

							// wall 1-direction 
							if (!bNorth && bSouth && bEast && bWest)
							{
								layoutList.Add(northBlock);
							}
							else if (bNorth && !bSouth && bEast && bWest)
							{
								layoutList.Add(southBlock);
							}
							else if (bNorth && bSouth && !bEast && bWest)
							{
								layoutList.Add(eastBlock);
							}
							else if (bNorth && bSouth && bEast && !bWest)
							{
								layoutList.Add(westBlock);
							}

							// wall 2-direction 
							if (!bNorth && !bSouth && bEast && bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(southBlock);
							}
							else if (!bNorth && bSouth && !bEast && bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(eastBlock);
							}
							else if (!bNorth && bSouth && bEast && !bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(westBlock);
							}
							else if (bNorth && !bSouth && !bEast && bWest)
							{
								layoutList.Add(southBlock);
								layoutList.Add(eastBlock);
							}
							else if (bNorth && !bSouth && bEast && !bWest)
							{
								layoutList.Add(southBlock);
								layoutList.Add(westBlock);
							}
							else if (bNorth && bSouth && !bEast && !bWest)
							{
								layoutList.Add(eastBlock);
								layoutList.Add(westBlock);
							}

							// wall 3-direction 
							if (bNorth && !bSouth && !bEast && !bWest)
							{
								layoutList.Add(southBlock);
								layoutList.Add(eastBlock);
								layoutList.Add(westBlock);
							}
							else if (!bNorth && bSouth && !bEast && !bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(eastBlock);
								layoutList.Add(westBlock);
							}
							else if (!bNorth && !bSouth && bEast && !bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(southBlock);
								layoutList.Add(westBlock);
							}
							else if (!bNorth && !bSouth && !bEast && bWest)
							{
								layoutList.Add(northBlock);
								layoutList.Add(southBlock);
								layoutList.Add(eastBlock);
							}
						}
						break;

					case BlockInfo.BlockTypes.ClosedGate:
						if (block.East.CanWalkThrough())
						{
							// x-axis direction gate
							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9100,
								Layout = Matrix.RotationY(MathUtil.PI * 0.5f) * Matrix.Translation(basePosition)
							});

							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.Translation(basePosition + new Vector3(0, 0, halfBlockSize))
							});

							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(MathUtil.PI) * Matrix.Translation(basePosition + new Vector3(0, 0, -halfBlockSize))
							});
						}
						else
						{
							// y-axis direction gate
							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9100,
								Layout = Matrix.Translation(basePosition)
							});

							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(MathUtil.PI / 2) * Matrix.Translation(basePosition + new Vector3(halfBlockSize, 0, 0))
							});

							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9200,
								Layout = Matrix.RotationY(-MathUtil.PI / 2) * Matrix.Translation(basePosition + new Vector3(-halfBlockSize, 0, 0))
							});
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
	}
}
