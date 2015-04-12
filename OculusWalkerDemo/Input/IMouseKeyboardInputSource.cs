using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using SharpDX;

namespace Oggy
{
	/// <summary>
	/// interface for basic input system (mouse, keyboard)
	/// </summary>
	public interface IMouseKeyboardInputSource : IDisposable
	{
		#region properties

		/// <summary>
		/// deference of mouse position from last frame [pixel]
		/// </summary>
		Vector2 MousePositionDelta { get; }

		int WheelDelta { get; }

		#endregion // properties

		#region event

		/// <summary>
		/// on mouse button down
		/// </summary>
		event MouseEventHandler MouseDown;

		/// <summary>
		/// on mouse button up
		/// </summary>
		event MouseEventHandler MouseUp;

		#endregion // event

		/// <summary>
		/// Check key state
		/// </summary>
		/// <param name="key">key code</param>
		/// <returns>
		/// true : key is down
		/// false : key is up
		/// </returns>
		bool TestKeyState(Keys key);
	}
}
