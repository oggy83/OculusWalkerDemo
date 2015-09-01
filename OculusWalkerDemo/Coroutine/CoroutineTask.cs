using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class CoroutineTask
	{
		public enum Result
		{
			Ok = 0,
			Error,
		}

		public virtual void Update(double dt)
		{
			// do nothing
		}

		public virtual IEnumerator<CoroutineTask> Execute()
		{
			yield break;
		}

		/// <summary>
		/// get yield return value that coroutine continues process
		/// </summary>
		/// <returns>null</returns>
		/// <remarks>
		/// yield return Cotinue()
		/// </remarks>
		public CoroutineTask Continue()
		{
			return null;
		}

		/// <summary>
		/// get result of coroutine
		/// </summary>
		/// <returns>result</returns>
		public virtual Result GetResult()
		{
			return Result.Ok;
		}
	}
}
