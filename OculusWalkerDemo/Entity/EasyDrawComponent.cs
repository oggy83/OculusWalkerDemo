using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace Oggy
{
	public class EasyDrawComponent : GameEntityComponent
	{
		#region public types

		public delegate DrawModel DrawFunction(IDrawContext context, Matrix layout, DrawModel prevDrawModel);

		#endregion // public types

		public EasyDrawComponent(DrawFunction func)
		: base(GameEntityComponent.UpdateLines.Draw)
		{
			m_layoutC = null;
			m_lastDrawModel = null;
			m_drawFunc = func;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			var layout = m_layoutC.Transform;
			var drawSys = DrawSystem.GetInstance();

			m_lastDrawModel = m_drawFunc(drawSys.GetDrawContext(), layout, m_lastDrawModel);
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "EasyDrawComponent depends on LayoutCompoment");
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			if (m_lastDrawModel != null)
			{
				m_lastDrawModel.Dispose();
				m_lastDrawModel = null;
			}

			m_layoutC = null;
			base.OnRemoveFromEntity(entity);
		}

		#region private members

		/// <summary>
		/// layout compoment
		/// </summary>
		private LayoutComponent m_layoutC;

		private DrawModel m_lastDrawModel;

		private DrawFunction m_drawFunc;

		#endregion // private members

	}
}
