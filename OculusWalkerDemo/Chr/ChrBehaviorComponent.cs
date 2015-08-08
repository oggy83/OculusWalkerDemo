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
            m_reqMoveDir = Vector3.Zero;
            m_lastAngleY = 0;
            m_movingTime = 0;
		}

        /// <summary>
        /// set bothe of direction and speed for moving for next update
        /// </summary>
        /// <param name="moveDir">moving vector</param>
        /// <remarks>the length of moveDir represents speed, and it must be set to between 0.0 and 1.0.</remarks>
        public void RequestMove(Vector3 moveDir)
        {
            m_reqMoveDir = moveDir;
        }

        /// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
        public override void Update(double dT)
        {
            float movePower = m_reqMoveDir.Length();
            Vector3 moveDir = m_reqMoveDir * (float)dT * MoveSpeed;
            m_reqMoveDir = Vector3.Zero;

            if (moveDir.IsZero)
            {
                m_walkHandle.Weight = (float)Math.Max(0.0, m_walkHandle.Weight - dT * 10);
                m_jogHandle.Weight = (float)Math.Max(0.0, m_jogHandle.Weight - dT * 10);
                m_pauseHandle.Weight = (float)Math.Min(1.0, m_pauseHandle.Weight + dT * 10);
                m_movingTime = 0;
                return;
            }

            // update animation weight
            m_movingTime += dT;

            float jogWeight =  movePower > 0.4f ? (movePower - 0.4f) : 0.0f;
            float walkWeight = 1 - movePower;

            m_walkHandle.Weight = (float)Math.Min(walkWeight, m_walkHandle.Weight + dT * 3);
            m_jogHandle.Weight = (float)Math.Min(jogWeight, m_jogHandle.Weight + dT * 3);
            m_pauseHandle.Weight = (float)Math.Max(0.0, m_pauseHandle.Weight - dT * 3);
            
            m_walkHandle.Speed = movePower * WalkAnimPlaySpeed;
            m_jogHandle.Speed = movePower * JogAnimPlaySpeed;

            
            // update layout transform
            double angleY = Math.Atan2(moveDir.X, moveDir.Z);
            angleY = _CalcNextYAngle(dT, angleY);

            var translation = Matrix.Translation(m_layoutC.Transform.TranslationVector + moveDir);
            var rotation = Matrix.RotationY((float)angleY);
            m_layoutC.Transform = rotation * translation;

            // normalize to [0, 2PI]
            m_lastAngleY = angleY % (2 * Math.PI);
            if (m_lastAngleY < 0)
            {
                m_lastAngleY += 2 * Math.PI;
            }
        }

        public override void OnAddToEntity(GameEntity entity)
        {
            base.OnAddToEntity(entity);

            m_layoutC = Owner.FindComponent<LayoutComponent>();
            Debug.Assert(m_layoutC != null, "PlayerInputCompoment depends on LayoutCompoment");

            m_animC = Owner.FindComponent<AnimComponent>();
            Debug.Assert(m_animC != null, "PlayerInputCompoment depends on AnimCompoment");

            m_walkHandle = m_animC.Play("Walk");
            m_jogHandle = m_animC.Play("Jog");
            m_pauseHandle = m_animC.Play("Pause");
        }

        public override void OnRemoveFromEntity(GameEntity entity)
        {
            m_layoutC = null;

            base.OnRemoveFromEntity(entity);
        }

        #region private methods

        /// <summary>
        /// calculate a character turn angle
        /// </summary>
        /// <param name="dt">delta time [sec]</param>
        /// <param name="yAngle">y angle input</param>
        /// <returns>next y angle [-PI, PI]</returns>
        /// <remarks>
        /// character turns to right or left side so that the angle of turning is shortening.
        /// and the angle is clampled with maxTurnAngle.
        /// </remarks>
        private double _CalcNextYAngle(double dT, double angleY)
        {
            angleY += Math.PI;// [0, 2PI]

            double maxTurnAngle = MathUtil.PI * 2.0f * dT;
            if (m_lastAngleY > angleY)
            {
                double turnAngle = angleY - m_lastAngleY;// [-2PI, 0]
                if (turnAngle < -Math.PI)
                {
                    turnAngle = 2 * Math.PI + turnAngle;// [0, PI]
                    angleY = m_lastAngleY + Math.Min(turnAngle, maxTurnAngle);
                }
                else
                {
                    // turnAngle;// [-PI, 0]
                    angleY = m_lastAngleY + Math.Max(turnAngle, -maxTurnAngle);
                }

            }
            else if (m_lastAngleY < angleY)
            {
                double turnAngle = angleY - m_lastAngleY;// [0, 2PI]
                if (turnAngle > Math.PI)
                {
                    turnAngle = turnAngle - 2 * Math.PI;// [-PI, 0]
                    angleY = m_lastAngleY + Math.Max(turnAngle, -maxTurnAngle);
                }
                else
                {
                    // turnAngle;// [0, PI]
                    angleY = m_lastAngleY + Math.Min(turnAngle, maxTurnAngle);
                }

            }
    
            return angleY;
        }

        #endregion // private methods

        #region private members

        /// <summary>
        /// layout compoment
        /// </summary>
        private LayoutComponent m_layoutC;

        /// <summary>
        /// request for moving
        /// </summary>
        private Vector3 m_reqMoveDir;

        /// <summary>
        /// angle y at last frame
        /// </summary>
        private double m_lastAngleY;

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

        /// <summary>
        /// scale factor of move speed
        /// </summary>
        private const float MoveSpeed = 3.0f;

        #endregion // private members

    }
}
