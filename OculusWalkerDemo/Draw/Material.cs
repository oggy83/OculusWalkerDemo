using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Oggy
{
	public abstract class MaterialBase 
	{
		public enum MaterialTypes
		{
			Standard = 0,
			Minimap,
			Marker,
			Debug,
		}

		private MaterialTypes m_type;
		public MaterialTypes Type 
		{
			get
			{
				return m_type;
			}
		}

		public virtual void SetTextureData(DrawSystem.TextureTypes tyep, DrawSystem.TextureData tex)
		{
			// do nothing
		}

		public virtual bool GetTextureData(DrawSystem.TextureTypes type, out DrawSystem.TextureData outTexture)
		{
			outTexture = DrawSystem.TextureData.Null();
			return false;
		}

		public virtual bool GetTextureDataBySlotIndex(int slotIndex, out DrawSystem.TextureData outTexture)
		{
			outTexture = DrawSystem.TextureData.Null();
			return false;
		}

		public virtual bool IsEnableInstanceRendering()
		{
			return false;
		}

		protected MaterialBase(MaterialTypes type)
		{
			m_type = type;
		}
	}

	public class StandardMaterial : MaterialBase
	{
		/// <summary>
		/// first diffuse texture for Default
		/// </summary>
		public DrawSystem.TextureData DiffuseTex0 { get; set; }

		/// <summary>
		/// first bump texture for Default
		/// </summary>
		public DrawSystem.TextureData BumpTex0 { get; set; }

		public StandardMaterial() : base(MaterialTypes.Standard) { }

		public static MaterialBase Create(DrawSystem.TextureData diffuse)
		{
			return new StandardMaterial()
			{
				DiffuseTex0 = diffuse,
			};
		}

		public override void SetTextureData(DrawSystem.TextureTypes type, DrawSystem.TextureData tex)
		{
			switch (type)
			{
				case DrawSystem.TextureTypes.Diffuse0 :
					DiffuseTex0 = tex;
					break;

				case DrawSystem.TextureTypes.Bump0:
					BumpTex0 = tex;
					break;

				default:
					Debug.Assert(false, "unsupported texture types");
					break;
			}
		}

		public override bool GetTextureData(DrawSystem.TextureTypes type, out DrawSystem.TextureData outTexture)
		{
			switch (type)
			{
				case DrawSystem.TextureTypes.Diffuse0:
					outTexture = DiffuseTex0;
					return true;

				case DrawSystem.TextureTypes.Bump0:
					outTexture = BumpTex0;
					return true;

				default:
					Debug.Assert(false, "unsupported texture types");
					outTexture = DrawSystem.TextureData.Null();
					return false;
			}
		}

		public override bool GetTextureDataBySlotIndex(int slotIndex, out DrawSystem.TextureData outTexture)
		{
			switch (slotIndex)
			{
				case 0:
					return GetTextureData(DrawSystem.TextureTypes.Diffuse0, out outTexture);

				default:
					outTexture = DrawSystem.TextureData.Null();
					return false;
			}
		}

		public override bool IsEnableInstanceRendering()
		{
			return true;
		}
	}

	public class MarkerMaterial : MaterialBase
	{
		/// <summary>
		/// marker id for Marker
		/// </summary>
		public int MarkerId;

		public MarkerMaterial() : base(MaterialTypes.Marker) { }

		public static MarkerMaterial Create(int markerId)
		{
			return new MarkerMaterial()
			{
				MarkerId = markerId,
			};
		}
	}

	public class DebugMaterial : MaterialBase
	{
		public DebugMaterial() : base(MaterialTypes.Debug) { }

		public static DebugMaterial Create()
		{
			return new DebugMaterial();
		}

	}
}
