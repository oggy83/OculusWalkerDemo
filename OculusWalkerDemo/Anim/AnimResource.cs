using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class AnimResource : ResourceBase
	{
		private AnimType.AnimationData? m_animData;
		public AnimType.AnimationData? Data
		{
			get
			{
				return m_animData;
			}
		}

		public AnimResource(String uid)
		: base(uid)
		{
			m_animData = null;
		}

		public static AnimResource FromBlenderScene(String uid, BlenderScene scene)
		{
			var animeRes = new AnimResource(uid);

			foreach (var n in scene.NodeList)
			{
				if (animeRes.m_animData == null && n.Animation != null)
				{
					// found anim res
					animeRes.m_animData = n.Animation;
				}
			}

			return animeRes;
		}
	}
}
