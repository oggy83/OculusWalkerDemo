using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
	public class GameConfig
	{
		/// <summary>
		/// input device used by user
		/// </summary>
		public enum UserInputDevices
		{
			MouseKeyboard,
			Pad
		}

		#region properties

		public UserInputDevices InputDevice
		{
			get;
			set;
		}

		#endregion // properties

		public GameConfig()
		{
			InputDevice = UserInputDevices.Pad;
		}

		[Conditional("DEBUG")]
		public void CreateDebugMenu(ToolStripMenuItem parent)
		{
           
			var item1 = new ToolStripRadioButtonMenuItem("MouseKeyboard");
			item1.Click += (sender, e) => { InputDevice = UserInputDevices.MouseKeyboard; };
			var item2 = new ToolStripRadioButtonMenuItem("GamePad");
			item2.Click += (sender, e) => { InputDevice = UserInputDevices.Pad; };
            parent.DropDownItems.AddRange(new ToolStripItem[] { item1, item2 });

			item2.Checked = true;
		}
	}
}
