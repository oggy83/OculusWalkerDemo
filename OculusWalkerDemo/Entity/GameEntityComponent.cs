using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Oggy
{
	public partial class GameEntityComponent
	{
		#region properties

		private UpdateLines m_updateLine;
		public UpdateLines UpdateLine
		{
			get
			{
				return m_updateLine;
			}
		}

		/// <summary>
		/// GameEntity which containts this component
		/// </summary>
		private GameEntity m_entity = null;
		public GameEntity Owner
		{
			get
			{
				return m_entity;
			}
		}

		/// <summary>
		/// Previous Component of GameEntityComponentLine
		/// </summary>
		public GameEntityComponent Previous { get; set; }

		/// <summary>
		/// Next Component of GameEntityComponentLine
		/// </summary>
		public GameEntityComponent Next { get; set; }

		#endregion // properties

		public GameEntityComponent(UpdateLines updateLine)
		{
			m_updateLine = updateLine;
			Previous = Next = null;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		/// <remarks>Do NOT write access to GameEntityComponentLine</remarks>
		virtual public void Update(double dT)
		{
			// nothing
		}

		/// <summary>
		/// callback that this component is added to entity
		/// </summary>
		/// <param name="entity">onwer entity</param>
		/// <remarks>
		/// If you override this methods, you must call this version too.
		/// </remarks>
		virtual public void OnAddToEntity(GameEntity entity)
		{
			m_entity = entity;
			if (m_updateLine != UpdateLines.Invalid)
			{
				var entitySys = EntitySystem.GetInstance();
				entitySys.GetComponentLine(m_updateLine).AddComponent(this);
			}
		}

		/// <summary>
		/// callback that this component is removed from entity
		/// </summary>
		/// <param name="entity">owner entity</param>
		/// <remarks>
		/// If you override this methods, you must call this version too.
		/// </remarks>
		virtual public void OnRemoveFromEntity(GameEntity entity)
		{
			Debug.Assert(entity == m_entity, "invalid entity");
			m_entity = null;
			if (m_updateLine != UpdateLines.Invalid)
			{
				var entitySys = EntitySystem.GetInstance();
				entitySys.GetComponentLine(m_updateLine).RemoveComponent(this);
			}
		}
	}
}
