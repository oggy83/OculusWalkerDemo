using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Oggy
{
	/// <summary>
	/// draw model class
	/// </summary>
	public partial class DrawModel : IDisposable// : ResourceBase
	{
        #region properties

        private List<Node> m_nodeList = null;
        public ReadOnlyCollection<Node> NodeList
        {
            get
            {
                return m_nodeList.AsReadOnly();
            }
        }

        /// <summary>
        /// get a bone array 
        /// </summary>
        /// <remarks>in case of bone count is 0, BoneArray is null</remarks>
        private DrawSystem.BoneData[] m_boneArray = null;
        public DrawSystem.BoneData[] BoneArray
        {
            get
            {
                return m_boneArray;
            }
        }

		private Aabb m_bb;
		public Aabb BoundingBox
		{
			get
			{
				return m_bb;
			}
		}

        #endregion // properties

        #region inner class

        public class Node
        {
            public DrawSystem.MeshData Mesh;
            public MaterialBase Material;
            public bool IsDebug;
            public bool HasBone;
        }

        private struct _VertexCommon
        {
            public Vector4 Position;
            public Vector3 Normal;
            public Vector2 Texcoord;
        }

        private struct _VertexBoneWeight
        {
            public const int MAX_COUNT = 4;

            public uint Index0;
            public uint Index1;
            public uint Index2;
            public uint Index3;

            public float Weight0;
            public float Weight1;
            public float Weight2;
            public float Weight3;
        }

        private struct _VertexDebug
        {
            public Vector4 Position;
            public Color4 Color;
        }

        #endregion // inner class

        public DrawModel(String uid)
            //: base(uid)
        {
         
            m_nodeList = new List<Node>();
            m_boneArray = null;
			m_bb = new Aabb(Vector3.Zero, Vector3.Zero);
        }

		public static DrawModel FromScene(String uid, BlenderScene scene)
        {
			return FromScene(uid, scene, "Image/");
		}

		public static DrawModel FromScene(String uid, BlenderScene scene, string fileSearchPath)
        {
            var drawSys = DrawSystem.GetInstance();
            var drawRepository = drawSys.ResourceRepository;
            var d3d = drawSys.D3D;

            var model = new DrawModel(uid);
            bool hasBone = BlenderUtil.GetLengthOf(scene.NodeList[0].BoneArray) > 0;// boneArray is set to the first node
			var aabb = Aabb.Invalid();
            foreach (var n in scene.NodeList)
            {
                if (n.MaterialData.Type == MaterialBase.MaterialTypes.Marker)
                {
					// marker material is used as 'Marker'
                    continue;
                }

                if (n.Vertics.Count() == 0)
                {
                    // empty vertex list
                    continue;
                }

                // Build a vertex buffer(s)
                var vertices1 = n.Vertics
                    .Select(v => new _VertexCommon()
                    {
                        Position = v.Position,
                        Normal = v.Normal,
                        Texcoord = v.Texcoord
                    }).ToArray();

                var vertices2 = n.Vertics
                    .Select(v => v.Tangent).ToArray();

				// update aabb
				foreach (var v in vertices1)
				{
					aabb.ExtendByPoint(MathUtil.ToVector3(v.Position));
				}

                var node = new Node();
                node.Material = n.MaterialData;
                node.IsDebug = false;
                node.HasBone = hasBone;
                if (node.HasBone)
                {
                    // if model has bone, we create a bone vertex info
                    var vertices3 = n.Vertics
                       .Select(v =>
                       {
                           Debug.Assert(BlenderUtil.GetLengthOf(v.BoneIndices) == BlenderUtil.GetLengthOf(v.BoneWeights), "both of bone index and bone weight must be matched");
                           //Debug.Assert(BlenderUtil.GetLengthOf(v.BoneWeights) <= _VertexBoneWeight.MAX_COUNT, "length of bone weight is over :" + BlenderUtil.GetLengthOf(v.BoneWeights));
                           Debug.Assert(BlenderUtil.GetLengthOf(v.BoneWeights) != 0, "no bone entry");
                           var tmp = new _VertexBoneWeight()
                           {
                               Index0 = BlenderUtil.GetLengthOf(v.BoneIndices) > 0 ? v.BoneIndices[0] : 0,
                               Weight0 = BlenderUtil.GetLengthOf(v.BoneWeights) > 0 ? v.BoneWeights[0] : 0.0f,
                               Index1 = BlenderUtil.GetLengthOf(v.BoneIndices) > 1 ? v.BoneIndices[1] : 0,
                               Weight1 = BlenderUtil.GetLengthOf(v.BoneWeights) > 1 ? v.BoneWeights[1] : 0.0f,
                               Index2 = BlenderUtil.GetLengthOf(v.BoneIndices) > 2 ? v.BoneIndices[2] : 0,
                               Weight2 = BlenderUtil.GetLengthOf(v.BoneWeights) > 2 ? v.BoneWeights[2] : 0.0f,
                               Index3 = BlenderUtil.GetLengthOf(v.BoneIndices) > 3 ? v.BoneIndices[3] : 0,
                               Weight3 = BlenderUtil.GetLengthOf(v.BoneWeights) > 3 ? v.BoneWeights[3] : 0.0f,
                           };
                           float sumWeight = tmp.Weight0 + tmp.Weight1 + tmp.Weight2 + tmp.Weight3;
                           tmp.Weight0 /= sumWeight;
                           tmp.Weight1 /= sumWeight;
                           tmp.Weight2 /= sumWeight;
                           tmp.Weight3 /= sumWeight;
                           return tmp;
                       }).ToArray();

                    node.Mesh = DrawUtil.CreateMeshData(d3d, PrimitiveTopology.TriangleList, vertices1, vertices2, vertices3);
                }
                else
                {
                    node.Mesh = DrawUtil.CreateMeshData(d3d, PrimitiveTopology.TriangleList, vertices1, vertices2);
                }
               
                // add dispoable
/*
foreach (var buf in node.Mesh.Buffers)
{
    model._AddDisposable(buf.Buffer);
}
*/

                // create skeleton
                if (model.m_boneArray == null && n.BoneArray != null)
                {
                    model.m_boneArray = (DrawSystem.BoneData[])n.BoneArray.Clone();
                }

                // load new texture
                foreach (var texInfo in n.TextureInfos.Values)
                {
                    if (!drawRepository.Contains(texInfo.Name))
                    {
                        var tex = TextureView.FromFile(texInfo.Name, drawSys.D3D, Path.Combine(fileSearchPath, texInfo.Name));
                        drawRepository.AddResource(tex);
                    }
                }

                // copy textures from cache
                if (n.TextureInfos.ContainsKey(DrawSystem.TextureTypes.Diffuse0))
                {
                    node.Material.SetTextureData(
						DrawSystem.TextureTypes.Diffuse0, 
						new DrawSystem.TextureData
						{
							Resource = drawRepository.FindResource<TextureView>(n.TextureInfos[DrawSystem.TextureTypes.Diffuse0].Name),
							UvScale = n.TextureInfos[DrawSystem.TextureTypes.Diffuse0].UvScale,
						});
                }
                if (n.TextureInfos.ContainsKey(DrawSystem.TextureTypes.Bump0))
                {
                    node.Material.SetTextureData(
						DrawSystem.TextureTypes.Bump0,
						new DrawSystem.TextureData
						{
							Resource = drawRepository.FindResource<TextureView>(n.TextureInfos[DrawSystem.TextureTypes.Bump0].Name),
							UvScale = n.TextureInfos[DrawSystem.TextureTypes.Bump0].UvScale,
						});
                }

                model.m_nodeList.Add(node);
            }

			model.m_bb = aabb;
            return model;
        }

		

		public void Dispose()
		{
            foreach (var node in m_nodeList)
            {
                foreach (var buffer in node.Mesh.Buffers)
                {
                    buffer.Buffer.Dispose();
                }
            }

            m_nodeList.Clear();
		}

		public static DrawModel CreateBox(float geometryScale, float uvScale, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float gs = geometryScale;
			float us = uvScale;

			var vertices = new _VertexCommon[]
			{
				// top plane
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  gs, 1), Texcoord = new Vector2(us, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  -gs, 1), Texcoord = new Vector2(0, us), Normal = Vector3.UnitY },

				// bottom plane
				new _VertexCommon() { Position = new Vector4( -gs, -gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  gs, 1), Texcoord = new Vector2(us, 0), Normal = -Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs, -gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs, -gs,  -gs, 1), Texcoord = new Vector2(0, us), Normal = -Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitY },

				// forward plane
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  -gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  -gs, 1), Texcoord = new Vector2(us, 0), Normal = -Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  -gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( -gs,  -gs,  -gs, 1), Texcoord = new Vector2(0, us), Normal = -Vector3.UnitZ },

				// back plane
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  gs, 1), Texcoord = new Vector2(us, 0), Normal = Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( -gs,  -gs,  gs, 1), Texcoord = new Vector2(0, us), Normal = Vector3.UnitZ },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitZ },

				// left plane
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  -gs, 1), Texcoord = new Vector2(us, 0), Normal = -Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( -gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( -gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = -Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( -gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = -Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( -gs,  -gs,  gs, 1), Texcoord = new Vector2(0, us), Normal = -Vector3.UnitX },
				
				// right plane
				new _VertexCommon() { Position = new Vector4( gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  -gs, 1), Texcoord = new Vector2(us, 0), Normal = Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( gs,  gs,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  gs, 1), Texcoord = new Vector2(0, us), Normal = Vector3.UnitX },
				new _VertexCommon() { Position = new Vector4( gs,  -gs,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitX },
				
			};

			Aabb aabb = Aabb.Invalid();
			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				aabb.ExtendByPoint(MathUtil.ToVector3(vertices[i].Position));
			}

			var model = new DrawModel("");
			var mesh = DrawUtil.CreateMeshData<_VertexCommon>(d3d, PrimitiveTopology.TriangleList, vertices);
            model.m_nodeList.Add(new Node() { Mesh = mesh, Material = DebugMaterial.Create(), IsDebug = false, HasBone = false });
			model.m_bb = aabb;

			return model;
		}

		public static DrawModel CreateFloor(float geometryScale, float uvScale, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float gs = geometryScale;
			float us = uvScale;

			var vertices = new _VertexCommon[]
			{
				new _VertexCommon() { Position = new Vector4( -gs,  0,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  0,  gs, 1), Texcoord = new Vector2(us, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  0,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs,  0,  gs, 1), Texcoord = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( gs,  0,  -gs, 1), Texcoord = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexCommon() { Position = new Vector4( -gs,  0,  -gs, 1), Texcoord = new Vector2(0, us), Normal = Vector3.UnitY },
			};

			Aabb aabb = Aabb.Invalid();
			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				aabb.ExtendByPoint(MathUtil.ToVector3(vertices[i].Position));
			}

			var model = new DrawModel("");
			var mesh = DrawUtil.CreateMeshData<_VertexCommon>(d3d, PrimitiveTopology.TriangleList, vertices);
			model.m_nodeList.Add(new Node() { Mesh = mesh, Material = DebugMaterial.Create(), IsDebug = false, HasBone = false });
			model.m_bb = aabb;

			return model;
		}

        public static DrawModel CreateBone(String uid, float length, Color4 color, Vector4 offset)
        {
            var drawSys = DrawSystem.GetInstance();
            var d3d = drawSys.D3D;
            float w = 0.1f * length;
            float L = length;

            var vertices = new _VertexDebug[]
			{
				// bottom pyramid
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( -w, -w, w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( -w, w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w,  w, 1) },

				// middle plane
				new _VertexDebug() { Position = new Vector4( w,   w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( -w,  w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( -w,  w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( -w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( -w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w,  -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w,  -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w,   w,  w, 1) },

				// top pyramid
				new _VertexDebug() { Position = new Vector4( 0,   0,  L, 1) },
				new _VertexDebug() { Position = new Vector4( -w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,   0,  L, 1) },
				new _VertexDebug() { Position = new Vector4( w,  -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,   0,  L, 1) },
				new _VertexDebug() { Position = new Vector4( -w,  w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,   0,  L, 1) },
				new _VertexDebug() { Position = new Vector4( w,   w,  w, 1) },
			};

			Aabb aabb = Aabb.Invalid();
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].Position += offset;
                vertices[i].Position.W = 1;
                vertices[i].Color = color;
				aabb.ExtendByPoint(MathUtil.ToVector3(vertices[i].Position));
            }

            var model = new DrawModel(uid);
            model.m_nodeList.Add(new Node()
            {
                Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.LineList, vertices),
				Material = DebugMaterial.Create(),
                IsDebug = true,
                HasBone = false,
            });
			model.m_bb = aabb;

            return model;
        }

		public static DrawModel CreateBox(String uid, Aabb box, Color4 color)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;

			Vector3 min = box.Min;
			Vector3 max = box.Max;
			var vertices = new _VertexDebug[]
			{
				new _VertexDebug() { Position = new Vector4(min.X, min.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, min.Y, min.Z, 1) },

				new _VertexDebug() { Position = new Vector4(min.X, min.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, max.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, min.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, max.Y, max.Z, 1) },

				new _VertexDebug() { Position = new Vector4(min.X, max.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, min.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(max.X, max.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, max.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, max.Y, max.Z, 1) },
				new _VertexDebug() { Position = new Vector4(min.X, max.Y, min.Z, 1) },
			};

			Aabb aabb = Aabb.Invalid();
			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
				aabb.ExtendByPoint(MathUtil.ToVector3(vertices[i].Position));
			}

			var model = new DrawModel(uid);
			model.m_nodeList.Add(new Node()
			{
				Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.LineList, vertices),
				Material = DebugMaterial.Create(),
				IsDebug = true,
				HasBone = false,
			});
			model.m_bb = aabb;

			return model;
		}

		public static DrawModel CreateFrustum(String uid, Matrix viewProjectionMatrix, Color4 color)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			viewProjectionMatrix.Invert();

			var points = new Vector4[8];
			points[0] = Vector3.Transform(new Vector3(-1, -1, 0), viewProjectionMatrix);
			points[1] = Vector3.Transform(new Vector3(1, -1, 0), viewProjectionMatrix);
			points[2] = Vector3.Transform(new Vector3(-1, 1, 0), viewProjectionMatrix);
			points[3] = Vector3.Transform(new Vector3(1, 1, 0), viewProjectionMatrix);
			points[4] = Vector3.Transform(new Vector3(-1, -1, 1), viewProjectionMatrix);
			points[5] = Vector3.Transform(new Vector3(1, -1, 1), viewProjectionMatrix);
			points[6] = Vector3.Transform(new Vector3(-1, 1, 1), viewProjectionMatrix);
			points[7] = Vector3.Transform(new Vector3(1, 1, 1), viewProjectionMatrix);

			var vertices = new _VertexDebug[]
			{
				new _VertexDebug() { Position = points[0] },
				new _VertexDebug() { Position = points[1] },
				new _VertexDebug() { Position = points[1] },
				new _VertexDebug() { Position = points[3] },
				new _VertexDebug() { Position = points[3] },
				new _VertexDebug() { Position = points[2] },
				new _VertexDebug() { Position = points[2] },
				new _VertexDebug() { Position = points[0] },

				new _VertexDebug() { Position = points[4] },
				new _VertexDebug() { Position = points[5] },
				new _VertexDebug() { Position = points[5] },
				new _VertexDebug() { Position = points[7] },
				new _VertexDebug() { Position = points[7] },
				new _VertexDebug() { Position = points[6] },
				new _VertexDebug() { Position = points[6] },
				new _VertexDebug() { Position = points[4] },

				new _VertexDebug() { Position = points[0] },
				new _VertexDebug() { Position = points[4] },
				new _VertexDebug() { Position = points[1] },
				new _VertexDebug() { Position = points[5] },
				new _VertexDebug() { Position = points[2] },
				new _VertexDebug() { Position = points[6] },
				new _VertexDebug() { Position = points[3] },
				new _VertexDebug() { Position = points[7] },
			};

			Aabb aabb = Aabb.Invalid();
			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position.X /= vertices[i].Position.W;
				vertices[i].Position.Y /= vertices[i].Position.W;
				vertices[i].Position.Z /= vertices[i].Position.W;
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
				aabb.ExtendByPoint(MathUtil.ToVector3(vertices[i].Position));
			}

			var model = new DrawModel(uid);
			model.m_nodeList.Add(new Node()
			{
				Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.LineList, vertices),
				Material = DebugMaterial.Create(),
				IsDebug = true,
				HasBone = false,
			});
			model.m_bb = aabb;

			return model;
		}
	}


}
