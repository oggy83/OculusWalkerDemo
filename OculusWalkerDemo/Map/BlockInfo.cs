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

		private BlockTypes m_types;
		public BlockTypes Type
		{
			get
			{
				return m_types;
			}
		}


		#endregion // properties

		public BlockInfo(BlockAddress address, BlockTypes type)
		{
			m_types = type;
			m_addres = address;
		}
	}
}
