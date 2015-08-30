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
    /// input processing component for camera entity
    /// </summary>
    /// <remarks>
    /// this class cooperates with FollowEntityCamera
    /// </remarks>
    public class FpsInputComponent : GameEntityComponent
    {
        private const float AngleFactor = 0.50f;
        private const float PositionFactor = 1.0f;
        private const float PI = (float)Math.PI;
        private const float MinVerticalAngle = PI * -0.3f;
        private const float MaxVerticalAngle = PI * 0.3f;
        private const float PadAngleFactor = 1.0f;
        private const float MinimumPadMagnitude = 0.15f;

        public FpsInputComponent()
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
            if (!m_behaviorC.IsIdling())
            {
                // wait for end of behavior
                return;
            }

            var cameraSys = CameraSystem.GetInstance();
            var drawSys = DrawSystem.GetInstance();

            // Calc camera 
            var viewHeadMat = cameraSys.GetCameraData().GetViewMatrix();
            viewHeadMat.Invert();
			var position = viewHeadMat.TranslationVector;
			position.Y = 0;
            var moveDirection = new Vector3(viewHeadMat.Backward.X, 0, viewHeadMat.Backward.Z);
            moveDirection.Normalize();
			float angleY = (float)Math.Atan2(moveDirection.X, moveDirection.Z);
			Matrix3x3 localToWorldMat = Matrix3x3.RotationY(angleY);



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
							// @todo
                        }
                    }
                    break;

                case GameConfig.UserInputDevices.Pad:
                    {
                       
                        IPadInputSource src = InputSystem.GetInstance().Pad;
                       
                        // update move
                        {
                            var thumbDir = src.LeftThumbInput.Direction;
                            float magnitude = src.LeftThumbInput.NormalizedMagnitude;
                            if (magnitude >= MinimumPadMagnitude)
                            {
                                var v = new Vector3(0, 0, 10);
                                v = Vector3.Transform(v, localToWorldMat) + position;
                                m_behaviorC.RequestMoveTo(v);
                            }
                        }

                        // update angle
                        {
                            var thumbDir = src.RightThumbInput.Direction;
                            float magnitude = src.RightThumbInput.NormalizedMagnitude;
                            if (magnitude >= MinimumPadMagnitude )
                            {
                                if (thumbDir.X > 0)
                                {
                                    var v = new Vector3(1, 0, 0);
                                    v = Vector3.Transform(v, localToWorldMat);
                                    m_behaviorC.RequestTurn(v);
                                    m_lastAngle = v;
                                }
                                else if (thumbDir.X < 0)
                                {
                                    var v = new Vector3(-1, 0, 0);
                                    v = Vector3.Transform(v, localToWorldMat);
                                    m_behaviorC.RequestTurn(v);
                                    m_lastAngle = v;
                                }
                            }
                        }
                       
                    }
                    break;

                default:
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

        private Vector3 m_lastAngle = Vector3.Zero;

        #endregion // private members
    }
}
