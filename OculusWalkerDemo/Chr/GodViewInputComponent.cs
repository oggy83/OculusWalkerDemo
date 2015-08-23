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
    /// <summary>
    /// input processing component for player entity
    /// </summary>
    /// <remarks>
    /// this class makes commands for behavior stage by input from gamepad or keyboard.
    /// to be specific,
    /// - operates the character move (direction and power)
    /// - this class cooperates with FixedCamera
    /// </remarks>
    public class GodViewInputComponent : GameEntityComponent
    {
        private const float AngleFactor = 0.50f;
		private const float PositionFactor = 1.0f;
		private const float PI = (float)Math.PI;
		private const float MinVerticalAngle = PI * -0.3f;
		private const float MaxVerticalAngle = PI * 0.3f;
		private const float PadAngleFactor = 1.0f;
		private const float MinimumPadMagnitude = 0.15f;

        public GodViewInputComponent()
		: base(GameEntityComponent.UpdateLines.Input)
		{
            // nothing
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			var cameraSys = CameraSystem.GetInstance();
            var drawSys = DrawSystem.GetInstance();

            // Calc camera 
            var viewHeadMat = cameraSys.GetCameraData().GetViewMatrix() * drawSys.GetDrawContext().GetHeadMatrix();
            viewHeadMat.Invert();
            var moveDirection = new Vector3(viewHeadMat.Backward.X, 0, viewHeadMat.Backward.Z);
            moveDirection.Normalize();

			var gameSys = GameSystem.GetInstance();
			switch (gameSys.Config.InputDevice)
			{
				case GameConfig.UserInputDevices.MouseKeyboard:
					{
						IMouseKeyboardInputSource src = InputSystem.GetInstance().MouseKeyboard;
                            
						var v = Vector3.Zero;
						if (src.TestKeyState(Keys.W))
						{
                            v.Z += PositionFactor;
						}
						if (src.TestKeyState(Keys.A))
						{
                            v.X -= PositionFactor;
						}
						if (src.TestKeyState(Keys.D))
						{
                            v.X += PositionFactor;
						}
						if (src.TestKeyState(Keys.S))
						{
                            v.Z -= PositionFactor;
						}

                        if (v.X != 0 || v.Z != 0)
                        {
                            float angleY = (float)Math.Atan2(moveDirection.X, moveDirection.Z);
                            v = Vector3.Transform(v, Matrix3x3.RotationY(angleY));
                            m_behaviorC.RequestMove(v);
                            m_behaviorC.RequestTurn(v);
                        }
                        else
                        {
                            m_behaviorC.RequestMove(Vector3.Zero);
                            m_behaviorC.RequestTurn(Vector3.Zero);
                        }
				}
					break;

				case GameConfig.UserInputDevices.Pad:
					{
						IPadInputSource src = InputSystem.GetInstance().Pad;

                        float angleY = (float)Math.Atan2(moveDirection.X, moveDirection.Z);
						var thumbDir = src.LeftThumbInput.Direction;
						float magnitude = src.LeftThumbInput.NormalizedMagnitude;
						if (magnitude >= MinimumPadMagnitude)
						{
                            var v = new Vector3(thumbDir.X, 0, thumbDir.Y);
                            v = Vector3.Transform(v, Matrix3x3.RotationY(angleY));
                            v = v * magnitude;
                            m_behaviorC.RequestMove(v);
                            m_behaviorC.RequestTurn(v);
						}
                        else
                        {
                            m_behaviorC.RequestMove(Vector3.Zero);
                            m_behaviorC.RequestTurn(Vector3.Zero);
                        }
					}
					break;

				default :
					Debug.Fail("unsupported input device type : " + gameSys.Config.InputDevice);
					break;
			}
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

            m_behaviorC = Owner.FindComponent<ChrBehaviorComponent>();
            Debug.Assert(m_behaviorC != null, "PlayerInputCompoment depends on ChrBehaviorCompoment");
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
            m_behaviorC = null;

			base.OnRemoveFromEntity(entity);
		}

		#region private members

        /// <summary>
        /// behavior component
        /// </summary>
        private ChrBehaviorComponent m_behaviorC;
		
		#endregion // private members
    }
}
