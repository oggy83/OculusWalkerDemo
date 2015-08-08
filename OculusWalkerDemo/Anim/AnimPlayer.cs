using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Oggy
{
	/// <summary>
	/// This class samples value from animation resource and apply it to skeleton.
	/// </summary>
	public class AnimPlayer
	{
		private const double FPS = 30;

		/// <summary>
		/// get a miximum frame number in given action group
		/// </summary>
		private int m_lastFrame = 0;
		public int LastFrame
		{
			get
			{
				return m_lastFrame;
			}
		}

		public AnimPlayer()
		{
			Stop();
		}

		/// <summary>
		/// start play a new animation
		/// </summary>
		/// <param name="skeleton"></param>
		/// <param name="action"></param>
		/// <returns>anim handle or null</returns>
		public AnimHandle Play(Skeleton skeleton, AnimType.ActionData action)
		{
			if (m_isPlaying)
			{
				Debug.Fail("old animation still playing");
				return null;
			}

			if (skeleton.GetBoneCount() == 0)
			{
				// ignore
				return null;
			}

			m_isPlaying = true;
			m_dt = 0.0f;
			m_skeleton = skeleton;
			m_animData = action;

			// initialize mapper
			m_boneIndex2groupIndexMapper = new int[m_skeleton.GetBoneCount()];
			m_samplers = new AnimSampler[m_skeleton.GetBoneCount()];

			for (int boneIndex = 0; boneIndex < m_skeleton.GetBoneCount(); ++boneIndex)
			{
				string boneName = m_skeleton.GetBoneName(boneIndex);
				int groupIndex = m_animData.Groups.Select((g, index) => new { g, index }).First(v => v.g.BoneName == boneName).index;
				m_boneIndex2groupIndexMapper[boneIndex] = groupIndex;

				m_samplers[boneIndex] = new AnimSampler(m_animData.Groups[groupIndex]);
			}

			// compute last frame
			int lastFrame = 0;
			for (int boneIndex = 0; boneIndex < m_skeleton.GetBoneCount(); ++boneIndex)
			{
				lastFrame = Math.Max(lastFrame, m_samplers[boneIndex].LastFrame);
			}
			m_lastFrame = lastFrame;

			return new AnimHandle()
			{
				Name = action.Name,
				Weight = 1.0f,
                Speed = 1.0f,
			};
		}

		public void Stop()
		{
			m_isPlaying = false;
			m_dt = 0.0f;
			m_skeleton = null;
			m_boneIndex2groupIndexMapper = null;
		}

		public void Update(double dt, bool enableApply)
		{
			if (!m_isPlaying)
			{
				return;
			}

			if (enableApply)
			{
				for (int boneIndex = 0; boneIndex < m_skeleton.GetBoneCount(); ++boneIndex)
				{
					m_skeleton.SetBoneMatrix(boneIndex, Sample(boneIndex).GetMatrix());
				}
			}

			m_dt += dt;
			m_dt = m_dt % ((float)LastFrame / FPS);
		}

		public AnimType.SampleValue Sample(int boneIndex)
		{
			float frame = (float)(m_dt * FPS);
			return m_samplers[boneIndex].Sample(frame);
		}


		#region private members

		/// <summary>
		/// state playing
		/// </summary>
		private bool m_isPlaying;

		/// <summary>
		/// the amount of delta time [sec]
		/// </summary>
		private double m_dt = 0.0f;

		/// <summary>
		/// target skeleton to apply sampled value
		/// </summary>
		private Skeleton m_skeleton;

		/// <summary>
		/// animation data
		/// </summary>
		private AnimType.ActionData m_animData;

		/// <summary>
		/// mapping from bone index to group index
		/// </summary>
		/// <remarks>
		/// If no entry in group, the element is stored -1.
		/// </remarks>
		private int[] m_boneIndex2groupIndexMapper;

		/// <summary>
		/// list of sampler 
		/// </summary>
		private AnimSampler[] m_samplers;

		#endregion // private members
	}
}
