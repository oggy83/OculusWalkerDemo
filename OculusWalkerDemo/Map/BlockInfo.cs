﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class BlockInfo
	{
		#region public types

		public enum BlockTypes
		{
			None = 0,
			Wall,
			Floor,
			StartPoint,
			ClosedGate,
		}

		#endregion // public types

		#region properties

		private int m_blockX;
		public int BlockX
		{
			get
			{
				return m_blockX;
			}
		}

		private int m_blockY;
		public int BlockY
		{
			get
			{
				return m_blockY;
			}
		}

		public BlockInfo North;
		public BlockInfo South;
		public BlockInfo East;
		public BlockInfo West;

		private BlockTypes m_type;
		public BlockTypes Type
		{
			get
			{
				return m_type;
			}
		}


		#endregion // properties

		public BlockInfo(int blockX, int blockY, BlockTypes type)
		{
			m_type = type;
			m_blockX = blockX;
			m_blockY = blockY;
		}

		public bool CanWalkThrough()
		{
			return m_type != BlockTypes.Wall && m_type != BlockTypes.ClosedGate;
		}

		public bool CanWalkHalf()
		{
			return m_type == BlockTypes.ClosedGate;
		}

		/// <summary>
		/// get all connected block infos with this block
		/// </summary>
		/// <returns>
		/// list of connected block info
		/// </returns>
		public List<MapLocation.DirectionType> GetJointBlockInfos()
		{
			var dirList = new List<MapLocation.DirectionType>();
			if (North.CanWalkThrough())
			{
				dirList.Add(MapLocation.DirectionType.North);
			}
			if (South.CanWalkThrough())
			{
				dirList.Add(MapLocation.DirectionType.South);
			}
			if (East.CanWalkThrough())
			{
				dirList.Add(MapLocation.DirectionType.East);
			}
			if (West.CanWalkThrough())
			{
				dirList.Add(MapLocation.DirectionType.West);
			}

			return dirList;
		}

		public BlockInfo GetForwardBlockInfo(MapLocation.DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return North;
				case MapLocation.DirectionType.South:
					return South;
				case MapLocation.DirectionType.East:
					return East;
				case MapLocation.DirectionType.West:
					return West;
				default:
					return null;
			}
		}

		public BlockInfo GetBackwardBlockInfo(MapLocation.DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return South;
				case MapLocation.DirectionType.South:
					return North;
				case MapLocation.DirectionType.East:
					return West;
				case MapLocation.DirectionType.West:
					return East;
				default:
					return null;
			}
		}

		public BlockInfo GetLeftBlockInfo(MapLocation.DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return East;
				case MapLocation.DirectionType.South:
					return West;
				case MapLocation.DirectionType.East:
					return South;
				case MapLocation.DirectionType.West:
					return North;
				default:
					return null;
			}
		}

		public BlockInfo GetRightBlockInfo(MapLocation.DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return West;
				case MapLocation.DirectionType.South:
					return East;
				case MapLocation.DirectionType.East:
					return North;
				case MapLocation.DirectionType.West:
					return South;
				default:
					return null;
			}
		}

		public static IEnumerable<BlockInfo> ToFlatArray(BlockInfo[,] src)
		{
			int height = src.GetLength(0);
			int width = src.GetLength(1);

			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					yield return src[i, j];
				}
			}

			yield break;
		}
	}
}
