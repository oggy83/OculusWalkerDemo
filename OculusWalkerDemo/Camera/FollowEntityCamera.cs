using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace Oggy
{
	public class FollowEntityCamera : ICamera
	{
		private const float Zoom = 10;

		public FollowEntityCamera()
		{
			m_player = null;
		}

		public void Update(double dt)
		{
			var layoutC = m_player.FindComponent<LayoutComponent>();
			Debug.Assert(layoutC!= null, "");

			//var markerC = m_player.FindComponent<ModelMarkerComponent>();
			//Debug.Assert(markerC != null, "");
            //var mtx = markerC.FindMarkerMatrix(10) * layoutC.Transform;

            var mtx = layoutC.Transform * Matrix.Translation(0, 1, 0);

			Vector3 eye, lookAt, up;
			eye = mtx.TranslationVector + Vector3.UnitY;
			lookAt = eye + mtx.Forward * Zoom;
			up = Vector3.UnitY;
			m_camera = new DrawSystem.CameraData(eye, lookAt, up);
		}

		public DrawSystem.CameraData GetCameraData()
		{
			return m_camera;
		}
		
		public void Activate(ICamera oldCamera)
		{
			m_player = ChrSystem.GetInstance().Player;
			Debug.Assert(m_player != null, "player must not be null");
		}

		public void Deactivate()
		{
			m_player = null;
		}

		#region private members


		/// <summary>
		/// target player
		/// </summary>
		private PlayerEntity m_player;

		/// <summary>
		/// camera data
		/// </summary>
		private DrawSystem.CameraData m_camera;

		#endregion // private members
	}
}
