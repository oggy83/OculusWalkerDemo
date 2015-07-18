using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Blender;


namespace Oggy
{
	public partial class BlenderScene : IDisposable
	{
		#region properties

		private List<SceneNode> m_nodeList;
		public ReadOnlyCollection<SceneNode> NodeList
		{
			get
			{
				return m_nodeList.AsReadOnly();
			}
		}

		#endregion // properties

		public BlenderScene()
		{
			m_nodeList = new List<SceneNode>();
		}

		public void Dispose()
		{
			m_nodeList.Clear();
		}

		public static BlenderScene FromFile(string path)
		{
			var repository = new BlendTypeRepository();
			var loader = new BlendLoader(repository);
			var entityList = loader.FromFile(path);
			if (entityList == null)
			{
				return null;
			}

			var scene = new BlenderScene();
			if (!scene._LoadScene(repository, entityList))
			{
				scene.Dispose();
				return null;
			}

			return scene;
		}

		#region private methods

		private bool _LoadScene(BlendTypeRepository repository, List<BlockHeaderEntity> entityList)
		{
			// find root
			BlendValueCapsule root = entityList.Where(e => e.Name == "GLOB").Select(e => e.Children[0].Value).First();
			if (root == null)
			{
				return false;
			}

			var scene = root.GetMember("curscene").GetRawValue<BlendAddress>().DereferenceOne();
			var listBase = scene.GetMember("base");
			var nextBase = listBase.GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();

			// load mesh
			while (nextBase != null)
			{
				var obj = nextBase.GetMember("object").GetRawValue<BlendAddress>().DereferenceOne();
				if (obj != null)
				{
					var data = obj.GetMember("data").GetRawValue<BlendAddress>().DereferenceOne();
					if (data.Type.Name == "Mesh")
					{
						// mesh object
						string name = obj.GetMember("id").GetMember("name").GetAllValueAsString();
						int restrictFlag = obj.GetMember("restrictflag").GetRawValue<char>();
						if ((restrictFlag & 1) == 0)
						{
							// enable view object
							Console.WriteLine("found mesh : " + name);
							if (!_LoadMesh(repository, obj))
							{
								return false;
							}
						}

					}

				}

				nextBase = nextBase.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
			}

			return true;
		}

		private bool _LoadMesh(BlendTypeRepository repository, BlendValueCapsule meshObj)
		{
			var mesh = meshObj.GetMember("data").GetRawValue<BlendAddress>().DereferenceOne();
			string meshName = meshObj.GetMember("id").GetMember("name").GetAllValueAsString();
			int mtlCount = meshObj.GetMember("totcol").GetRawValue<int>();

			// Build Armature
			DrawSystem.BoneData[] boneArray = null;
			int[] deformGroupIndex2BoneIndex = null;
			AnimType.AnimationData animData;
			if (!_LoadArmature(repository, meshObj, out boneArray, out deformGroupIndex2BoneIndex, out animData))
			{
				// igrnore
			}
			
			// Build original vertices
			var originVertices = _CreateOriginalVertices(repository, mesh, deformGroupIndex2BoneIndex);

			// Build materials
			var pmtlType = new BlendPointerType(repository.Find("Material"));
			var mtls = mesh.GetMember("mat").GetRawValue<BlendAddress>().DereferenceAll(pmtlType);
			Debug.Assert(mtls.Count == mtlCount, "material count is unmatched");

			for (int mtlIndex = 0; mtlIndex < mtlCount; ++mtlIndex)
			{
				// Build a vertex buffer(s)
				var vertices = originVertices
					.Where(v => v.MaterialIndex == mtlIndex)
					.ToArray();

				
				var bMaterial = mtls[mtlIndex].GetRawValue<BlendAddress>().DereferenceOne();
				if (bMaterial != null)
				{
					Dictionary<DrawSystem.TextureTypes, string> textureNames = null;
					DrawSystem.MaterialData material;
					if (!_LoadMaterial(repository, bMaterial, out textureNames, out material))
					{
						continue;
					}

					var node = new SceneNode()
					{
						Name = meshName,
						Vertics = vertices,
						MaterialData = material,
						TextureNames = textureNames,
						BoneArray = mtlIndex == 0 ? boneArray : null,// set boneArray for the first node
						Animation = animData.Actions != null ? (AnimType.AnimationData?)animData : null,
					};
					m_nodeList.Add(node);
				}

			}

			return m_nodeList.Count != 0;
		}

