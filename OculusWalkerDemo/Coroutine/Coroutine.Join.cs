using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public partial class Coroutine
	{
		/// <summary>
		/// this class join multiple coroutine tasks as sequence
		/// </summary>
		public class JoinedTask : CoroutineTask
		{
			public JoinedTask()
			{
			}

			public override void Update(double dt)
			{
				if (m_currentIndex < m_list.Count())
				{
					m_list[m_currentIndex].Update(dt);
				}
			}

			public override IEnumerator<CoroutineTask> Execute()
			{
				while (m_currentIndex < m_list.Count())
				{
					yield return m_list[m_currentIndex];
					m_currentIndex++;
				}

				yield break;
			}

			public void Add(CoroutineTask task)
			{
				m_list.Add(task);
			}

			private List<CoroutineTask> m_list = new List<CoroutineTask>();
			private int m_currentIndex = 0;

		}

		public static CoroutineTask Join(CoroutineTask task1)
		{
			return task1;
		}

		public static CoroutineTask Join(CoroutineTask task1, CoroutineTask task2)
		{
			var result = new JoinedTask();
			result.Add(task1);
			result.Add(task2);
			return result;
		}
	}

	
}
