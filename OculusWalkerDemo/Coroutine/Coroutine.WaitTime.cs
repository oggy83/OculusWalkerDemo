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
		/// wait time coroutine
		/// </summary>
		public class WaitTime : CoroutineTask
		{
			public WaitTime(double waitTime)
			{
				m_waitTime = waitTime;
			}

			public override void Update(double dt)
			{
				m_waitTime -= dt;
			}

			public override IEnumerator<CoroutineTask> Execute()
			{
				while (m_waitTime > 0.0)
				{
					yield return Continue();
				}

				yield break;
			}

			/// <summary>
			/// remain wait time [sec]
			/// </summary>
			private double m_waitTime;
		}
	}
}
