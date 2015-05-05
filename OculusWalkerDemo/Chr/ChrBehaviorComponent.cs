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
            // nothing
		}

        public void RequestMove(Vector3 moveDir)
        {
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
            m_layoutC.Transform *= Matrix.Translation(moveDir);

        }

        public override void OnAddToEntity(GameEntity entity)
        {
            base.OnAddToEntity(entity);

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

            base.OnRemoveFromEntity(entity);
        }

        #region private members

        /// <summary>
        /// layout compoment
        /// </summary>
        private LayoutComponent m_layoutC;

        /// <summary>
        /// animation component
        /// </summary>
        //private AnimComponent m_animC;

        /// <summary>
        /// handle for walk animation
        /// </summary>
        //private AnimHandle m_walkHandle;

        /// <summary>
        /// handle for pause animation
        /// </summary>
        //private AnimHandle m_pauseHandle;


        #endregion // private members

    }
}
