using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	public class MaterialBase 
	{
		public enum MaterialTypes
		{
			Default = 0,
			DbgBoneWeight,
			Marker,
			Debug,
		}

		public MaterialTypes Type { get; set; }

		/// <summary>
		/// first diffuse texture for Default
		/// </summary>
		public DrawSystem.TextureData DiffuseTex0 { get; set; }

		/// <summary>
		/// first bump texture for Default
		/// </summary>
		public DrawSystem.TextureData BumpTex0 { get; set; }

		/// <summary>
		/// index of showing bone weight for DbgBoneWeight
		/// </summary>
		public int DbgBoneIndex;

		/// <summary>
		/// marker id for Marker
		/// </summary>
		public int MarkerId;

		public static MaterialBase Create(DrawSystem.TextureData diffuse)
		{
			return new MaterialBase()
			{
				Type = MaterialTypes.Default,
				DiffuseTex0 = diffuse,
				DbgBoneIndex = 0,
				MarkerId = 0
			};
		}
	}
}
