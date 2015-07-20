using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Oggy
{
	public class AnimSampler
	{
		private delegate Type _IpoType<Type>(Type a, Type b, float t);

		#region properties

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

		#endregion // properties

		public AnimSampler(AnimType.ActionGroupData action)
		{
			m_groupData = action;

			int lastFrame = 0;
			lastFrame = Math.Max(lastFrame, m_groupData.Location.KeyFrames[m_groupData.Location.KeyFrames.Length - 1].Frame);
			lastFrame = Math.Max(lastFrame, m_groupData.Rotation.KeyFrames[m_groupData.Rotation.KeyFrames.Length - 1].Frame);
			lastFrame = Math.Max(lastFrame, m_groupData.Scale.KeyFrames[m_groupData.Scale.KeyFrames.Length - 1].Frame);
			m_lastFrame = lastFrame;
		}

		public AnimType.SampleValue Sample(float frame)
		{
            Vector3 location = _Sample<Vector3>(frame, m_groupData.Location, Vector3.Zero, AnimUtil.IpoVec3);
			Quaternion rotation = _Sample<Quaternion>(frame, m_groupData.Rotation, Quaternion.Zero, AnimUtil.IpoQuat);
			Vector3 scaling = _Sample<Vector3>(frame, m_groupData.Scale, Vector3.One, AnimUtil.IpoVec3);

			return new AnimType.SampleValue()
			{
				Location = location,
				Rotation = rotation,
				Scaling = scaling.X,
			};
		}

		#region private members

		/// <summary>
		/// animation data
		/// </summary>
		private AnimType.ActionGroupData m_groupData;

		#endregion // private members

		#region private methods

		private Type _Sample<Type>(float frame, AnimType.ChannelData<Type> channel, Type defaultValue, _IpoType<Type> ipo)
		{
			if (channel.KeyFrames.Count() == 0)
			{
				return defaultValue;
			}
			else if (frame < channel.KeyFrames[0].Frame)
			{
				float t = frame / (float)channel.KeyFrames[0].Frame;
				return ipo(defaultValue, channel.KeyFrames[0].Value, t);
			}

			int count = channel.KeyFrames.Count();
			for (int index = 0; index < count; ++index)
			{
				if (frame < channel.KeyFrames[index].Frame)
				{
					float t = (frame - (float)channel.KeyFrames[index - 1].Frame) / ((float)channel.KeyFrames[index].Frame - (float)channel.KeyFrames[index - 1].Frame);
					return ipo(channel.KeyFrames[index - 1].Value, channel.KeyFrames[index].Value, t);
				}
			}

			// last frame
			return channel.KeyFrames[count - 1].Value;
		}

		#endregion // private methods
	}
}
