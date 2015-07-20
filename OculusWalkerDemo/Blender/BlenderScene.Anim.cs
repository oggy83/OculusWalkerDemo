using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using Blender;

namespace Oggy
{
	public partial class BlenderScene
	{
		private bool _LoadArmature(BlendTypeRepository repository, BlendValueCapsule meshObj, out DrawSystem.BoneData[] outBoneArray, out int[] outDeformGroupIndex2BoneIndex, out AnimType.AnimationData outAnimData)
		{
			BlendValueCapsule bArmature = null;
			BlendValueCapsule bAnimData = null;

			// find blend value
			{
				var mod = meshObj.GetMember("modifiers").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
				while (mod != null)
				{
					if (mod.Type.Equals(repository.Find("ArmatureModifierData")))
					{
						// animation modifier
						var armatureObj = mod.GetMember("object").GetRawValue<BlendAddress>().DereferenceOne();
						if (armatureObj != null)
						{
							bArmature = armatureObj.GetMember("data").GetRawValue<BlendAddress>().DereferenceOne();
							bAnimData = armatureObj.GetMember("adt").GetRawValue<BlendAddress>().DereferenceOne();
                            break;
						}
					}

                    mod = mod.GetMember("modifier").GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
				}
			}

			// build boneList from armature
			var boneList = new List<DrawSystem.BoneData>();
			if (bArmature != null)
			{
				var firstBone = bArmature.GetMember("bonebase").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
				var tmpBoneList = new List<DrawSystem.BoneData>();
				_LoadBone(repository, firstBone, -1, ref tmpBoneList);

				// compute local bone matrix
				for (int boneIndex = 0; boneIndex < tmpBoneList.Count(); ++boneIndex)
				{
					var tmp = tmpBoneList[boneIndex];
					if (!tmp.IsMasterBone())
					{
						tmp.BoneTransform = tmp.BoneTransform * Matrix.Invert(tmpBoneList[tmp.Parent].BoneTransform);
					}
					boneList.Add(tmp);
				}
			}

			// build anime data from animAction
			var animActionList = new List<AnimType.ActionData>();
			if (bAnimData != null)
			{
				var bAnimAction = bAnimData.GetMember("action").GetRawValue<BlendAddress>().DereferenceOne();
				while (bAnimAction != null)
				{
					_LoadAction(repository, bAnimAction, ref animActionList);
					bAnimAction = bAnimAction.GetMember("id").GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
				}
			}

			var deformGroupNameList = new List<string>();
			var deformGroup = meshObj.GetMember("defbase").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
			while (deformGroup != null)
			{
				var groupName = deformGroup.GetMember("name").GetAllValueAsString();
				//Console.WriteLine("    found deform group : " + groupName);
				deformGroupNameList.Add(groupName);
				deformGroup = deformGroup.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
			}

			if (deformGroupNameList.Count != 0 && boneList.Count != 0)
			{
				// sort boneArray by deform group
				outDeformGroupIndex2BoneIndex = new int[boneList.Count()];
				int nextIndex = 0;
				foreach (var defName in deformGroupNameList)
				{
					var bone = boneList.Select((n, index) => new { n, index }).FirstOrDefault(ni => ni.n.Name == defName);
                    if (bone != null)
                    {
                        outDeformGroupIndex2BoneIndex[nextIndex] = bone.index;
                    }
                    nextIndex++;
				}
				outBoneArray = boneList.ToArray();
				if (animActionList.Count == 0)
				{
					outAnimData = new AnimType.AnimationData();
				}
				else
				{
					outAnimData = new AnimType.AnimationData();
					outAnimData.Actions = animActionList.ToArray();
				}
				return true;
			}
			else
			{
				outDeformGroupIndex2BoneIndex = new int[0];
				outBoneArray = new DrawSystem.BoneData[0];
				outAnimData = new AnimType.AnimationData();
				return false;
			}
		}

