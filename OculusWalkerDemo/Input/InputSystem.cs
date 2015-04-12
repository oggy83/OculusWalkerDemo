using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Oggy
{
	/// <summary>
	/// This class propose a basic input system (Mouse, Keyboard)
	/// </summary>
	public partial class InputSystem
	{
		#region properties

		private RawInputSource m_rawInputSrc = null;
		public IMouseKeyboardInputSource MouseKeyboard
		{
			get
			{
				return m_rawInputSrc;
			}
		}

		private XInputPadInputSource m_padInputSrc = null;
		public IPadInputSource Pad
		{
			get
			{
				return m_padInputSrc;
			}
		}


		#endregion // properties

		#region static

		private static InputSystem s_singleton = null;

		static public void Initialize(Panel renderTarget)
		{
			s_singleton = new InputSystem(renderTarget);
		}

		static public void Dispose()
		{
			s_singleton.m_rawInputSrc.Dispose();
			s_singleton.m_padInputSrc.Dispose();
			s_singleton = null;
		}

		static public InputSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		public void Update(double dT)
		{
			m_rawInputSrc.Update(dT);
			m_padInputSrc.Update(dT);
		}

		private InputSystem(Panel renderTarget)
		{
			m_rawInputSrc = new RawInputSource(renderTarget);
			m_padInputSrc = new XInputPadInputSource();
		}

		#region private members

		

		#endregion // private members
	}
}
