using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class AnimHandle
	{
		/// <summary>
		/// action name
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// blending weight
		/// </summary>
		public float Weight
		{
			get;
			set;
		}

        /// <summary>
        /// play speed (default is 1.0)
        /// </summary>
		public float Speed
		{
			get;
			set;
		}

		/// <summary>
		/// get wheather handle is valid or not
		/// </summary>
		private bool m_isValid;
		public bool IsValid
		{
			get
			{
				return m_isValid;
			}
		}

		public AnimHandle(string name, float weight, float speed)
		{
			Name = name;
			Weight = weight;
			Speed = speed;
			m_isValid = true;
		}

		public static AnimHandle Invalid()
		{
			var handle = new AnimHandle("", 0.0f, 1.0f);
			handle.m_isValid = false;
			return handle;
		}
	}
}
