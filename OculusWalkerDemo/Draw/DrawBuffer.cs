using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace Oggy
{
	/// <summary>
	/// draw buffer for efficient rendering (e.g. instance draw)
	/// </summary>
	public class DrawBuffer : IDisposable
	{
		public DrawBuffer()
		{
		}

		virtual public void Dispose()
		{
		}

		public void AppendStaticModel(Matrix layout, float z, ref DrawSystem.MeshData mesh, ref DrawSystem.MaterialData material)
		{
			var key = new _StaticModelData() { Mesh = mesh, Material = material };
			List<_InstanceData> instanceList = null;
			if (!m_staticModelBuffer.TryGetValue(key, out instanceList))
			{
				int capacity = 128;
				instanceList = new List<_InstanceData>(capacity);
				m_staticModelBuffer.Add(key, instanceList);
			}

			instanceList.Add(new _InstanceData() { Layout = layout, Z = z });
		}

		public void Process(IDrawContext context)
		{
			// draw static model
			foreach (var pair in m_staticModelBuffer)
			{
				pair.Value.Sort((a, b) => { return a.Z < b.Z ? -1 : 1; });// z sort (near object has priority to render)

				var key = pair.Key;
				context.BeginDrawInstance(key.Mesh, key.Material.DiffuseTex0, DrawSystem.RenderMode.Opaque);
				foreach (var instance in pair.Value)
				{
					context.AddInstance(instance.Layout, Color4.White);
				}
				context.EndDrawInstance();
			}

			m_staticModelBuffer.Clear();
		}

		#region private types

		private struct _StaticModelData
		{
			public DrawSystem.MeshData Mesh;
			public DrawSystem.MaterialData Material;
		}

		private class _InstanceData
		{
			public Matrix Layout;
			public float Z;
		}

		#endregion // private types

		#region private members

		private Dictionary<_StaticModelData, List<_InstanceData>> m_staticModelBuffer = new Dictionary<_StaticModelData, List<_InstanceData>>();

		#endregion // private members
	}
}
