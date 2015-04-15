using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Oggy
{
	public class GameEntityComponentLine
	{
		#region properties

		private GameEntityComponent.UpdateLines m_updteLine;
		public GameEntityComponent.UpdateLines UpdateLine
		{
			get
			{
				return m_updteLine;
			}
		}

		/// <summary>
		/// the number of components
		/// </summary>
		private int m_count = 0;
		public int Count
		{
			get
			{
				return m_count;
			}
		}

		#endregion // properties

		public GameEntityComponentLine(GameEntityComponent.UpdateLines updateLine)
		{
			m_updteLine = updateLine;

			m_head = new GameEntityComponent(updateLine);
			m_tail = new GameEntityComponent(updateLine);
			m_head.Next = m_tail;
			m_tail.Previous = m_head;
		}

		/// <summary>
		/// Add component to the tail of this line 
		/// </summary>
		/// <param name="component">component (same UpdateLine)</param>
		/// <remarks>
		/// This method is called by GameEntityComponent only.
		/// </remarks>
		public void AddComponent(GameEntityComponent component)
		{
			Debug.Assert(component.UpdateLine == m_updteLine, "defferent update line");

			GameEntityComponent position = m_tail.Previous;
			position.Next = component;
			component.Previous = position;

			component.Next = m_tail;
			m_tail.Previous = component;
			m_count++;
		}

		/// <summary>
		/// Remove component from this line
		/// </summary>
		/// <param name="component">component (same UpdateLine)</param>
		/// <remarks>
		/// This method is called by GameEntityComponent only.
		/// </remarks>
		public void RemoveComponent(GameEntityComponent component)
		{
			Debug.Assert(component.UpdateLine == m_updteLine, "defferent update line");
			Debug.Assert(component.Next != null, "next component is null");
			Debug.Assert(component.Previous != null, "previous component is null");

			GameEntityComponent prev = component.Previous;
			GameEntityComponent next = component.Next;

			prev.Next = next;
			next.Previous = prev;
			component.Previous = null;
			component.Next = null;
			m_count--;
		}

		/// <summary>
		/// Update all component of line
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public void Update(double dT)
		{
			GameEntityComponent component = m_head.Next;
			while (component != m_tail)
			{
				component.Update(dT);
				component = component.Next;
			}
		}

		#region private members

		/// <summary>
		/// dummy component which represents head of line
		/// </summary>
		GameEntityComponent m_head = null;

		/// <summary>
		/// dummy component which represents tail of line
		/// </summary>
		GameEntityComponent m_tail = null;

		#endregion // private members
	}
}
