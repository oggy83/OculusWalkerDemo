using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	/// <summary>
	/// This class contains multiple animation player, and apply mixed sampling values to skeleton.
	/// </summary>
	public class LayeredAnimPlayer
	{
		public LayeredAnimPlayer(Skeleton skeleton)
		{
			m_skeleton = skeleton;
		}

		public AnimHandle Play(AnimType.ActionData action)
		{
			var player = new AnimPlayer();
			var handle = player.Play(m_skeleton, action);
			if (handle == null)
			{
				return null;
			}

			var entry = new _Entry()
			{
				Player = player,
				Handle = handle,
			};
			m_entryList.Add(entry);

			return handle;
		}

		public void Update(double dt)
		{
			if (m_entryList.Count == 0)
			{
				return;
			}

			// sample and apply
			for (int boneIndex = 0; boneIndex < m_skeleton.GetBoneCount(); ++boneIndex)
			{
                AnimType.SampleValue value = m_entryList[0].Player.Sample(boneIndex);
                float sumWeight = m_entryList[0].Handle.Weight;

                for (int entryIndex = 1; entryIndex < m_entryList.Count; ++entryIndex)
                {
                    float weight = m_entryList[entryIndex].Handle.Weight;
                    sumWeight += weight;
                    if (sumWeight == 0)
                    {
                        continue;
                    }

                    float rate = weight / sumWeight;

                    AnimType.SampleValue tmpValue = m_entryList[entryIndex].Player.Sample(boneIndex);
                    value.Location = AnimUtil.IpoVec3(value.Location, tmpValue.Location, rate);
                    value.Rotation = AnimUtil.IpoQuat(value.Rotation, tmpValue.Rotation, rate);
                    value.Scaling = AnimUtil.IpoFloat(value.Scaling, tmpValue.Scaling, rate);
                }

                m_skeleton.SetBoneMatrix(boneIndex, value.GetMatrix());
			}
			
			// update player
			foreach (var entry in m_entryList)
			{
				entry.Player.Update(dt * entry.Handle.Speed, false);
			}
		}

		#region private types

		private struct _Entry
		{
			public AnimHandle Handle;
			public AnimPlayer Player;
		}

		#endregion // private types

		#region private members

		private Skeleton m_skeleton;

		private List<_Entry> m_entryList = new List<_Entry>();

		#endregion // private members
	}
}
