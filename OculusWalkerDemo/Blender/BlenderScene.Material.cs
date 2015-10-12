using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Blender;
using System.Diagnostics;

namespace Oggy
{
	public partial class BlenderScene
	{

		private bool _LoadMaterial(BlendTypeRepository repository, BlendValueCapsule bMaterial, out Dictionary<DrawSystem.TextureTypes, TextureInfo> outTextureInfos, out DrawSystem.MaterialData outMaterial)
		{
			string mtlName = bMaterial.GetMember("id").GetMember("name").GetAllValueAsString();
			Console.WriteLine("    found material : " + mtlName);

			var texInfos = new Dictionary<DrawSystem.TextureTypes, TextureInfo>();
			string materialTypeName = Path.GetExtension(mtlName);
			switch (materialTypeName)
			{
				case "":
				case ".std":
case ".map":
					{
						var mtexs = bMaterial.GetMember("mtex");
						var mtexsType = mtexs.Type as BlendArrayType;
						for (int mtexIndex = 0; mtexIndex < mtexsType.GetLength(0); ++mtexIndex)
						{
							// Build textures
							var mtex = mtexs.GetAt(mtexIndex).GetRawValue<BlendAddress>().DereferenceOne();
							if (mtex != null)
							{
                                float scaleU = mtex.GetMember("size").GetAt(0).GetRawValue<float>();
                                float scaleV = mtex.GetMember("size").GetAt(1).GetRawValue<float>();
								var tex = mtex.GetMember("tex").GetRawValue<BlendAddress>().DereferenceOne();
								if (tex != null)
								{
									var ima = tex.GetMember("ima").GetRawValue<BlendAddress>().DereferenceOne();
									if (ima != null)
									{
										var typename = tex.GetMember("id").GetMember("name").GetAllValueAsString();
										typename = Path.GetFileNameWithoutExtension(typename);//  TEdiffuse.001 => TEdiffuse
										var path = Path.GetFileName(ima.GetMember("name").GetAllValueAsString());

										Console.WriteLine("    found texture : " + path);

										DrawSystem.TextureTypes type = DrawSystem.TextureTypes.Diffuse0;
										switch (typename)
										{
											case "TEdiffuse":
												type = DrawSystem.TextureTypes.Diffuse0;
												break;

											case "TEnormal":
												type = DrawSystem.TextureTypes.Bump0;
												break;

											default:
												Debug.Fail("unsupported texture typename " + typename);
												break;
										}

                                        texInfos.Add(type, new TextureInfo { Name = path, UvScale = new Vector2(scaleU, scaleV) });
									}
								}
							}
						}

						outMaterial = new DrawSystem.MaterialData();
						outTextureInfos = texInfos;
					}
					return true;

				case ".mark":
					{
						var prop = _FindCustomProperty(bMaterial.GetMember("id"), "id");
						if (prop == null)
						{
							Debug.Fail("marker material must have id property");
							break;
						}

						outMaterial = new DrawSystem.MaterialData()
						{
							Type = DrawSystem.MaterialTypes.Marker,
							MarkerId = prop.Value,
						};
						outTextureInfos = texInfos;
					}
					return true;

				default:
					Debug.Fail("unknown material type : " + materialTypeName);
					break;
			}

			outMaterial = new DrawSystem.MaterialData();
			outTextureInfos = texInfos;
			return false;
		}

		/// <summary>
		/// find a custom property which contains a given name
		/// </summary>
		/// <param name="bId">material id</param>
		/// <param name="propertyName">search key</param>
		/// <returns></returns>
		CustomProperty _FindCustomProperty(BlendValueCapsule bId, string propertyName)
		{
			var bTopIdProperty = bId.GetMember("properties").GetRawValue<BlendAddress>().DereferenceOne();
			var bNextIdProperty = bTopIdProperty.GetMember("data").GetMember("group").GetMember("first").GetRawValue<BlendAddress>().DereferenceOne();
			while (bNextIdProperty != null)
			{
				// found
				var name = bNextIdProperty.GetMember("name").GetAllValueAsString();
				if (propertyName == name)
				{
					var prop = new CustomProperty()
					{
						Name = name,
						Value = bNextIdProperty.GetMember("data").GetMember("val").GetRawValue<int>()
					};
					return prop;
				}

				bNextIdProperty = bNextIdProperty.GetMember("next").GetRawValue<BlendAddress>().DereferenceOne();
			}

			// not fund
			return null;
		}
	}
}
