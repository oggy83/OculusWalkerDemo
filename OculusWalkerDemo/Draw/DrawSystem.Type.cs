using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Oggy
{
	public partial class DrawSystem
	{
		/// <summary>
		/// directional light 
		/// </summary>
		public struct DirectionalLightData
		{
			/// <summary>
			/// direction in world coords
			/// </summary>
			public Vector3 Direction;

			/// <summary>
			/// color 
			/// </summary>
			public Color3 Color;

			public DirectionalLightData(Vector3 dir, Color3 col)
			{
				this.Direction = dir;
				this.Color = col;
			}
		}

        /// <summary>
        /// camera
        /// </summary>
        public struct CameraData
        {
            public Vector3 eye;
            public Vector3 lookAt;
            public Vector3 up;

            public CameraData(Vector3 eye, Vector3 lookAt, Vector3 up)
            {
                this.eye = eye;
                this.lookAt = lookAt;
                this.up = up;
            }

            public Matrix GetViewMatrix()
            {
                return Matrix.LookAtLH(eye, lookAt, up);
            }

			public static CameraData GetIdentity()
			{
				return new CameraData(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			}

			public static CameraData Conbine(CameraData a, CameraData b)
			{
				var z = (a.lookAt - a.eye);
				z.Normalize();
				var x = Vector3.Cross(a.up, z);
				x.Normalize();
				var y = Vector3.Cross(z, x);

				var trans = new Matrix3x3(x.X, x.Y, x.Z, y.X, y.Y, y.Z, z.X, z.Y, z.Z);

				var result = new CameraData(
					MathUtil.Transform3(b.eye, trans),
					MathUtil.Transform3(b.lookAt, trans), 
					MathUtil.Transform3(b.up, trans));
				result.eye += a.eye;
				result.lookAt += a.eye;
				result.up.Normalize();
				return result;
			}
        }

		/// <summary>
		/// world data (camera + light)
		/// </summary>
		public struct WorldData
		{
			public CameraData Camera;
			public Color3 AmbientColor;
			public Color3 FogColor;
			public DrawSystem.DirectionalLightData DirectionalLight;
            public float NearClip;
            public float FarClip;
		}

		public struct D3DData
		{
			public Device Device;
			public SwapChain SwapChain;
			public IntPtr WindowHandle;
		}

		public enum RenderMode
		{
			Opaque = 0,		// disable alpha blend
			Transparency,	// enable alpha blend
		}

        public enum TextureTypes
        {
            Diffuse0 = 0,
            Bump0,
        }

        public enum MaterialTypes
        {
            Default = 0,
            DbgBoneWeight,
            Marker,
        }

        public struct TextureData
        {
            public TextureView Resource { get; set; }

            public Vector2 UvScale { get; set; }

            public static TextureData Null()
            {
                return new TextureData
                {
                    Resource = null,
                    UvScale = Vector2.One,
                };
            }
        }

        public struct MaterialData
        {
            public MaterialTypes Type { get; set; }

            /// <summary>
            /// first diffuse texture for Default
            /// </summary>
            public TextureData DiffuseTex0 { get; set; }

            /// <summary>
            /// first bump texture for Default
            /// </summary>
            public TextureData BumpTex0 { get; set; }

            /// <summary>
            /// index of showing bone weight for DbgBoneWeight
            /// </summary>
            public int DbgBoneIndex;

            /// <summary>
            /// marker id for Marker
            /// </summary>
            public int MarkerId;
        }

        public struct MeshData
        {
            public int VertexCount { get; set; }
            public VertexBufferBinding[] Buffers { get; set; }
            public PrimitiveTopology Topology { get; set; }

            public static MeshData Create(int bufferCount)
            {
                var data = new MeshData()
                {
                    VertexCount = 0,
                    Buffers = new VertexBufferBinding[bufferCount],
                    Topology = PrimitiveTopology.TriangleList
                };

                return data;
            }
        }

        public struct BoneData
        {
            /// <summary>
            /// bone name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// index of the parent bone
            /// </summary>
            /// <remarks>in case of master bone, this is set to -1</remarks>
            public int Parent { get; set; }

            /// <summary>
            /// bone matrix which transforms parent bone coords system to this bone coords system
            /// </summary>
            public Matrix BoneTransform { get; set; }

            /// <summary>
            /// bone matrix which transforms model coords system to this bone coords system
            /// </summary>
            public Matrix BoneOffset { get; set; }

            /// <summary>
            /// bone length
            /// </summary>
            public float Length { get; set; }

            public bool IsMasterBone()
            {
                return Parent == -1;
            }

        }

		public struct EyeData
		{
			/// <summary>
			/// get/set a view matrix 
			/// </summary>
			public Matrix ViewMatrix { get; set; }

			/// <summary>
			/// get/set a projection matrix 
			/// </summary>
			public Matrix ProjectionMatrix { get; set; }

			/// <summary>
			/// get/set a camera eye position
			/// </summary>
			public Vector3 EyePosition { get; set; }
		}

	}
}
