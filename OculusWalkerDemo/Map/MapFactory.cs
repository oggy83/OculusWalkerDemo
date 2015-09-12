using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
		/// create a set of block info from a map image
		/// </summary>
		/// <param name="mapSrcImage">path of map image</param>
		/// <returns>the created set of block info</returns>
		/// <remarks>
		/// block info represents 
		/// </remarks>
		public static BlockInfo[,] CreateBlockInfoMap(string mapSrcImage)
		{
			// create map block
			{
				// load map from image file
				var bitmap = (Bitmap)Image.FromFile("Image/testmap.png");
				int mapBlockWidth = bitmap.Width;
				int mapBlockHeight = bitmap.Height;

				var blockInfoMap = new BlockInfo[mapBlockHeight, mapBlockWidth];

				for (int i = 0; i < mapBlockHeight; ++i)
				{
					for (int j = 0; j < mapBlockWidth; ++j)
					{
						BlockInfo.BlockTypes type = BlockInfo.BlockTypes.None;

						System.Drawing.Color color = bitmap.GetPixel(j, i);
						if (color.R == 0 && color.G == 0 && color.B == 0)	
						{
							// black is wall
							type = BlockInfo.BlockTypes.Wall;
							
						}
						else if (color.R == 255 && color.G == 255 && color.B == 255)
						{ 
							// white is floor
							type = BlockInfo.BlockTypes.Floor;
						}

						blockInfoMap[i, j] = new BlockInfo(new BlockAddress(j, i), type);
					}
				}

				bitmap.Dispose();

				return blockInfoMap;
			}
		}

		public struct LayoutInfo
		{
			public int ModelId;
			public Matrix Layout;
		}

		public static List<LayoutInfo> CreateMapLayout(BlockInfo[,] blockInfo, Vector3 origin, float blockSize)
		{
			var layoutList = new List<LayoutInfo>();
			int height = blockInfo.GetLength(0);
			int width = blockInfo.GetLength(1);

			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					var basePosition = new Vector3(j * blockSize + origin.X, origin.Y, i * blockSize + origin.Z);

					var type = blockInfo[i, j].Type;
					switch (type)
					{
						case BlockInfo.BlockTypes.Wall :
							layoutList.Add(new LayoutInfo() 
							{
								ModelId = 9000,// m9000
								Layout = Matrix.Translation(basePosition)
							});
							break;

						case BlockInfo.BlockTypes.Floor:
						case BlockInfo.BlockTypes.None:
						default:
							break;
					}
				}
			}

			return layoutList;
		}
	}
}
