using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Oggy
{
	/// <summary>
	/// Game Entity
	/// </summary>
	public class GameEntity : IDisposable
	{
		#region properties

		/// <summary>
		/// Entity Name (Not Unique)
		/// </summary>
		private String m_name = "";
		public String Name
		{
			get
			{
				return m_name;
			}
		}

		/// <summary>
		/// Entity components
		/// </summary>
		private List<GameEntityComponent> m_components = null;
		public ReadOnlyCollection<GameEntityComponent> Components
		{
			get
			{
				return m_components.AsReadOnly();
			}
		}

		#endregion // properties

		public GameEntity(string name)
		{
			m_name = name;
			m_components = new List<GameEntityComponent>();
		}

		/// <summary>
		/// Add new component to entity
		/// </summary>
		/// <param name="component">new component</param>
		public void AddComponent(GameEntityComponent component)
		{
			m_components.Add(component);
			component.OnAddToEntity(this);
		}

		/// <summary>
		/// Dispose entity
		/// </summary>
		/// /// <remarks>
		/// If you override this methods, you must call this version too.
		/// </remarks>
		public void Dispose()
		{
			foreach (var component in m_components.Reverse<GameEntityComponent>())
			{
				component.OnRemoveFromEntity(this);
			}
			m_components = null;
		}

		/// <summary>
		/// Find component by type
		/// </summary>
		/// <typeparam name="ComponentType">GameEntityComponent Type</typeparam>
		/// <returns>found component or null</returns>
		public ComponentType FindComponent<ComponentType>()
			where ComponentType : GameEntityComponent
		{
			foreach (var component in m_components)
			{
				if (component.GetType() == typeof(ComponentType))
				{
					return (ComponentType)component;
				}
			}

			// not found
			return null;
		}
	}
}
