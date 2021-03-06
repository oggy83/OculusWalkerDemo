﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;

namespace Oggy
{
	public partial class BlenderScene
	{
		public struct VertexOriginal
		{
			public Vector4 Position;
			public Vector3 Normal;
			public Vector3 Tangent;
			public Vector3 Binormal;
			public Vector2 Texcoord;
			public int MaterialIndex;
			public float[] BoneWeights;
			public uint[] BoneIndices;
		}

        public struct TextureInfo
        {
            public string Name;
            public Vector2 UvScale;
        }

		public struct SceneNode
		{
			public string Name { get; set; }

			public VertexOriginal[] Vertics { get; set; }

			public MaterialBase MaterialData { get; set; }

			public Dictionary<DrawSystem.TextureTypes, TextureInfo> TextureInfos { get; set; }

			/// <summary>
			/// get/set a bone array (optional)
			/// </summary>
			public DrawSystem.BoneData[] BoneArray { get; set; }

			/// <summary>
			/// get/set a model animation (optional)
			/// </summary>
			public AnimType.AnimationData? Animation { get; set; }
		}

		public struct LinkNode
		{
			public string Name { get; set; }

			public Matrix Layout { get; set; }

			public string TargetFileName { get; set; }
		}

		public class CustomProperty
		{
			public string Name;
			public int Value;
		}
	}
}
