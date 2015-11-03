using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Oggy
{
	public class MinimapMaterial : MaterialBase
	{
		/// <summary>
		/// diffuse texture
		/// </summary>
		public DrawSystem.TextureData DiffuseTex0 { get; set; }

		public MinimapMaterial() : base(MaterialTypes.Minimap) { }

		public static MaterialBase Create(DrawSystem.TextureData diffuse)
		{
			return new MinimapMaterial()
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

				default:
					Debug.Assert(false, "unsupported texture types");
					outTexture = DrawSystem.TextureData.Null();
					return false;
			}
		}

		public void SetMap(int width, int height, int[] mapTable)
		{
			m_mapTable = (int[])mapTable.Clone();
			m_mapWidth = width;
			m_mapHeight = height;
		}

		public int[] GetMapTable()
		{
			return m_mapTable;
		}

		public int GetMapWidth()
		{
			return m_mapWidth;
		}

		public int GetMapHeight()
		{
			return m_mapHeight;
		}

		#region private members

		private int[] m_mapTable = new int[1];
		private int m_mapWidth = 0;
		private int m_mapHeight = 0;

		#endregion // private members
	}
}
