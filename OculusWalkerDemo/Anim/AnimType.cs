using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;

namespace Oggy
{
    public static class AnimType
    {
        public struct KeyData<Type>
        {
            /// <summary>
            /// a position of this key at timeline
            /// </summary>
            /// <remarks>
            /// the top of frame is 1
            /// </remarks>
            public int Frame;

            /// <summary>
            /// a value of this key
            /// </summary>
            public Type Value;

            public KeyData(int frame, Type value)
            {
                Frame = frame;
                Value = value;
            }
        }

        public struct ChannelData<Type>
        {
            /// <summary>
            /// an array of key
            /// </summary>
            public KeyData<Type>[] KeyFrames;

            public static ChannelData<Type> Empty()
            {
                var channel = new ChannelData<Type>();
                channel.KeyFrames = null;
                return channel;
            }
        }

        /// <summary>
        /// this structure represents one of the animations for a bone
        /// </summary>
        public struct ActionGroupData
        {
            /// <summary>
            /// a bone name which is animated by this data
            /// </summary>
            public string BoneName;

            public ChannelData<Vector3> Location;

            public ChannelData<Quaternion> Rotation;

            public ChannelData<Vector3> Scale;
        }

        /// <summary>
        /// this structure represents one of the animations for a model
        /// </summary>
        public struct ActionData
        {
            /// <summary>
            /// a name of this animation action
            /// </summary>
            public string Name;

            /// <summary>
            /// an array of group
            /// </summary>
            public ActionGroupData[] Groups;
        }

        /// <summary>
        /// this structure represents set of animations for a model
        /// </summary>
        public struct AnimationData
        {
            /// <summary>
            /// an array of action
            /// </summary>
            public ActionData[] Actions;

        }

        /// <summary>
        /// sampled value type
        /// </summary>
        public struct SampleValue
        {
            public Vector3 Location;
            public Quaternion Rotation;
            public float Scaling;

            public Matrix GetMatrix()
            {
                Quaternion q = Rotation;
                q.Normalize();
                return Matrix.AffineTransformation(Scaling, Rotation, Location);
            }
        }
    }
}
