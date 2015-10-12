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

        public FpsInputComponent(MapLocation startLocation, bool isEnableRightStick)
            : base(GameEntityComponent.UpdateLines.Input)
        {
			m_coroutine = new Coroutine();
			m_currentLocation = startLocation;
			m_isEnableRightStick = isEnableRightStick;
        }

        /// <summary>
        /// Update component
        /// </summary>
        /// <param name="dT">spend time [sec]</param>
        public override void Update(double dT)
        {
			var mapSys = MapSystem.GetInstance();

			m_coroutine.Update(dT);

            if (!m_coroutine.HasCompleted())
            {
				// wait for end of character operation
                return;
            }

            var cameraSys = CameraSystem.GetInstance();
            var drawSys = DrawSystem.GetInstance();
          
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
							_InputType inputType = _InputType.None;

                            var thumbDir = src.LeftThumbInput.Direction;
                            float magnitude = src.LeftThumbInput.NormalizedMagnitude;
							if (magnitude >= MinimumPadMagnitude)
							{
								// use pad stick
								if (thumbDir.Y <= -0.851)
								{
									inputType = _InputType.Backward;// go backward
								}
								else if (thumbDir.Y >= 0.851)
								{
									inputType = _InputType.Forward;// go forward
								}
								else
								{
									if (thumbDir.X < 0)
									{
										inputType = _InputType.Left;// go left
									}
									else
									{
										inputType = _InputType.Right;// go right
									}
								}
							}
							else
							{
								// use pad cross key
								if (src.TestButtonState(InputSystem.PadButtons.Down))
								{
									inputType = _InputType.Backward;// go backward
								}
								else if (src.TestButtonState(InputSystem.PadButtons.Up))
								{
									inputType = _InputType.Forward;// go forward
								}
								else if (src.TestButtonState(InputSystem.PadButtons.Left))
								{
									inputType = _InputType.Left;// go left
								}
								else if (src.TestButtonState(InputSystem.PadButtons.Right))
								{
									inputType = _InputType.Right;// go right
								}

							}

							switch (inputType)
							{
								case _InputType.Forward:
									{
										var nextDir = MapLocation.GetForwardDirection(m_currentLocation.Direction);
										MapLocation? nextLocation = mapSys.Walk(m_currentLocation, nextDir);
										if (nextLocation.HasValue)
										{
											m_currentLocation = nextLocation.Value;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(new _MoveToTask(this, pose.TranslationVector));// no turn
										}
									}
									break;

								case _InputType.Backward:
									{
										var nextDir = MapLocation.GetBackwardDirection(m_currentLocation.Direction);
										MapLocation? nextLocation = mapSys.Walk(m_currentLocation, nextDir);
										if (nextLocation.HasValue)
										{
											m_currentLocation = nextLocation.Value;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
									}
									break;

								case _InputType.Left:
									{
										var nextDir = MapLocation.GetLeftDirection(m_currentLocation.Direction);
										MapLocation? nextLocation = mapSys.Walk(m_currentLocation, nextDir);
										if (nextLocation.HasValue)
										{
											m_currentLocation = nextLocation.Value;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
									}
									break;

								case _InputType.Right:
									{
										var nextDir = MapLocation.GetRightDirection(m_currentLocation.Direction);
										MapLocation? nextLocation = mapSys.Walk(m_currentLocation, nextDir);
										if (nextLocation.HasValue)
										{
											m_currentLocation = nextLocation.Value;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
									}
									break;
							}
                        }

                        // update angle
						if (m_isEnableRightStick)
						{
							if (m_coroutine.HasCompleted())
							{
								var thumbDir = src.RightThumbInput.Direction;
								float magnitude = src.RightThumbInput.NormalizedMagnitude;
								if (magnitude >= MinimumPadMagnitude)
								{
									if (thumbDir.X > 0)
									{
										var nextDir = MapLocation.GetRightDirection(m_currentLocation.Direction);
										m_currentLocation.Direction = nextDir;
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(new _TurnToTask(this, pose.Forward));
									}
									else if (thumbDir.X < 0)
									{
										var nextDir = MapLocation.GetLeftDirection(m_currentLocation.Direction);
										m_currentLocation.Direction = nextDir;
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(new _TurnToTask(this, pose.Forward));
									}
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

			// set first pose
			var pose = MapSystem.GetInstance().GetPose(m_currentLocation);
			m_behaviorC.Warp(pose);
        }

        public override void OnRemoveFromEntity(GameEntity entity)
        {
            m_behaviorC = null;

            base.OnRemoveFromEntity(entity);
        }

		#region private types

		private class _MoveToTask : CoroutineTask
		{
			public _MoveToTask(FpsInputComponent parent, Vector3 targetPos)
			{
				m_parent = parent;
				m_targetPos = targetPos;
			}

			public override IEnumerator<CoroutineTask> Execute()
			{
				m_parent.m_behaviorC.RequestMoveTo(m_targetPos);

				yield return new Coroutine.WaitDelegate(() =>
				{
					// wait for end of behavior
					return m_parent.m_behaviorC.IsIdling();
				});
			}

			private FpsInputComponent m_parent;
			private Vector3 m_targetPos;
		}

		private class _TurnToTask : CoroutineTask
		{
			public _TurnToTask(FpsInputComponent parent, Vector3 targetDir)
			{
				m_parent = parent;
				m_targetDir = targetDir;
			}

			public override IEnumerator<CoroutineTask> Execute()
			{
				m_parent.m_behaviorC.RequestTurn(m_targetDir);

				yield return new Coroutine.WaitDelegate(() =>
				{
					// wait for end of behavior
					return m_parent.m_behaviorC.IsIdling();
				});

				yield return new Coroutine.WaitTime(0.3);
				yield break;
			}

			private FpsInputComponent m_parent;
			private Vector3 m_targetDir;
		}

		enum _InputType
		{
			None = 0,
			Forward,
			Backward,
			Left,
			Right
		}

		#endregion // private types

		#region private members

		/// <summary>
        /// behavior component
        /// </summary>
        private ChrBehaviorComponent m_behaviorC;

		/// <summary>
		/// coroutine for character operation 
		/// </summary>
		private Coroutine m_coroutine;

		/// <summary>
		/// current location on map
		/// </summary>
		private MapLocation m_currentLocation;

		private bool m_isEnableRightStick;

        #endregion // private members
    }
}
