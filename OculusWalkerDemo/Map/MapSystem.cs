using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
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
	public class MapSystem
	{
		#region static

		private static MapSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new MapSystem();
		}

		static public void Dispose()
		{
			if (s_singleton == null)
			{
				return;
			}

			foreach (var entity in s_singleton.m_mapEntities)
			{
				entity.Dispose();
			}

			s_singleton.m_floor.Dispose();
			s_singleton.UnloadResource();
			

			s_singleton = null;
		}

		static public MapSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		public void CreateMap(string mapSrcImage)
		{
			// create map block
			{
				// load map from image file
				var bitmap = (Bitmap)Image.FromFile("Image/testmap.png");
				int mapBlockWidth = bitmap.Width;
				int mapBlockHeight = bitmap.Height;

				for (int i = 0; i < mapBlockHeight; ++i)
				{
					for (int j = 0; j < mapBlockWidth; ++j)
					{
						System.Drawing.Color color = bitmap.GetPixel(j, i);
						if (color.R == 0 && color.G == 0 && color.B == 0)	// black is wall
						{
							var v = new Vector3((j - (float)mapBlockWidth * 0.5f) * 10, 0, (i - (float)mapBlockHeight * 0.5f) * 10);
							var entity = new GameEntity("wall");

							var layoutC = new LayoutComponent();
							layoutC.Transform = Matrix.Translation(v);
							entity.AddComponent(layoutC);

							var modelC = new ModelComponent();
							modelC.ModelContext.EnableCastShadow = true;
							modelC.ModelContext.DrawModel = m_wallModel;
							entity.AddComponent(modelC);

							m_mapEntities.Add(entity);
						}
					}
				}

				// create floor entity
				var drawSys = DrawSystem.GetInstance();
				var floorModel = DrawModel.CreateFloor(mapBlockHeight * 5, 60.0f, Vector4.Zero);
				m_floor = new ModelEntity(new ModelEntity.InitParam()
				{
					Model = floorModel,
					Texture = new DrawSystem.TextureData
					{
						Resource = drawSys.ResourceRepository.FindResource<TextureView>("floor"),
						UvScale = Vector2.One
					},
					Layout = Matrix.Identity,
					Delay = 0.0f,
					Forward = Vector3.Zero,
					Color = Color4.White,
					Speed = 1,
				});
				m_drawModelList.Add(floorModel);


				bitmap.Dispose();
			}
		}

		public void LoadResource()
		{
			var path = "Map/m9000/m9000.blend";
			var searchPath = "Map/m9000";
			var scene = BlenderScene.FromFile(path);
			m_wallModel = DrawModel.FromScene(path + "/draw", scene, searchPath);

			m_drawModelList.Add(m_wallModel);
		}

		public void UnloadResource()
		{
			foreach (var model in m_drawModelList)
			{
				model.Dispose();
			}
			m_drawModelList.Clear();
			m_wallModel = null;
		}

		public void Update(double dt, IDrawContext context)
		{
			m_floor.Draw(context);
		}

		#region private methods

		private MapSystem()
		{
			
		}

		
		#endregion // private methods

		#region private members

		private DrawModel m_wallModel = null;
		//private DrawModel m_floorModel = null;

		private ModelEntity m_floor = null;
		private List<GameEntity> m_mapEntities = new List<GameEntity>();
		private List<DrawModel> m_drawModelList = new List<DrawModel>();

		#endregion // private members
	}
}
