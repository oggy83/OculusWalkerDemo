using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
	/// <summary>
	/// Camera for debug
	/// </summary>
	public class FreeCamera : ICamera
	{
		private const float MaxZoom = 50;
		private const float MinZoom = 1;
		private const float ZoomFactor = 0.01f;
		private const float AngleFactor = 0.01f;
		private const float PositionFactor = 0.05f;

		#region properties

		/// <summary>
		/// get/set a distance eye from lookAt
		/// </summary>
		public float Zoom
		{
			get;
			set;
		}

		/// <summary>
		/// get/set a lookAt vector
		/// </summary>
		public Vector3 LookAt
		{
			get
			{
				return m_cameraTrans.TranslationVector;
			}
		}


		#endregion // properties

		public FreeCamera()
		{
			Zoom = 10;
			m_isLeftDrugging = false;
			m_isRightDrugging = false;
			m_cameraTrans = Matrix.Translation(0, 4, 0);
		}

		public void Update(double dt)
		{
			IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;

			int dWheel = src.WheelDelta;
			Zoom = MathUtil.Clamp(Zoom + dWheel * -ZoomFactor, MinZoom, MaxZoom);
			Vector2 dPos = src.MousePositionDelta;

			if (m_isLeftDrugging)
			{
				// update angle
				float hAngle = dPos.X * AngleFactor;
				float vAngle = dPos.Y * AngleFactor;
				m_cameraTrans = Matrix.RotationYawPitchRoll(hAngle, vAngle, 0) * m_cameraTrans;
			}

			if (m_isRightDrugging)
			{
				// update look at position
				float xOffset = -dPos.X * PositionFactor;
				float yOffset = dPos.Y * PositionFactor;
				m_cameraTrans = Matrix.Translation(xOffset, yOffset, 0) * m_cameraTrans;
			}
		}
		
		public DrawSystem.CameraData GetCameraData()
		{
			Vector3 eye, lookAt, up;
			lookAt = LookAt;
			eye = m_cameraTrans.Backward * -Zoom + lookAt;
			up = m_cameraTrans.Up;
			return new DrawSystem.CameraData(eye, lookAt, up);
		}

		public void Activate(ICamera oldCamera)
		{
			IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
			src.MouseDown += _OnMouseDown;
			src.MouseUp += _OnMouseUp;

			if (oldCamera != null)
			{
				var data = oldCamera.GetCameraData();
				m_isLeftDrugging = false;
				m_isRightDrugging = false;

				var gaze = data.lookAt - data.eye;
				Zoom = gaze.Length();

				gaze.Normalize();
				var right = Vector3.Cross(data.up, gaze);
				var position = data.eye;

				m_cameraTrans.Row1 = new Vector4(right, 0);
				m_cameraTrans.Row2 = new Vector4(data.up, 0);
				m_cameraTrans.Row3 = new Vector4(gaze, 0);
				m_cameraTrans.Row4 = new Vector4(position, 1);
			}
		}

		public void Deactivate()
		{
			IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
			src.MouseDown -= _OnMouseDown;
			src.MouseUp -= _OnMouseUp;
		}

		#region private members

		private bool m_isLeftDrugging;
		private bool m_isRightDrugging;

		private Matrix m_cameraTrans;

		#endregion // private members

		#region private methods

		private void _OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_isLeftDrugging = true;
			}
			else if (e.Button == MouseButtons.Right)
			{
				m_isRightDrugging = true;
			}
		}

		private void _OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_isLeftDrugging = false;
			}
			else if (e.Button == MouseButtons.Right)
			{
				m_isRightDrugging = false;
			}
		}

		#endregion // private methods
	}
}
