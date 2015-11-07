using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Drawing;
using System.Diagnostics;

namespace Oggy
{
	public class MinimapComponent : GameEntityComponent
	{
		public MinimapComponent()
		: base(GameEntityComponent.UpdateLines.PostBehavior)
		{
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			var mapSys = MapSystem.GetInstance();

			// set minimap material param
			var minimapModel = m_modelC.ModelContext.DrawModel;
			var minimapMtl = minimapModel.NodeList[0].Material as MinimapMaterial;
			Size blockSize = mapSys.GetBlockSize();
			var mapTable = new int[blockSize.Width * blockSize.Height];
			for (int h = 0; h < blockSize.Height; ++h)
			{
				for (int w = 0; w < blockSize.Width; ++w)
				{
					var blockInfo = mapSys.GetBlockInfo(w, h);
					int data = blockInfo.CanWalkHalf()
						? 2
						: blockInfo.CanWalkThrough()
							? 1
							: 0;
					mapTable[h * blockSize.Width + w] = data;
				}
			}
			minimapMtl.SetMap(blockSize.Width, blockSize.Height, mapTable);
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "MinimapComponent depends on LayoutCompoment");

			m_modelC = Owner.FindComponent<ModelComponent>();
			Debug.Assert(m_modelC != null, "MinimapComponent depends on ModelCompoment");
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			m_layoutC = null;
			m_modelC = null;

			base.OnRemoveFromEntity(entity);
		}

		#region private members

		/// <summary>
		/// layout compoment
		/// </summary>
		private LayoutComponent m_layoutC;

		/// <summary>
		/// model component
		/// </summary>
		private ModelComponent m_modelC;

		#endregion // private members
	}
}
