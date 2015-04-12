using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
	public class GameSystem
	{
		#region static

		private static GameSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new GameSystem();
		}

		static public void Dispose()
		{
			// nothing
		}

		static public GameSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region properties

		public GameConfig Config
		{
			get;
			set;
		}

		#endregion // properties

		public GameSystem()
		{
			Config = new GameConfig();
		}

		[Conditional("DEBUG")]
		public void CreateDebugMenu(ToolStripMenuItem parent)
		{
			Config.CreateDebugMenu(parent);
		}

	}
}
