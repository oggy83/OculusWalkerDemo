using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using SharpDX;
using System.Diagnostics;

namespace Oggy
{
	/// <summary>
	/// game pad input source
	/// </summary>
	public class XInputPadInputSource : IPadInputSource
	{
		#region properties

		/// <summary>
		/// left game pad stick input data
		/// </summary>
		private InputSystem.ThumbInput m_leftThumbInput;
		public InputSystem.ThumbInput LeftThumbInput
		{
			get
			{
				return m_leftThumbInput;
			}
		}

		/// <summary>
		/// left game pad stick spec data
		/// </summary>
		public InputSystem.ThumbSpec LeftThumbSpec
		{
			get;
			set;
		}

		/// <summary>
		/// right game pad stick input data
		/// </summary>
		private InputSystem.ThumbInput m_rightThumbInput;
		public InputSystem.ThumbInput RightThumbInput
		{
			get
			{
				return m_rightThumbInput;
			}
		}

		/// <summary>
		/// right game pad stick spec data
		/// </summary>
		public InputSystem.ThumbSpec RightThumbSpec
		{
			get;
			set;
		}

		#endregion // properties

		public XInputPadInputSource()
		{
			SelectPad();
			LeftThumbSpec = InputSystem.ThumbSpec.Default();
			RightThumbSpec = InputSystem.ThumbSpec.Default();
		}

		public bool TestButtonState(InputSystem.PadButtons button)
		{
			bool value = false;
			if (m_buttonStateTable.TryGetValue(button, out value))
			{
				return value;
			}
			else
			{
				return false;
			}
		}

		public void SelectPad()
		{
			var ctrlerList = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

			// select first available game pad
			var selectCtrler = ctrlerList.Where(pad => pad.IsConnected).First();
			if (selectCtrler != null)
			{
				m_selectCtrler = selectCtrler;
			}
		}

		public void Dispose()
		{
			// nothing
		}

		public void Update(double dT)
		{
			var pad = m_selectCtrler.GetState().Gamepad;

			m_leftThumbInput = _ComputeThumbInput(pad.LeftThumbX, pad.LeftThumbY, LeftThumbSpec);
			m_rightThumbInput = _ComputeThumbInput(pad.RightThumbX, pad.RightThumbY, RightThumbSpec);

			//Trace.WriteLine("LeftThumbInput(Raw=" + m_leftThumbInput.RawMagnitude + ", Magnitude =" + m_leftThumbInput.Magnitude + ", Normalized=" + m_leftThumbInput.NormalizedMagnitude + ")");
			//Trace.WriteLine("RightThumbInput(Raw=" + m_rightThumbInput.RawMagnitude + ", Magnitude =" + m_rightThumbInput.Magnitude + ", Normalized=" + m_rightThumbInput.NormalizedMagnitude + ")");

			// process pad button input
			var s_buttonTable = new Tuple<InputSystem.PadButtons, GamepadButtonFlags>[]
			{
				Tuple.Create(InputSystem.PadButtons.Down, GamepadButtonFlags.DPadDown),
				Tuple.Create(InputSystem.PadButtons.Up, GamepadButtonFlags.DPadUp),
				Tuple.Create(InputSystem.PadButtons.Left, GamepadButtonFlags.DPadLeft),
				Tuple.Create(InputSystem.PadButtons.Right, GamepadButtonFlags.DPadRight),
			};
			foreach (var button in s_buttonTable)
			{
				bool flag = ((int)pad.Buttons & (int)button.Item2) != 0;
				if (m_buttonStateTable.ContainsKey(button.Item1))
				{
					m_buttonStateTable[button.Item1] = flag;
				}
				else
				{
					m_buttonStateTable.Add(button.Item1, flag);
				}
			}
		}

		#region private members

		private Controller m_selectCtrler = null;
		private Dictionary<InputSystem.PadButtons, bool> m_buttonStateTable = new Dictionary<InputSystem.PadButtons, bool>();
		
		#endregion // private members

		#region private methods

		/// <summary>
		/// compute ThumbInput data from game pad input
		/// </summary>
		/// <param name="thumbX">game pad input about x coord</param>
		/// <param name="thumbY">game pad input about y coord</param>
		/// <param name="spec">input spec data</param>
		/// <returns>computed data</returns>
		private InputSystem.ThumbInput _ComputeThumbInput(short thumbX, short thumbY, InputSystem.ThumbSpec spec)
		{
			var input = new InputSystem.ThumbInput();
			input.Direction = new Vector2(thumbX, thumbY);
			input.RawMagnitude = input.Direction.Length();
			input.Direction.Normalize();

			if (input.RawMagnitude > spec.MinMagnitude)
			{
				input.Magnitude = Math.Min(input.RawMagnitude, spec.MaxMagnitude);
				input.Magnitude -= spec.MinMagnitude;
				input.NormalizedMagnitude = input.Magnitude / (spec.MaxMagnitude - spec.MinMagnitude);
			}
			else
			{
				input.Magnitude = 0;
				input.NormalizedMagnitude = 0;
			}

			return input;
		}

		#endregion // private methods
	}
}
