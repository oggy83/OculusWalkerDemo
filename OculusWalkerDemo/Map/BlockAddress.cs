using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oggy
{
	public struct BlockAddress
	{
		public int X;
		public int Y;

		public BlockAddress(int x, int y)
		{
			X = x;
			Y = y;
		}

	}
}
