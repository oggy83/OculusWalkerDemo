using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Diagnostics;

namespace Oggy
{
    public class ChrBehaviorComponent : GameEntityComponent
    {
        /// <summary>
        /// this value decides a play speed per max move power for walk animation
        /// </summary>
        private const float WalkAnimPlaySpeed = 2.5f;

        private const float JogAnimPlaySpeed = WalkAnimPlaySpeed * 30.0f / 40.0f;// to fit anim cycle period to walk anim

        public ChrBehaviorComponent()
		: base(GameEntityComponent.UpdateLines.Behavior)
		{
            m_reqMove = Vector3.Zero;
            m_reqMoveTo = Vector3.Zero;
            m_reqForwardDir = Vector3.Zero;
            m_movingTime = 0;
		}

        /// <summary>
        /// set position for moving for next update
        /// </summary>
        /// <param name="move">translation vector</param>
        /// <remarks>the length of move represents speed, and it must be set to between 0.0 and 1.0.</remarks>
        public void RequestMove(Vector3 move)
        {
            m_reqMove = move;
			m_reqForwardDir = move;
			m_isTurning = true;
			m_isMoving = true;
        }

        public void RequestMoveTo(Vector3 pos)
        {
			m_reqMoveTo = pos;
			m_reqMove = Vector3.Zero;
			m_isMoving = true;
        }

        public void RequestTurn(Vector3 dir)
        {
            m_reqForwardDir = dir;
            m_isTurning = true;
        }

		public void Warp(Matrix pose)
		{
			m_reqMove = Vector3.Zero;
			m_reqMoveTo = Vector3.Zero;
			m_reqForwardDir = Vector3.Zero;
			m_isMoving = false;
			m_isTurning = false;

			m_layoutC.Transform = pose;
		}

        /// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
        public override void Update(double dT)
        {
			Vector3 moveDir = Vector3.Zero;
			if (m_isMoving)
			{
				float movePower = 1;
				if (!m_reqMove.IsZero)
				{
					// move relatively
					movePower = m_reqMove.Length();
					moveDir = m_reqMove * (float)dT * MoveSpeed;

					m_reqMove = Vector3.Zero;
					m_isMoving = false;
				}
				else if (!m_reqMoveTo.IsZero)
				{
					Vector3 diffVec = m_reqMoveTo - m_layoutC.Transform.TranslationVector;
					movePower = 1;

					double requestSpeed = diffVec.Length();
					if (requestSpeed <= 1E-3)
					{
						m_reqMoveTo = Vector3.Zero;
						m_isMoving = false;
					}

					double speed = Math.Min(requestSpeed, MoveSpeed * dT);
					diffVec.Normalize();
					moveDir = diffVec * (float)speed;
				}

				// update animation weight
				m_movingTime += dT;

				float jogWeight = movePower > 0.4f ? (movePower - 0.4f) : 0.0f;
				float walkWeight = 1 - movePower;

				m_walkHandle.Weight = (float)Math.Min(walkWeight, m_walkHandle.Weight + dT * 3);
				m_jogHandle.Weight = (float)Math.Min(jogWeight, m_jogHandle.Weight + dT * 3);
				m_pauseHandle.Weight = (float)Math.Max(0.0, m_pauseHandle.Weight - dT * 3);

				m_walkHandle.Speed = movePower * WalkAnimPlaySpeed;
				m_jogHandle.Speed = movePower * JogAnimPlaySpeed;
			}
			else
			{
				// stop motion
				m_walkHandle.Weight = (float)Math.Max(0.0, m_walkHandle.Weight - dT * 10);
				m_jogHandle.Weight = (float)Math.Max(0.0, m_jogHandle.Weight - dT * 10);
				m_pauseHandle.Weight = (float)Math.Min(1.0, m_pauseHandle.Weight + dT * 10);
				m_movingTime = 0;
			}

            // update turning
            double angleY = Math.Atan2(m_layoutC.Transform.Forward.X, m_layoutC.Transform.Forward.Z);
            if (m_isTurning)
            {
                double goalAngleY = Math.Atan2(m_reqForwardDir.X, m_reqForwardDir.Z);
                double currentAngleY = angleY;
                angleY = _CalcNextYAngle(dT, goalAngleY, currentAngleY);
                if (Math.Abs(goalAngleY - angleY) <= 1E-6)
                {
                    m_isTurning = false;
                    m_reqForwardDir = Vector3.Zero;
                }
            }

            // update layout transform
            var translation = Matrix.Translation(m_layoutC.Transform.TranslationVector + moveDir);
            var rotation = Matrix.RotationY((float)(angleY + Math.PI));// forward is z-
            m_layoutC.Transform = rotation * translation;
        }