		private void _LoadAction(BlendTypeRepository repository, BlendValueCapsule bAnimAction, ref List<AnimType.ActionData> animActionList)
		{
			var groupList = new List<AnimType.ActionGroupData>();
			var bGroup = bAnimAction.GetMember("groups").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
			while (bGroup != null)
			{
				var groupData = new AnimType.ActionGroupData();
				groupData.BoneName = bGroup.GetMember("name").GetAllValueAsString();
				groupData.Location = AnimType.ChannelData<Vector3>.Empty();
				groupData.Rotation = AnimType.ChannelData<Quaternion>.Empty();
				groupData.Scale = AnimType.ChannelData<Vector3>.Empty();

				//Console.WriteLine("    found anim action group : " + groupData.BoneName);

				var channelList = new List<AnimType.ChannelData<float>>();
				var bChannel = bGroup.GetMember("channels").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
				while (bChannel != null)
				{
					string boneName = "";
					string propertyName = "";
					var bRnaPath = bChannel.GetMember("rna_path").GetRawValue<BlendAddress>().DereferenceAll(Blender.BlendPrimitiveType.Char());
					string rnaPath = Blender.ConvertUtil.CharArray2String(bRnaPath.Select(c => (object)c.GetRawValue<char>()));
					if (!BlenderUtil.ParseRnaPath(rnaPath, ref boneName, ref propertyName))
					{
						Debug.Fail("Failed to parse rna path(" + rnaPath + ")");
						return;
					}
					int arrayIndex = bChannel.GetMember("array_index").GetRawValue<int>();

					if (boneName == groupData.BoneName)
					{
						//Console.WriteLine(String.Format("        {0}.{1}[{2}]", boneName, propertyName, arrayIndex));

						var bBeztList = bChannel.GetMember("bezt").GetRawValue<BlendAddress>().DereferenceAll();
						var channel = new AnimType.ChannelData<float>();
						channel.KeyFrames = new AnimType.KeyData<float>[bBeztList.Count()];

						foreach (var bBezt in bBeztList.Select((value, index) => new { value, index }))
						{
							float frame = bBezt.value.GetMember("vec").GetAt(1, 0).GetRawValue<float>();
							float value = bBezt.value.GetMember("vec").GetAt(1, 1).GetRawValue<float>();

							channel.KeyFrames[bBezt.index] = new AnimType.KeyData<float>((int)frame, value);
						}

						channelList.Add(channel);
					}

					bChannel = bChannel.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
				}	// while

				if (channelList.Count() == 10)
				{
					// channel type convertion 
					// location : floatx3 to Vector3
					// rotation : floatx4 to Quatanion
					// scale : floatx3 to Vector3
					groupData.Location.KeyFrames
						= channelList[0].KeyFrames
						.Select((key, index) => new AnimType.KeyData<Vector3>(key.Frame, new Vector3(key.Value, channelList[1].KeyFrames[index].Value, channelList[2].KeyFrames[index].Value)))
						.Select(key => { key.Value = BlenderUtil.ChangeCoordsSystem(key.Value); return key; })
						//.Select(key => { key.Frame--; return key; })	// blender frame index starts from 1
						.ToArray();
					groupData.Rotation.KeyFrames
						= channelList[3].KeyFrames
						.Select((key, index) => new AnimType.KeyData<Quaternion>(key.Frame, new Quaternion(channelList[4].KeyFrames[index].Value, channelList[5].KeyFrames[index].Value, channelList[6].KeyFrames[index].Value, key.Value)))
						.Select(key => { key.Value = BlenderUtil.ChangeCoordsSystem(key.Value); return key; })
						//.Select(key => { key.Frame--; return key; })	// blender frame index starts from 1
						.ToArray();
					groupData.Scale.KeyFrames
						= channelList[7].KeyFrames
						.Select((key, index) => new AnimType.KeyData<Vector3>(key.Frame, new Vector3(key.Value, channelList[8].KeyFrames[index].Value, channelList[9].KeyFrames[index].Value)))
						.Select(key => { key.Value = BlenderUtil.ChangeCoordsSystem(key.Value); return key; })
						//.Select(key => { key.Frame--; return key; })	// blender frame index starts from 1
						.ToArray();
					groupList.Add(groupData);

					bGroup = bGroup.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
				}
				else
				{
					Debug.Fail("unexpected the number of channels.");
					return;
				}
			}

			if (groupList.Count != 0)
			{
				var actionData = new AnimType.ActionData();
				var actionName = bAnimAction.GetMember("id").GetMember("name").GetAllValueAsString();
				actionName = actionName.Substring(2, actionName.Length - 2);//  ACArmatureAction => ArmatureAction

				actionData.Name = actionName;
				actionData.Groups = groupList.ToArray();
				animActionList.Add(actionData);
			}
		}

		private void _LoadBone(BlendTypeRepository repository, BlendValueCapsule bone, int parentBoneIndex, ref List<DrawSystem.BoneData> outList)
		{

			if (bone != null)
			{
				// make bone data
				var name = bone.GetMember("name").GetAllValueAsString();
				float length = bone.GetMember("length").GetRawValue<float>();
				var offset = new Vector3()
				{
					X = bone.GetMember("head").GetAt(0).GetRawValue<float>(),
					Y = bone.GetMember("head").GetAt(1).GetRawValue<float>(),
					Z = bone.GetMember("head").GetAt(2).GetRawValue<float>(),
				};
				offset = BlenderUtil.ChangeCoordsSystem(offset);

				var elements = new float[16];
				for (int i = 0; i < 4; ++i)
				{
					for (int j = 0; j < 4; ++j)
					{
						elements[i * 4 + j] = bone.GetMember("arm_mat").GetAt(i, j).GetRawValue<float>();
					}
				}
				var modelTrans = new Matrix(elements);
				modelTrans = BlenderUtil.ChangeCoordsSystem(modelTrans);

				var result = new DrawSystem.BoneData()
				{
					Name = name,
					Parent = parentBoneIndex,
					BoneTransform = modelTrans,// convert local bone transformation after
					BoneOffset = Matrix.Invert(modelTrans),
					Length = length,
				};

				outList.Add(result);
				parentBoneIndex = outList.Count() - 1;
				//Console.WriteLine("    found bone : " + name);

				// call for children
				var childBone = bone.GetMember("childbase").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
				while (childBone != null)
				{
					_LoadBone(repository, childBone, parentBoneIndex, ref outList);
					childBone = childBone.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
				}
			}

		}


	}	// class
}
