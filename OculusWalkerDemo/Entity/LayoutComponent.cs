using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;


namespace Oggy
{
	/// <summary>
	/// This class represents a layout (position, rotation, and scale).
	/// </summary>
	/// <remarks>any 3d-object should have this compoment</remarks>
	public class LayoutComponent : GameEntityComponent
	{
		/// <summary>
		/// get/set layout as matrix
		/// </summary>
		public Matrix Transform { get; set; }

		public LayoutComponent()
		: base(GameEntityComponent.UpdateLines.Invalid)
		{
			Transform = Matrix.Identity;
		}
	}
}
