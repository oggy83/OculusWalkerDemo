using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class ChrSystem
	{
		#region static

		private static ChrSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new ChrSystem();
		}

		static public void Dispose()
		{
			// nothing
		}

		static public ChrSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region properties

		public PlayerEntity Player
		{
			get;
			set;
		}

		#endregion // properties

		public ChrSystem()
		{
			// nothing
		}

	}
}
