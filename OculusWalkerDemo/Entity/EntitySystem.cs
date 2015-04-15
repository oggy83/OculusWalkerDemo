
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Oggy
{
	/// <summary>
	/// GameEntity Collection.
	/// This class supports Component Update System.
	/// </summary>
	class EntitySystem
	{
		#region static

		private static EntitySystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new EntitySystem();
		}

		static public void Dispose()
		{
			// clean entityes
			var entityTable = s_singleton.m_entityTable;
			foreach (var entities in entityTable.Values)
			{
				foreach (var entity in entities)
				{
					entity.Dispose();
				}
			}
			s_singleton.m_entityTable = null;

			// check update line count
			foreach (var line in s_singleton.m_lineTable.Values)
			{
				Debug.Assert(line.Count == 0, "component still remains in update line");
			}
			s_singleton.m_lineTable = null;
		}

		static public EntitySystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region properties

		#endregion // properties

		/// <summary>
		/// Add new entity to EntitySystem
		/// </summary>
		/// <param name="entity">entity</param>
		public void AddEntity(GameEntity entity)
		{
			string name = entity.Name;
			List<GameEntity> entities = null;
			if (!m_entityTable.TryGetValue(name, out entities))
			{
				entities = new List<GameEntity>();
				m_entityTable.Add(name, entities);
			}


			entities.Add(entity);
		}

		/// <summary>
		/// Remove entity from EntitySystem
		/// </summary>
		/// <param name="entity">entity</param>
		/// <remarks>
		/// This method calls GameEntity.Dispose() implicitly.
		/// </remarks>
		public void RemoveEntity(GameEntity entity)
		{
			string name = entity.Name;
			List<GameEntity> entities = null;
			if (m_entityTable.TryGetValue(name, out entities))
			{
				if (entities.Remove(entity))
				{
					entity.Dispose();
					if (entities.Count == 0)
					{
						// delete entry
						m_entityTable.Remove(name);
					}
				}
			}
		}

		/// <summary>
		/// Find all entities which has the indicated name
		/// </summary>
		/// <param name="name">name</param>
		/// <returns>entity list or null</returns>
		public List<GameEntity> FindEntitiesByName(string name)
		{
			List<GameEntity> entities = null;
			if (m_entityTable.TryGetValue(name, out entities))
			{
				Debug.Assert(entities.Count != 0, "no item entry found");
				return entities;
			}
			else
			{
				// not found
				return null;
			}
		}

		/// <summary>
		/// Find entity which has the indicated name
		/// </summary>
		/// <param name="name">name</param>
		/// <returns>fond item or null</returns>
		/// <remarks>
		/// this methods assumes that no other entities has same name.
		/// </remarks>
		public GameEntity FindEntityByName(string name)
		{
			var entities = FindEntitiesByName(name);
			if (entities == null)
			{
				// not found
				return null;
			}

			Debug.Assert(entities.Count == 1, "multi entities are contained. use FindEntitiesByName().");
			return entities.ElementAt(0);
		}

		/// <summary>
		/// Get GameEntityComponentLine which has the indicated UpdateLine
		/// </summary>
		/// <param name="updateLine"></param>
		/// <returns></returns>
		public GameEntityComponentLine GetComponentLine(GameEntityComponent.UpdateLines updateLine)
		{
			GameEntityComponentLine result = null;
			if (!m_lineTable.TryGetValue(updateLine, out result))
			{
				result = new GameEntityComponentLine(updateLine);
				m_lineTable.Add(updateLine, result);
			}

			return result;
		}

		/// <summary>
		/// Update components of line
		/// </summary>
		/// <param name="updateLine">update component line</param>
		/// <param name="dT">spend time [sec]</param>
		public void UpdateComponents(GameEntityComponent.UpdateLines updateLine, double dT)
		{
			Debug.Assert(updateLine != GameEntityComponent.UpdateLines.Invalid, "UpdateLine=Invalid is not updated.");
			GetComponentLine(updateLine).Update(dT);
		}
			 
		#region private members

		/// <summary>
		/// All entity collection
		/// </summary>
		/// <remarks>
		/// Key is entity's Name. 
		/// Value is entities which has same name.
		/// </remarks>
		private Dictionary<String, List<GameEntity>> m_entityTable = null;

		/// <summary>
		/// All component update line collection
		/// </summary>
		private Dictionary<GameEntityComponent.UpdateLines, GameEntityComponentLine> m_lineTable = null;

		#endregion // private members

		#region private methods

		private EntitySystem()
		{
			int capacity = 1024;
			m_entityTable = new Dictionary<String, List<GameEntity>>(capacity);
			m_lineTable = new Dictionary<GameEntityComponent.UpdateLines, GameEntityComponentLine>();
		}

		#endregion // private methods
	}
}


