using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public partial class Coroutine
	{
		public Coroutine()
		{
			m_callStack = new List<_StackData>();
		}

		public void Start(CoroutineTask task)
		{
			Stop();
			m_callStack.Add(new _StackData() { Process = task.Execute(), Task = task });
		}

		public void Stop()
		{
			m_callStack.Clear();
		}

		public void Update(double dt)
		{
			foreach (var elem in m_callStack)
			{
				elem.Task.Update(dt);
			}

			if (m_callStack.Count() != 0)
			{
				int lastIndex = m_callStack.Count() - 1;
				_StackData lastElem = m_callStack[lastIndex];
				if (lastElem.Process.MoveNext())
				{
					// has next element
					CoroutineTask newTask = lastElem.Process.Current;
					if (newTask != null)
					{
						// stack new sub-coroutine
						m_callStack.Add(new _StackData() { Process = newTask.Execute(), Task = newTask });
					}
				}
				else
				{
					// finish current coroutine
					m_callStack.RemoveAt(lastIndex);
				}
			}
		}

		public bool HasCompleted()
		{
			return m_callStack.Count == 0;
		}

		#region private types

		private struct _StackData
		{
			/// <summary>
			/// result of Coroutine.Execute()
			/// </summary>
			public IEnumerator<CoroutineTask> Process;

			/// <summary>
			/// task
			/// </summary>
			public CoroutineTask Task; 
		}

		#endregion // private types

		#region private members

		private List<_StackData> m_callStack;

		#endregion // private members
	}
}
