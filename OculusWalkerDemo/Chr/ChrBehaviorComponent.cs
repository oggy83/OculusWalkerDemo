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
        public ChrBehaviorComponent()
		: base(GameEntityComponent.UpdateLines.Behavior)
		{
            m_reqMoveDir = Vector3.Zero;
            m_lastAngleY = 0;
            m_movingTime = 0;
		}

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
            if (m_reqMoveDir.IsZero)
            {
                m_walkHandle.Weight = (float)Math.Max(0.0, m_walkHandle.Weight - dT * 10);
                m_pauseHandle.Weight = (float)Math.Min(1.0, m_pauseHandle.Weight + dT * 10);
                m_movingTime = 0;
                return;
            }

            // update animation weight
            m_movingTime += dT;
            m_walkHandle.Weight = (float)Math.Min(1.0, m_walkHandle.Weight + dT * 3);
            m_pauseHandle.Weight = (float)Math.Max(0.0, m_pauseHandle.Weight - dT * 3);

            var translation = Matrix.Translation(m_layoutC.Transform.TranslationVector + m_reqMoveDir);

            // calc turn angle
            // character turns to right or left side so that the angle of turning is shortening.
            // and the angle is clampled with maxTurnAngle.
            m_reqMoveDir.Normalize();
            double angleY = Math.Atan2(m_reqMoveDir.X, m_reqMoveDir.Z);// the range of result is [-PI, PI]
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
            m_pauseHandle = m_animC.Play("Pause");
        }

        public override void OnRemoveFromEntity(GameEntity entity)
        {
            m_layoutC = null;

            base.OnRemoveFromEntity(entity);
        }

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
        /// handle for pause animation
        /// </summary>
        private AnimHandle m_pauseHandle;

        /// <summary>
        /// continuous moving time [sec]
        /// </summary>
        private double m_movingTime;

        #endregion // private members

    }
}
