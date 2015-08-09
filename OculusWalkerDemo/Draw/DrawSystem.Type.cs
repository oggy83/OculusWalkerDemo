﻿using System;
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
        }

		/// <summary>
		/// world data (camera + light)
		/// </summary>
		public struct WorldData
		{
			public Matrix camera;
			public Color3 ambientCol;
			public Color3 fogCol;
			public DrawSystem.DirectionalLightData dirLight;
		}

		public struct D3DData
		{
			public Device Device;
			public SwapChain SwapChain;
			public IntPtr WindowHandle;
		}

        /*
        public struct TextureData
        {
            public string Name { get; set; }

            public static TextureData FromName(string name)
            {
                var data = new TextureData()
                {
                    Name = name,
                };

                return data;
            }
        }
        */

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

	}
}
