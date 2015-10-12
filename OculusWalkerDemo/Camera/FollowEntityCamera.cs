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
		private const float MinimumPadMagnitude = 0.15f;

		public FollowEntityCamera()
		{
			m_player = null;
			m_isEnableHeadRotation = false;
			m_headRotAngle = Vector3.Zero;
		}

		public void Update(double dt)
		{
			if (m_player == null)
			{
				return;
			}

			var layoutC = m_player.FindComponent<LayoutComponent>();
			Debug.Assert(layoutC!= null, "");

			Matrix headRotTrans = Matrix.Identity;
			if (m_isEnableHeadRotation)
			{
				

				var gameSys = GameSystem.GetInstance();
				switch (gameSys.Config.InputDevice)
				{
					case GameConfig.UserInputDevices.MouseKeyboard:
						break;

					case GameConfig.UserInputDevices.Pad:
						{
							IPadInputSource src = InputSystem.GetInstance().Pad;
							var thumbDir = src.RightThumbInput.Direction;
							float magnitude = src.RightThumbInput.NormalizedMagnitude;
							if (magnitude >= MinimumPadMagnitude)
							{
								float maxAngle = 0.9f;
								m_headRotAngle.X = Math.Min(Math.Max(m_headRotAngle.X + thumbDir.Y * (float)dt, -maxAngle), maxAngle);
								m_headRotAngle.Y = Math.Min(Math.Max(m_headRotAngle.Y + thumbDir.X * (float)dt, -maxAngle), maxAngle);
								m_headRotAngle.Z = 0;

							}
							else
							{
								m_headRotAngle *= 0.9f;
							}
						}
						break;
				}

				headRotTrans = Matrix.RotationYawPitchRoll(m_headRotAngle.Y, m_headRotAngle.X, m_headRotAngle.Z);
			}
			
			

			//var markerC = m_player.FindComponent<ModelMarkerComponent>();
			//Debug.Assert(markerC != null, "");
            //var mtx = markerC.FindMarkerMatrix(10) * layoutC.Transform;

            var mtx = headRotTrans * layoutC.Transform * Matrix.Translation(0, 1, 0);

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

			m_isEnableHeadRotation = !GameSystem.GetInstance().Config.IsUseHmd;
		}

		public void Deactivate()
		{
			// nothing
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

		private bool m_isEnableHeadRotation;

		private Vector3 m_headRotAngle;

		#endregion // private members
	}
}
