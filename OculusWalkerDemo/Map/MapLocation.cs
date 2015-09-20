using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oggy
{
	public struct MapLocation
	{
		public enum PositionType
		{
			Center = 0,
			North,
			South,
			East,
			West,
		}

		public enum DirectionType
		{
			North,
			South,
			East,
			West,
		}

		private int m_blockX;
		public int BlockX
		{
			get
			{
				return m_blockX;
			}
			set
			{
				m_blockX = value;
			}
		}

		private int m_blockY;
		public int BlockY
		{
			get
			{
				return m_blockY;
			}
			set
			{
				m_blockY = value;
			}
		}

		private PositionType m_pos;
		public PositionType Position
		{
			get
			{
				return m_pos;
			}
			set
			{
				m_pos = value;
			}
		}

		private DirectionType m_dir;
		public DirectionType Direction
		{
			get
			{
				return m_dir;
			}
			set
			{
				m_dir = value;
			}
		}

		public MapLocation(int x, int y)
		{
			m_blockX = x;
			m_blockY = y;
			m_pos = PositionType.Center;
			m_dir = DirectionType.North;
		}

		public void SetCenter()
		{
			m_pos = PositionType.Center;
		}

		public static DirectionType GetForwardDirection(DirectionType forwardDir)
		{
			return forwardDir;
		}

		public static DirectionType GetBackwardDirection(DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return DirectionType.South;
				case MapLocation.DirectionType.South:
					return DirectionType.North;
				case MapLocation.DirectionType.East:
					return DirectionType.West;
				case MapLocation.DirectionType.West:
					return DirectionType.East;
				default:
					return DirectionType.North;
			}
		}

		public static DirectionType GetLeftDirection(DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return DirectionType.West;
				case MapLocation.DirectionType.South:
					return DirectionType.East;
				case MapLocation.DirectionType.East:
					return DirectionType.North;
				case MapLocation.DirectionType.West:
					return DirectionType.South;
				default:
					return DirectionType.North;
			}
		}

		public static DirectionType GetRightDirection(DirectionType forwardDir)
		{
			switch (forwardDir)
			{
				case MapLocation.DirectionType.North:
					return DirectionType.East;
				case MapLocation.DirectionType.South:
					return DirectionType.West;
				case MapLocation.DirectionType.East:
					return DirectionType.South;
				case MapLocation.DirectionType.West:
					return DirectionType.North;
				default:
					return DirectionType.North;
			}
		}
	}

}