		private VertexOriginal[] _CreateOriginalVertices(BlendTypeRepository repository, BlendValueCapsule mesh, int[] deformGroupIndex2BoneIndex)
		{
			var mpolyList = mesh.GetMember("mpoly").GetRawValue<BlendAddress>().DereferenceAll();
			var mloopList = mesh.GetMember("mloop").GetRawValue<BlendAddress>().DereferenceAll();
			var mloopuvList = mesh.GetMember("mloopuv").GetRawValue<BlendAddress>().DereferenceAll();
			var mvertList = mesh.GetMember("mvert").GetRawValue<BlendAddress>().DereferenceAll();
			var dvertList = mesh.GetMember("dvert").GetRawValue<BlendAddress>().DereferenceAll();

			int capacity = mpolyList.Count() * 6;// assume that all polygons is square.
			var vertices = new List<VertexOriginal>(capacity);
			foreach (var mpoly in mpolyList)
			{
				int offset = mpoly.GetMember("loopstart").GetRawValue<int>();
				int count = mpoly.GetMember("totloop").GetRawValue<int>();
				short materialIndex = mpoly.GetMember("mat_nr").GetRawValue<short>();
				Debug.Assert(count >= 0, "negative totloop is here!");// todo: ref previous loop

				int[] plan = null;
				switch (count)
				{
					case 3:
						plan = new int[] { offset + 2, offset + 1, offset + 0};
						break;
					case 4 :
						// triangulation
						plan = new int[] { offset + 2, offset + 1, offset, offset + 3, offset + 2, offset };
						break;
					default:
						Debug.Fail("tutloop must be 3 or 4");// todo: ref previous loop
						break;
				}

                if (plan == null)
                {
                    continue;
                }

				foreach (int i in plan)
				{
					int vIndex = mloopList[i].GetMember("v").GetRawValue<int>();
					var position = mvertList[vIndex].GetMember("co");
					var normal = mvertList[vIndex].GetMember("no");

					VertexOriginal vertex;
					vertex.Position.X = position.GetAt(0).GetRawValue<float>();
					vertex.Position.Y = position.GetAt(1).GetRawValue<float>();
					vertex.Position.Z = position.GetAt(2).GetRawValue<float>();
					vertex.Position.W = 1;
					vertex.Position = BlenderUtil.ChangeCoordsSystem(vertex.Position);

					vertex.Normal.X = normal.GetAt(0).GetRawValue<short>();
					vertex.Normal.Y = normal.GetAt(1).GetRawValue<short>();
					vertex.Normal.Z = normal.GetAt(2).GetRawValue<short>();
					vertex.Normal = BlenderUtil.ChangeCoordsSystem(vertex.Normal);
					vertex.Normal.Normalize();

					var uv = mloopuvList[i].GetMember("uv");
					vertex.Texcoord.X = uv.GetAt(0).GetRawValue<float>();
					vertex.Texcoord.Y = 1 - uv.GetAt(1).GetRawValue<float>();

					vertex.Tangent = Vector3.Zero;
					vertex.Binormal = Vector3.Zero;

					vertex.MaterialIndex = materialIndex;

					var weights = dvertList == null
						? null
						: dvertList[vIndex].GetMember("dw").GetRawValue<BlendAddress>().DereferenceAll();
					if (weights == null || deformGroupIndex2BoneIndex == null)
					{
						vertex.BoneWeights = null;
						vertex.BoneIndices = null;
					}
					else
                    {
                        // load bone weights
                        // bone weight can be stored 0, so we ignore this case.
                        //int maxWeightCount = dvertList[vIndex].GetMember("totweight").GetRawValue<int>();
                        var noneZeroWeightList =
                            weights.Select(w => new Tuple<float, int>(w.GetMember("weight").GetRawValue<float>(), w.GetMember("def_nr").GetRawValue<int>()))
                            .Where(tuple => tuple.Item1 > 0.1f);// ignore near zero value too
                        int weightCount = noneZeroWeightList.Count();

                        vertex.BoneWeights = new float[weightCount];
                        vertex.BoneIndices = new uint[weightCount];
                        {
                            int wIndex = 0;
                            foreach (var tuple in noneZeroWeightList)
                            {
                                float weight = tuple.Item1;
                                int deformGroupIndex = tuple.Item2;
                                vertex.BoneWeights[wIndex] = weight;

                                // def_nr is NOT index of bones, but index of deform group
                                // we must replace to index of bones using bone-deform mapping.
                                vertex.BoneIndices[wIndex] = (uint)deformGroupIndex2BoneIndex[deformGroupIndex];

                                wIndex++;
                            }
                        }

                        // normalize weight
                        float sumWeight = vertex.BoneWeights.Sum();
                        for (int wIndex = 0; wIndex < weightCount; ++wIndex)
                        {
                            vertex.BoneWeights[wIndex] /= sumWeight;
                        }

                    }

					vertices.Add(vertex);
				}
			}

			// compute tangent and binormal
			int polyCount = vertices.Count / 3;
			for (int polyIndex = 0; polyIndex < polyCount; ++polyIndex)
			{
				var posArray = new Vector4[] 
				{
					vertices[polyIndex * 3].Position,
					vertices[polyIndex * 3 + 1].Position,
					vertices[polyIndex * 3 + 2].Position,
				};

				var normalArray = new Vector3[] 
				{
					vertices[polyIndex * 3].Normal,
					vertices[polyIndex * 3 + 1].Normal,
					vertices[polyIndex * 3 + 2].Normal,
				};

				var uvArray = new Vector2[] 
				{
					vertices[polyIndex * 3].Texcoord,
					vertices[polyIndex * 3 + 1].Texcoord,
					vertices[polyIndex * 3 + 2].Texcoord,
				};

				var faceTangent = MathUtil.ComputeFaceTangent(posArray[0], posArray[1], posArray[2], uvArray[0], uvArray[1], uvArray[2]);

				for (int vIndex = 0; vIndex < 3; ++vIndex)
				{
					// calc tangent
					var normal = normalArray[vIndex];
					var tangent = faceTangent;
					MathUtil.Orthonormalize(ref normal, ref tangent);

					// calc binormal
					var binormal = Vector3.Cross(normalArray[vIndex], tangent);
					//binormal.Normalize();

					var oldVertex = vertices[polyIndex * 3 + vIndex];
					oldVertex.Tangent = tangent;
					oldVertex.Binormal = binormal;
					vertices[polyIndex * 3 + vIndex] = oldVertex;
				}

			}

			return vertices.ToArray();
		}

		

		#endregion // private methods
	}
}
