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

        public FpsInputComponent(MapLocation startLocation)
            : base(GameEntityComponent.UpdateLines.Input)
        {
			m_coroutine = new Coroutine();
			m_currentLocation = startLocation;
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
                            var thumbDir = src.LeftThumbInput.Direction;
                            float magnitude = src.LeftThumbInput.NormalizedMagnitude;
                            if (magnitude >= MinimumPadMagnitude)
                            {
								if (thumbDir.Y <= -0.851)
								{
									// go backward
									var currentBlockInfo = mapSys.GetBlockInfo(m_currentLocation.BlockX, m_currentLocation.BlockY);
									var nextDir = MapLocation.GetBackwardDirection(m_currentLocation.Direction);
									var nextBlockInfo = currentBlockInfo.GetAdjacent(nextDir);

									if (nextBlockInfo.CanWalkHalf())
									{
										m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
										m_currentLocation.SetPosition(nextDir);
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
									}
									else if (nextBlockInfo.CanWalkThrough())
									{
										m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
										m_currentLocation.Direction = nextDir;
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
									}
								}
								else if (thumbDir.Y >= 0.851)
								{
									// go forward
									var currentBlockInfo = mapSys.GetBlockInfo(m_currentLocation.BlockX, m_currentLocation.BlockY);
									var nextDir = MapLocation.GetForwardDirection(m_currentLocation.Direction);
									var nextBlockInfo = currentBlockInfo.GetAdjacent(m_currentLocation.Direction);

									if (nextBlockInfo.CanWalkHalf())
									{
										m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
										m_currentLocation.SetPosition(nextDir);
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(new _MoveToTask(this, pose.TranslationVector));// no turn
									}
									else if (nextBlockInfo.CanWalkThrough())
									{
										m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
										m_currentLocation.Direction = nextDir;
										var pose = mapSys.GetPose(m_currentLocation);
										m_coroutine.Start(new _MoveToTask(this, pose.TranslationVector));// no turn
									}
								}
								else
								{
									if (thumbDir.X < 0)
									{
										// go left
										var currentBlockInfo = mapSys.GetBlockInfo(m_currentLocation.BlockX, m_currentLocation.BlockY);
										var nextDir = MapLocation.GetLeftDirection(m_currentLocation.Direction);
										var nextBlockInfo = currentBlockInfo.GetAdjacent(nextDir);

										if (nextBlockInfo.CanWalkHalf())
										{
											m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
											m_currentLocation.SetPosition(nextDir);
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
										else if (nextBlockInfo.CanWalkThrough())
										{
											m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
											m_currentLocation.Direction = nextDir;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
									}
									else
									{
										// go right
										var currentBlockInfo = mapSys.GetBlockInfo(m_currentLocation.BlockX, m_currentLocation.BlockY);
										var nextDir = MapLocation.GetRightDirection(m_currentLocation.Direction);
										var nextBlockInfo = currentBlockInfo.GetAdjacent(nextDir);

										if (nextBlockInfo.CanWalkHalf())
										{
											m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
											m_currentLocation.SetPosition(nextDir);
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
										else if (nextBlockInfo.CanWalkThrough())
										{
											m_currentLocation = new MapLocation(nextBlockInfo.BlockX, nextBlockInfo.BlockY);
											m_currentLocation.Direction = nextDir;
											var pose = mapSys.GetPose(m_currentLocation);
											m_coroutine.Start(Coroutine.Join(new _TurnToTask(this, pose.Forward), new _MoveToTask(this, pose.TranslationVector)));
										}
									}
								}
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

				yield return new Coroutine.WaitTime(0.01);
				yield break;
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

        #endregion // private members
    }
}
