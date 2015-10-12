using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace Oggy
{
	/// <summary>
	/// interface for game pad input system
	/// </summary>
	public interface IPadInputSource : IDisposable
	{
		#region properties

		/// <summary>
		/// left game pad stick input data
		/// </summary>
		InputSystem.ThumbInput LeftThumbInput { get; }

		/// <summary>
		/// left game pad stick spec data
		/// </summary>
		InputSystem.ThumbSpec LeftThumbSpec { get; set; }

		/// <summary>
		/// right game pad stick input data
		/// </summary>
		InputSystem.ThumbInput RightThumbInput { get; }

		/// <summary>
		/// right game pad stick spec data
		/// </summary>
		InputSystem.ThumbSpec RightThumbSpec { get; set; }

		#endregion // properties
		
		/// <summary>
		/// check button state
		/// </summary>
		/// <param name="button">button</param>
		/// <returns>
		/// true : button is down
		/// false : button is up
		/// </returns>
		bool TestButtonState(InputSystem.PadButtons button);
	}
}
