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
		private const float BlockSize = 10.0f;

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

		#region properties

		#endregion // properties

		public BlockInfo GetBlockInfo(BlockAddress address)
		{
			return m_blockInfoMap[address.X, address.Y];
		}

		public BlockAddress GetBlockAddress(Vector3 v)
		{
			// temporary code
			int height = m_blockInfoMap.GetLength(0);
			int width = m_blockInfoMap.GetLength(1);
			Vector3 origin = new Vector3((float)width * -0.5f * BlockSize, 0.0f, (float)height * -0.5f * BlockSize);
			Vector3 bias = new Vector3(0.5f * BlockSize, 0.0f, 0.5f * BlockSize);

			var address = (v - origin + bias) / BlockSize;
			return new BlockAddress((int)address.X, (int)address.Z);
		}

		public Matrix GetStartPose()
		{
			Vector3 startPos = Vector3.Zero;
			Vector3 startDir = Vector3.UnitZ;

			int width = m_blockInfoMap.GetLength(1);
			int height = m_blockInfoMap.GetLength(0);
			foreach (var blockInfo in BlockInfo.ToFlatArray(m_blockInfoMap))
			{
				if (blockInfo.Type == BlockInfo.BlockTypes.StartPoint)
				{
					// found start point!
					Vector3 origin = new Vector3((float)width * -0.5f * BlockSize, 0.0f, (float)height * -0.5f * BlockSize);
					startPos = new Vector3(BlockSize * blockInfo.Address.X + origin.X, 0, BlockSize * blockInfo.Address.Y + origin.Z);

					BlockAddress nextBlockAddr = blockInfo.GetJointBlockInfos().First().Address;// we assumes that start point has only one joint
					startDir = new Vector3(nextBlockAddr.X - blockInfo.Address.X, 0, nextBlockAddr.Y - blockInfo.Address.Y);
					startDir.Normalize();
				}
			}

			return MathUtil.GetRotationY(startDir) * Matrix.Translation(startPos);
		}

		public void CreateMap(string tmxPath)
		{
			
			m_blockInfoMap = MapFactory.CreateBlockInfoMap(tmxPath);
			int height = m_blockInfoMap.GetLength(0);
			int width = m_blockInfoMap.GetLength(1);
			Vector3 origin = new Vector3((float)width * -0.5f * BlockSize, 0.0f, (float)height * -0.5f * BlockSize);

			var layoutList = MapFactory.CreateMapLayout(m_blockInfoMap, origin, BlockSize);
			foreach (var layout in layoutList)
			{
				var entity = new GameEntity("map");

				var layoutC = new LayoutComponent();
				layoutC.Transform = layout.Layout;
				entity.AddComponent(layoutC);

				var modelC = new ModelComponent();
				modelC.ModelContext.EnableCastShadow = true;
				modelC.ModelContext.DrawModel = m_drawModelList.Find((v) => v.ModelId == layout.ModelId).Model;
				//modelC.ModelContext.DebugModel = debugModel;
				entity.AddComponent(modelC);

				m_mapEntities.Add(entity);
			}

			// create floor entity
			var drawSys = DrawSystem.GetInstance();
			var floorModel = DrawModel.CreateFloor(height * 5, 60.0f, Vector4.Zero);
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
			m_drawModelList.Add(new _ModelInfo() { Model = floorModel, ModelId = -1 });
		}

		public void LoadResource()
		{
			// load wall
			{
				var path = "Map/m9000/m9000.blend";
				var searchPath = "Map/m9000";
				var scene = BlenderScene.FromFile(path);
				m_wallModel = DrawModel.FromScene(path + "/draw", scene, searchPath);

				m_drawModelList.Add(new _ModelInfo() { Model = m_wallModel, ModelId = 9000 });
			}
			
			// load closed gate
			{
				var path = "Map/m9100/m9100.blend";
				var searchPath = "Map/m9100";
				var scene = BlenderScene.FromFile(path);
				m_wallModel = DrawModel.FromScene(path + "/draw", scene, searchPath);

				m_drawModelList.Add(new _ModelInfo() { Model = m_wallModel, ModelId = 9100 });
			}
		}

		public void UnloadResource()
		{
			foreach (var model in m_drawModelList)
			{
				model.Model.Dispose();
			}
			m_drawModelList.Clear();
			m_wallModel = null;
		}

		public void Update(double dt, IDrawContext context)
		{
			m_floor.Draw(context);
		}

		#region private types

		private struct _ModelInfo
		{
			public DrawModel Model;
			public int ModelId;	// -1 is floor
		}

		#endregion // private tyeps

		#region private methods

		private MapSystem()
		{
			// @todo
		}
				
		#endregion // private methods

		#region private members

		private DrawModel m_wallModel = null;
		//private DrawModel m_floorModel = null;

		private ModelEntity m_floor = null;
		private List<GameEntity> m_mapEntities = new List<GameEntity>();
		private List<_ModelInfo> m_drawModelList = new List<_ModelInfo>();
		private BlockInfo[,] m_blockInfoMap = null;

		#endregion // private members
	}
}