        public override void OnAddToEntity(GameEntity entity)
        {
            base.OnAddToEntity(entity);

            m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "ChrBahaviorCompoment depends on LayoutCompoment");

            m_animC = Owner.FindComponent<AnimComponent>();
            Debug.Assert(m_animC != null, "ChrBahaviorCompoment depends on AnimCompoment");

            m_walkHandle = m_animC.Play("Walk");
            m_jogHandle = m_animC.Play("Jog");
            m_pauseHandle = m_animC.Play("Pause");
        }

        public override void OnRemoveFromEntity(GameEntity entity)
        {
            m_layoutC = null;

            base.OnRemoveFromEntity(entity);
        }

        /// <summary>
        /// get whether character is idling 
        /// </summary>
        public bool IsIdling()
        {
            return !m_isMoving && !m_isTurning;
        }

        #region private methods

        /// <summary>
        /// calculate a character turn angle
        /// </summary>
        /// <param name="dt">delta time [sec]</param>
        /// <param name="yAngle">y angle input [-PI, PI]</param>
        /// <param name="currentAngleY">current y angle</param>
        /// <returns>next y angle [-PI, PI]</returns>
        /// <remarks>
        /// character turns to right or left side so that the angle of turning is shortening.
        /// and the angle is clampled with maxTurnAngle.
        /// </remarks>
        private static double _CalcNextYAngle(double dT, double inputAngleY, double currentAngleY)
        {
            double nextAngle = inputAngleY + Math.PI;// [0, 2PI]
            double lastAngle = currentAngleY + Math.PI;// [0, 2PI]

            double maxTurnAngle = MathUtil.PI * 2.0f * dT;
            if (lastAngle > nextAngle)
            {
                double turnAngle = nextAngle - lastAngle;// [-2PI, 0]
                if (turnAngle < -Math.PI)
                {
                    turnAngle = 2 * Math.PI + turnAngle;// [0, PI]
                    nextAngle = lastAngle + Math.Min(turnAngle, maxTurnAngle);
                }
                else
                {
                    // turnAngle;// [-PI, 0]
                    nextAngle = lastAngle + Math.Max(turnAngle, -maxTurnAngle);
                }

            }
            else if (lastAngle < nextAngle)
            {
                double turnAngle = nextAngle - lastAngle;// [0, 2PI]
                if (turnAngle > Math.PI)
                {
                    turnAngle = turnAngle - 2 * Math.PI;// [-PI, 0]
                    nextAngle = lastAngle + Math.Max(turnAngle, -maxTurnAngle);
                }
                else
                {
                    // turnAngle;// [0, PI]
                    nextAngle = lastAngle + Math.Min(turnAngle, maxTurnAngle);
                }

            }

            nextAngle -= Math.PI;// [-PI, PI]
            return nextAngle;
        }

        #endregion // private methods

        #region private members

        /// <summary>
        /// layout compoment
        /// </summary>
        private LayoutComponent m_layoutC;

        /// <summary>
        /// request for moving (relatively)
        /// </summary>
        private Vector3 m_reqMove;

        /// <summary>
        /// request to move to the given position
        /// </summary>
        private Vector3 m_reqMoveTo;

        /// <summary>
        /// request to turning to the given direction
        /// </summary>
        private Vector3 m_reqForwardDir;

        /// <summary>
        /// animation component
        /// </summary>
        private AnimComponent m_animC;

        /// <summary>
        /// handle for walk animation
        /// </summary>
        private AnimHandle m_walkHandle;

        /// <summary>
        /// handle for jog animation
        /// </summary>
        private AnimHandle m_jogHandle;

        /// <summary>
        /// handle for pause animation
        /// </summary>
        private AnimHandle m_pauseHandle;

        /// <summary>
        /// continuous moving time [sec]
        /// </summary>
        private double m_movingTime;

        private bool m_isTurning = false;

        private bool m_isMoving = false;

        /// <summary>
        /// scale factor of move speed
        /// </summary>
        private const float MoveSpeed = 5.0f;

        #endregion // private members

    }
}
