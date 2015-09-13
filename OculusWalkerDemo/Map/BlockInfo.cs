using System;
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

		private BlockAddress m_addres;
		public BlockAddress Address
		{
			get
			{
				return m_addres;
			}
		}

		public BlockInfo Up;
		public BlockInfo Down;
		public BlockInfo Left;
		public BlockInfo Right;

		private BlockTypes m_type;
		public BlockTypes Type
		{
			get
			{
				return m_type;
			}
		}


		#endregion // properties

		public BlockInfo(BlockAddress address, BlockTypes type)
		{
			m_type = type;
			m_addres = address;
		}

		public bool CanWalk()
		{
			return m_type != BlockTypes.Wall && m_type != BlockTypes.ClosedGate;
		}

		/// <summary>
		/// get all connected block infos with this block
		/// </summary>
		/// <returns>
		/// list of connected block info
		/// </returns>
		public IEnumerable<BlockInfo> GetJointBlockInfos()
		{
			return new BlockInfo[] { Up, Down, Left, Right }.Where(v => v.CanWalk());
		}
	}
}
