using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public partial class Coroutine
	{

		public class WaitDelegate : CoroutineTask
		{
			/// <summary>
			/// wait function type
			/// </summary>
			/// <returns></returns>
			/// <remarks>
			/// if return value is false, this coroutine finish
			/// </remarks>
			public delegate bool WaitFunc();

			public WaitDelegate(WaitFunc func)
			{
				m_func = func;
			}

			public override IEnumerator<CoroutineTask> Execute()
			{
				while (!m_func())
				{
					yield return Continue();
				}

				yield break;
			}

			private WaitFunc m_func;
		}
	}
}
