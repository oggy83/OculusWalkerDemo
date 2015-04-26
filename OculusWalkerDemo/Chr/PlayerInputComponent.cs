using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
    public class PlayerInputComponent : GameEntityComponent
    {
        private const float AngleFactor = 0.50f;
		private const float PositionFactor = 1.0f;
		private const float PI = (float)Math.PI;
		private const float MinVerticalAngle = PI * -0.3f;
		private const float MaxVerticalAngle = PI * 0.3f;
		private const float PadAngleFactor = 1.0f;
		private const float PadPositionFactor = 1.5f;

        public PlayerInputComponent()
		: base(GameEntityComponent.UpdateLines.Input)
		{
			m_position = new Vector3(0, 0, 0);
			m_angle = Vector3.Zero;
			m_isLeftDrugging = false;
			m_movingTime = 0.0f;
			//m_walkHandle = null;
			//m_pauseHandle = null;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			var cameraSys = CameraSystem.GetInstance();
            var drawSys = DrawSystem.GetInstance();

            var viewMat = cameraSys.GetCameraData().GetViewMatrix() * drawSys.GetDrawContext().GetHeadMatrix();
            viewMat.Invert();
            var moveDirection = new Vector3(viewMat.Backward.X, 0, viewMat.Backward.Z);
            moveDirection.Normalize();

			bool isMoving = false;

			var gameSys = GameSystem.GetInstance();
			switch (gameSys.Config.InputDevice)
			{
				case GameConfig.UserInputDevices.MouseKeyboard:
					{
						IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
						Vector2 dPos = src.MousePositionDelta;

						if (m_isLeftDrugging)
						{
							// update angle
							float hAngle = dPos.X * AngleFactor * (float)dT;
                            float vAngle = dPos.Y * AngleFactor * (float)dT;

							m_angle.X = (m_angle.X + hAngle) % (2 * PI);
							m_angle.Y = MathUtil.Clamp(m_angle.Y + vAngle, MinVerticalAngle, MaxVerticalAngle);
						}

						// update position
						{
							var v = new Vector4(0, 0, 0, 1);
							if (src.TestKeyState(Keys.W))
							{
                                v.Z += PositionFactor * (float)dT;
							}
							if (src.TestKeyState(Keys.A))
							{
                                v.X -= PositionFactor * (float)dT;
							}
							if (src.TestKeyState(Keys.D))
							{
                                v.X += PositionFactor * (float)dT;
							}
							if (src.TestKeyState(Keys.S))
							{
                                v.Z -= PositionFactor * (float)dT;
							}

							v = Vector4.Transform(v, Matrix.RotationY(m_angle.X));
							if (v.X != 0.0f || v.Y != 0.0f || v.Z != 0.0f)
							{
								isMoving = true;
							}
							m_position += MathUtil.ToVector3(v);
						}
					}
					break;

				case GameConfig.UserInputDevices.Pad:
					{
						IPadInputSource src = InputSystem.GetInstance().Pad;

						{
							// update angle
							var direction = src.RightThumbInput.Direction;
							float magnitude = src.RightThumbInput.NormalizedMagnitude;
                            float hAngle = direction.X * magnitude * PadAngleFactor * (float)dT;
                            float vAngle = -direction.Y * magnitude * PadAngleFactor * (float)dT;

							//m_angle.X = (m_angle.X + hAngle) % (2 * PI);
							//m_angle.Y = MathUtil.Clamp(m_angle.Y + vAngle, MinVerticalAngle, MaxVerticalAngle);
                            m_angle.X = (float)Math.Atan2(moveDirection.X, moveDirection.Z);
                            m_angle.Y = 0;
						}

						{
							// update position
							var direction = src.LeftThumbInput.Direction;
							float magnitude = src.LeftThumbInput.NormalizedMagnitude;
							if (magnitude != 0.0f)
							{
								isMoving = true;
							}

							var v = new Vector4(direction.X, 0, direction.Y, 1);
							v = Vector4.Transform(v, Matrix.RotationY(m_angle.X));
                            m_position += (MathUtil.ToVector3(v) * magnitude * PadPositionFactor * (float)dT);
						}
					}
					break;

				default :
					Debug.Fail("unsupported input device type : " + gameSys.Config.InputDevice);
					break;
			}

			if (isMoving)
			{
				m_movingTime += dT;
			}
			else
			{
				// reset moving time
				m_movingTime = 0.0f;
			}

            /*
			// update animation weight
			if (m_movingTime <= 0.3f)
			{
				m_walkHandle.Weight = Math.Max(0.0f, m_walkHandle.Weight - dT * 3);
				m_pauseHandle.Weight = Math.Min(1.0f, m_pauseHandle.Weight + dT * 3);
			}
			else
			{
				m_walkHandle.Weight = Math.Min(1.0f, m_walkHandle.Weight + dT * 3);
				m_pauseHandle.Weight = Math.Max(0.0f, m_pauseHandle.Weight - dT * 3);
			}
            */

			// update layout
			m_layoutC.Transform = GetNextWorldMatrix();
			//m_animC.HeadBoneMatrix = GetNextHeadMatrix();
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
			src.MouseDown += _OnMouseDown;
			src.MouseUp += _OnMouseUp;

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "PlayerInputCompoment depends on LayoutCompoment");

			//m_animC = Owner.FindComponent<AnimComponent>();
			//Debug.Assert(m_animC != null, "PlayerInputCompoment depends on AnimCompoment");

			//m_walkHandle = m_animC.Play("Walk");
			//m_pauseHandle = m_animC.Play("Pause");
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			m_layoutC = null;

			IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
			src.MouseDown -= _OnMouseDown;
			src.MouseUp -= _OnMouseUp;

			base.OnRemoveFromEntity(entity);
		}

		public Matrix GetNextWorldMatrix()
		{
			return Matrix.RotationYawPitchRoll(m_angle.X, 0.0f, 0.0f) * Matrix.Translation(m_position);
		}

		public Matrix GetNextHeadMatrix()
		{
			//return Matrix.RotationYawPitchRoll(0.0f, m_angle.Y, m_angle.Z);
			return Matrix.RotationYawPitchRoll(0.0f, 0.0f, m_angle.Y);
		}

		#region private members

		private bool m_isLeftDrugging;

		/// <summary>
		/// camera position vector
		/// </summary>
		private Vector3 m_position;

		/// <summary>
		/// camera angle vector
		/// </summary>
		private Vector3 m_angle;

		/// <summary>
		/// layout compoment
		/// </summary>
		private LayoutComponent m_layoutC;

		/// <summary>
		/// animation component
		/// </summary>
		//private AnimComponent m_animC;

		/// <summary>
		/// time span of moving
		/// </summary>
		private double m_movingTime;

		/// <summary>
		/// handle for walk animation
		/// </summary>
		//private AnimHandle m_walkHandle;

		/// <summary>
		/// handle for pause animation
		/// </summary>
		//private AnimHandle m_pauseHandle;

		#endregion // private members

		#region private methods

		private void _OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_isLeftDrugging = true;
			}
		}

		private void _OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_isLeftDrugging = false;
			}
		}

		#endregion // private methods
    }
}
