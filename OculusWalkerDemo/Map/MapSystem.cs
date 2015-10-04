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

		public BlockInfo GetBlockInfo(int blockX, int blockY)
		{
			return m_blockInfoMap[blockY, blockX];
		}

		public MapLocation GetBlockAddress(Vector3 v)
		{
			Vector3 bias = new Vector3(0.5f * BlockSize, 0.0f, -0.5f * BlockSize);

			var address = (v + bias) / BlockSize;
			return new MapLocation((int)address.X, -(int)address.Z);
		}

		public Vector3 GetDirection(MapLocation.DirectionType dir)
		{
			switch (dir)
			{
				case MapLocation.DirectionType.North:
					return Vector3.UnitZ;
				case MapLocation.DirectionType.South:
					return -Vector3.UnitZ;
				case MapLocation.DirectionType.West:
					return -Vector3.UnitX;
				case MapLocation.DirectionType.East:
					return Vector3.UnitX;
				default:
					return Vector3.UnitZ;
			}
		}

		public Matrix GetPose(MapLocation loc)
		{
			var pos = new Vector3(loc.BlockX * BlockSize, 0, -loc.BlockY * BlockSize);
			switch (loc.Position)
			{
				case MapLocation.PositionType.North:
					pos.Z += 0.5f * BlockSize;
					break;

				case MapLocation.PositionType.South:
					pos.Z -= 0.5f * BlockSize;
					break;

				case MapLocation.PositionType.West:
					pos.X -= 0.5f * BlockSize;
					break;

				case MapLocation.PositionType.East:
					pos.X += 0.5f * BlockSize;
					break;
			}
			var dir = GetDirection(loc.Direction);
			return MathUtil.GetRotationY(dir) * Matrix.Translation(pos);
		}

		public MapLocation? Walk(MapLocation currentLocation, MapLocation.DirectionType dir)
		{
			var currentBlockInfo = GetBlockInfo(currentLocation.BlockX, currentLocation.BlockY);
			var nextBlockInfo = currentBlockInfo.GetAdjacent(dir);

			bool canWalk = nextBlockInfo.CanWalkThrough();
			bool canWalkHalf = nextBlockInfo.CanWalkHalf();
			if (!canWalk && !canWalkHalf)
			{
				// can't walk
				return null;
			}

			if (currentBlockInfo.CanWalkHalf())
			{
				// can't walk central of current block
				switch (currentLocation.Position)
				{
					case MapLocation.PositionType.North :
						if (dir == MapLocation.DirectionType.South) return null;
						break;

					case MapLocation.PositionType.South :
						if (dir == MapLocation.DirectionType.North) return null;
						break;

					case MapLocation.PositionType.East :
						if (dir == MapLocation.DirectionType.West) return null;
						break;

					case MapLocation.PositionType.West:
						if (dir == MapLocation.DirectionType.East) return null;
						break;
				}
			}

			if (canWalk)
			{
				var result = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
				result.Direction = dir;
				return result;
			}
			else 
			{
				// can half walk
				var result = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
				result.SetPosition(dir);
				return result;
			}
		}

		public MapLocation GetStartInfo()
		{
			var result = new MapLocation(0, 0);

			foreach (var blockInfo in BlockInfo.ToFlatArray(m_blockInfoMap))
			{
				if (blockInfo.Type == BlockInfo.BlockTypes.StartPoint)
				{
					// found start point!
					result = new MapLocation(blockInfo.BlockX, blockInfo.BlockY);
					result.Direction = blockInfo.GetJointBlockInfos().First();// we assumes that start point has only one joint
					break;
				}
			}

			// not found
			return result;
		}

		public void CreateMap(string tmxPath)
		{
			m_blockInfoMap = MapFactory.CreateBlockInfoMap(tmxPath);
			int height = m_blockInfoMap.GetLength(0);
			int width = m_blockInfoMap.GetLength(1);

			var layoutList = MapFactory.CreateMapLayout(m_blockInfoMap, Vector3.Zero, BlockSize);
			foreach (var layout in layoutList)
			{
				var entity = new GameEntity("map");

				var layoutC = new LayoutComponent();
				layoutC.Transform = layout.Layout;
				entity.AddComponent(layoutC);

				var modelC = new ModelComponent(GameEntityComponent.UpdateLines.PreDraw);
				modelC.ModelContext.EnableCastShadow = true;
				modelC.ModelContext.DrawModel = m_drawModelList.Find((v) => v.ModelId == layout.ModelId).Model;
				//modelC.ModelContext.DebugModel = debugModel;
				entity.AddComponent(modelC);

				m_mapEntities.Add(entity);
			}

			// create floor entity
			var drawSys = DrawSystem.GetInstance();
			var floorModel = DrawModel.CreateFloor(height * BlockSize * 0.5f, 60.0f, Vector4.Zero);
			m_floor = new ModelEntity(new ModelEntity.InitParam()
			{
				Model = floorModel,
				Texture = new DrawSystem.TextureData
				{
					Resource = drawSys.ResourceRepository.FindResource<TextureView>("floor"),
					UvScale = Vector2.One
				},
				Layout = Matrix.Translation(BlockSize * 0.5f * width, 0, BlockSize * -0.5f * height),
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
			_LoadParts(9200);
			
			// load closed gate
			_LoadParts(9100);
		}

		public void UnloadResource()
		{
			foreach (var model in m_drawModelList)
			{
				model.Model.Dispose();
			}
			m_drawModelList.Clear();
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

		private DrawModel _LoadParts(int modelId)
		{
			var pathFormat = "Parts/p{0}/p{0}.blend";
			var scene = BlenderScene.FromFile(String.Format(pathFormat, modelId));

			var searchPathFormat = "Parts/p{0}";
			var drawModel = DrawModel.FromScene("p" + modelId + "/draw", scene, String.Format(searchPathFormat, modelId));

			m_drawModelList.Add(new _ModelInfo() { Model = drawModel, ModelId = modelId });
			return drawModel;
		}
				
		#endregion // private methods

		#region private members

		private ModelEntity m_floor = null;
		private List<GameEntity> m_mapEntities = new List<GameEntity>();
		private List<_ModelInfo> m_drawModelList = new List<_ModelInfo>();
		private BlockInfo[,] m_blockInfoMap = null;

		#endregion // private members
	}
}
