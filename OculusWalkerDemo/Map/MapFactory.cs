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
			var layoutList = new List<LayoutInfo>();

			foreach (var block in BlockInfo.ToFlatArray(blockInfoMap))
			{
				var basePosition = new Vector3(block.BlockX * blockSize + origin.X, origin.Y, -block.BlockY * blockSize + origin.Z);

				switch (block.Type)
				{
					case BlockInfo.BlockTypes.Wall:
						layoutList.Add(new LayoutInfo()
						{
							ModelId = 9000,
							Layout = Matrix.Translation(basePosition)
						});
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
						}
						else
						{
							// y-axis direction gate
							layoutList.Add(new LayoutInfo()
							{
								ModelId = 9100,
								Layout = Matrix.Translation(basePosition)
							});
						}
						break;

					case BlockInfo.BlockTypes.Floor:
					case BlockInfo.BlockTypes.None:
					default:
						break;
				}
			}

			return layoutList;
		}
	}
}
