using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using System.Drawing;


namespace Oggy
{
	public partial class DrawSystem
	{
		public static Format GetRenderTargetFormat()
		{
			return Format.R8G8B8A8_UNorm;
			//return Format.R8G8B8A8_UNorm_SRgb;
		}
	}
}
