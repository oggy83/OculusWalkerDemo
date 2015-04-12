using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Oggy
{
	public interface ICamera
	{
		void Update(float dt);

		/// <summary>
		/// get camera data from current setting
		/// </summary>
		/// <returns>camera data</returns>
		DrawSystem.CameraData GetCameraData();

		/// <summary>
		/// begin to use this camera as the active camera
		/// </summary>
		/// <param name="oldCamera">old active camera</param>
		/// <remarks>This function is just called by CameraSystem.</remarks>
		void Activate(ICamera oldCamera);

		/// <summary>
		/// end to use this camera as the active camera
		/// </summary>
		void Deactivate();
	}
}
