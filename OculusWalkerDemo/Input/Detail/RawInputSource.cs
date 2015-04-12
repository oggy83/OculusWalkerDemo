using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.RawInput;
using SharpDX.Multimedia;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX;

namespace Oggy
{
	/// <summary>
	/// mouse and keyboard input source
	/// </summary>
	public class RawInputSource : IMouseKeyboardInputSource
	{
		#region properties

		private Vector2 m_mousePosDelta;
		public Vector2 MousePositionDelta
		{
			get
			{ 
				return m_mousePosDelta; 
			}
		}

		private int m_wheelDelta;
		public int WheelDelta
		{
			get
			{
				return m_wheelDelta;
			}
		}

		#endregion // properties

		#region event

		/// <summary>
		/// on mouse button down
		/// </summary>
		public event MouseEventHandler MouseDown = (sender, e) => {};

		/// <summary>
		/// on mouse button up
		/// </summary>
		public event MouseEventHandler MouseUp = (sender, e) => {};

		#endregion // event

		public RawInputSource(Panel renderTarget)
		{
			// Init Members
			m_mousePosDelta = new Vector2 { X=0, Y=0 };
			m_tmpMousePosDelta = new Vector2 { X = 0, Y = 0 };
			m_wheelDelta = 0;
			m_tmpWheelDelta = 0;
			m_renderTarget = renderTarget;
			m_renderTarget.MouseDown += (sender, e) => { MouseDown(sender, e); };
			m_renderTarget.MouseUp += (sender, e) => { MouseUp(sender, e); };

			// Register Mouse
			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
			Device.MouseInput += _OnMouseEvent;

			// Register Keyboard
			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
			Device.KeyboardInput += _OnKeyboardEvent;
		}

		public void Dispose()
		{
			// nothing
		}

		public void Update(double dT)
		{
			m_mousePosDelta = m_tmpMousePosDelta;
			m_tmpMousePosDelta = new Vector2 { X = 0, Y = 0 };
			m_wheelDelta = m_tmpWheelDelta;
			m_tmpWheelDelta = 0;
		}

		public bool TestKeyState(Keys key)
		{
			bool flag;
			if (!m_keyStateTable.TryGetValue(key, out flag))
			{
				// no entry
				return false;
			}

			return flag;
		}

		#region private method

		private void _OnMouseEvent(object sender, MouseInputEventArgs args)
		{
			//Trace.WriteLine("OnMouseEvent(x=" + args.X + ", y=" + args.Y + ", WheelDelta=" + args.WheelDelta + ")");
			m_tmpMousePosDelta.X += args.X;
			m_tmpMousePosDelta.Y += args.Y;
			m_tmpWheelDelta = args.WheelDelta;
		}

		private void _OnKeyboardEvent(object snder, KeyboardInputEventArgs args)
		{
			//Trace.WriteLine("OnKeyboardEvent(key=" + args.Key.ToString() + ", state=" + args.State.ToString() + ")");
			bool flag = args.State == KeyState.KeyDown;
			if (m_keyStateTable.ContainsKey(args.Key))
			{
				m_keyStateTable[args.Key] = flag;
			}
			else
			{
				m_keyStateTable.Add(args.Key, flag);
			}
		}

		#endregion // private method

		#region private members

		private Vector2 m_tmpMousePosDelta;
		private int m_tmpWheelDelta;
		private Dictionary<Keys, bool> m_keyStateTable = new Dictionary<Keys, bool>();

		/// <summary>
		/// render target panel
		/// </summary>
		/// <remarks>
		/// this class delegates mouse events of render target.
		/// </remarks>
		private Panel m_renderTarget;

		#endregion // private members

	}
}
