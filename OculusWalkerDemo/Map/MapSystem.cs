﻿using System;
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

		public Size GetBlockSize()
		{
			return new Size(m_blockInfoMap.GetLength(0), m_blockInfoMap.GetLength(1));
		}

		public BlockInfo GetBlockInfo(int blockX, int blockY)
		{
			return m_blockInfoMap[blockY, blockX];
		}

		public MapLocation GetMapLocation(GameEntity entity)
		{
			return GetMapLocation(entity.FindComponent<LayoutComponent>().Transform.TranslationVector);
		}

		public MapLocation GetMapLocation(Vector3 v)
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
		}

		public void LoadResource()
		{
			_LoadParts(9000);// floor
			_LoadParts(9100);// closed gate
			_LoadParts(9200);// wall
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

			var drawModel = DrawModel.FromScene("p" + modelId + "/draw", scene);

			m_drawModelList.Add(new _ModelInfo() { Model = drawModel, ModelId = modelId });
			return drawModel;
		}

		#endregion // private methods

		#region private members

		private List<GameEntity> m_mapEntities = new List<GameEntity>();
		private List<_ModelInfo> m_drawModelList = new List<_ModelInfo>();
		private BlockInfo[,] m_blockInfoMap = null;

		#endregion // private members
	}
}
