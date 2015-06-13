using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;

namespace Oggy
{
	/// <summary>
	/// this class represents a set of bone (it is called 'armature' by Blender)
	/// </summary>
	public class Skeleton
	{
		public Skeleton(DrawSystem.BoneData[] boneArray)
		{
			m_masterBoneIndex = 0;
			m_nodeArray = null;

			if (boneArray == null || boneArray.Length < 1)
			{
				Debug.Fail("empty bone array");
			}
			else
			{
				int boneIndex = 0;
				m_nodeArray = boneArray.Select(b => new _Node() { Index = boneIndex++, Bone = b, LocalTransform = Matrix.Identity, DbgDrawModel = null }).ToArray();

				// find master bone
				for (int i = 0; i < m_nodeArray.Length; ++i)
				{
					if (m_nodeArray[i].Bone.IsMasterBone())
					{
						m_masterBoneIndex = i;
						break;
					}
				}
			}
		}

		/// <summary>
		/// get matrix which transforms this bone coords system to parent bone coords sytem
		/// </summary>
		/// <param name="boneIndex">bone index</param>
		/// <returns>bone transform</returns>
		public Matrix GetBoneTransform(int boneIndex)
		{
			return m_nodeArray[boneIndex].LocalTransform;
		}

		/// <summary>
		/// get a matrix which transforms this bone coords system to model coords system
		/// </summary>
		/// <param name="boneIndex"></param>
		/// <returns></returns>
		public Matrix GetModelTransform(int boneIndex)
		{
			// todo : improve performance!!
			var nextNode = m_nodeArray[boneIndex];
			Matrix modelTrans = nextNode.LocalTransform * nextNode.Bone.BoneTransform;

			while (true)
			{
				if (nextNode.Bone.IsMasterBone())
				{
					break;
				}

				nextNode = m_nodeArray[nextNode.Bone.Parent];
				modelTrans = modelTrans * nextNode.LocalTransform * nextNode.Bone.BoneTransform;
			}

			return modelTrans;
		}


		/// <summary>
		/// get a matrix for skinning
		/// </summary>
		/// <param name="boneIndex"></param>
		/// <returns></returns>
		public Matrix GetSkinningTransform(int boneIndex)
		{
			return m_nodeArray[boneIndex].Bone.BoneOffset * GetModelTransform(boneIndex);
		}

		/// <summary>
		/// get all matrices for skinning
		/// </summary>
		/// <param name="boneIndex"></param>
		/// <returns></returns>
		public Matrix[] GetAllSkinningTransforms()
		{
			var result = new Matrix[m_nodeArray.Length];
			for (int nodeIndex = 0; nodeIndex < m_nodeArray.Length; ++nodeIndex)
			{
				result[nodeIndex] = GetSkinningTransform(nodeIndex);
			}

			return result;
		}

		/// <summary>
		/// set a matrix which represents nth local transformation.
		/// </summary>
		/// <param name="matrix">bone matrix</param>
		/// <param name="boneIndex"></param>
		/// <returns></returns>
		public void SetBoneMatrix(int boneIndex, Matrix matrix)
		{
			m_nodeArray[boneIndex].LocalTransform = matrix;
		}

		/// <summary>
		/// get the number of bone
		/// </summary>
		/// <returns>bone count</returns>
		public int GetBoneCount()
		{
			return m_nodeArray.Length;
		}

		/// <summary>
		/// get a name of bone
		/// </summary>
		/// <param name="boneIndex">bone index</param>
		/// <returns>bone name</returns>
		public string GetBoneName(int boneIndex)
		{
			return m_nodeArray[boneIndex].Bone.Name;
		}

		/// <summary>
		/// find a bone by bone name
		/// </summary>
		/// <param name="name">bone name</param>
		/// <returns>index of a found bone</returns>
		public int FindBoneByName(string name)
		{
			for (int nodeIndex = 0; nodeIndex < m_nodeArray.Length; ++nodeIndex)
			{
				if (m_nodeArray[nodeIndex].Bone.Name == name)
				{
					return nodeIndex;
				}
			}

			// not found
			return -1;
		}

		[Conditional("DEBUG")]
		public void DrawDebugModel(Matrix masterTrans)
		{
			var drawSys = DrawSystem.GetInstance();
            var context = drawSys.GetDrawContext();

			foreach (var node in m_nodeArray)
			{
				if (node.DbgDrawModel == null)
				{
					// create model in first draw time
					var model = DrawModel.CreateBone(node.Bone.Name, node.Bone.Length, Color.Yellow, Vector4.Zero);
					//var model = DrawModel.CreateGizmo(node.Bone.Name, 1.0f);
					node.DbgDrawModel = model;
				}

				var mesh = node.DbgDrawModel.NodeList[0].Mesh;
				var worldTransform = GetModelTransform(node.Index) * masterTrans;
				var material = node.DbgDrawModel.NodeList[0].Material;
                context.DrawDebugModel(worldTransform, mesh, DrawSystem.RenderMode.Transparency);
			}
		}

		#region private types

		class _Node
		{
			public int Index;
			public DrawSystem.BoneData Bone;
			public DrawModel DbgDrawModel;

			/// <summary>
			/// local transform
			/// </summary>
			public Matrix LocalTransform { get; set; }

		}

		#endregion // private types

		#region private members

		private int m_masterBoneIndex;
		private _Node[] m_nodeArray;

		#endregion // private members
	}
}
