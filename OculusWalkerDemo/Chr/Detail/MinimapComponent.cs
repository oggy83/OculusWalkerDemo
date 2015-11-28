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
			var chrSys = ChrSystem.GetInstance();

			var currentLocation = mapSys.GetMapLocation(chrSys.Player);
			m_progressInfos[currentLocation.BlockY, currentLocation.BlockX] = true;// check walked flag

			// set minimap material param
			Size blockSize = mapSys.GetBlockSize();
			var mapTable = new int[blockSize.Width * blockSize.Height];
			for (int h = 0; h < blockSize.Height; ++h)
			{
				for (int w = 0; w < blockSize.Width; ++w)
				{
					if (!m_progressInfos[h, w])
					{
						// not-walked block is not displayed
						mapTable[h * blockSize.Width + w] = 0;
					}
					else
					{
						var blockInfo = mapSys.GetBlockInfo(w, h);
						int data = blockInfo.CanWalkHalf()
							? 2			// walk half
							: blockInfo.CanWalkThrough()
								? 1		// can walk
								: 0;	// can not walk
						mapTable[h * blockSize.Width + w] = data;
					}
					
				}
			}

			// current position
			mapTable[currentLocation.BlockY * blockSize.Width + currentLocation.BlockX] = 3;

			m_targetMtl.SetMap(blockSize.Width, blockSize.Height, mapTable);
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "MinimapComponent depends on LayoutCompoment");

			m_modelC = Owner.FindComponent<ModelComponent>();
			Debug.Assert(m_modelC != null, "MinimapComponent depends on ModelCompoment");

			var minimapModel = m_modelC.ModelContext.DrawModel;
			m_targetMtl = minimapModel.NodeList[0].Material as MinimapMaterial;

			var mapSys = MapSystem.GetInstance();
			Size size = mapSys.GetBlockSize();
			m_progressInfos = new bool[size.Height, size.Width];
			for (int h = 0; h < size.Height; ++h)
			{
				for (int w = 0; w < size.Width; ++w)
				{
					m_progressInfos[h, w] = false;
				}
			}
			
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

		/// <summary>
		/// target minimap material
		/// </summary>
		private MinimapMaterial m_targetMtl;

		/// <summary>
		/// array of flag whether the player had walked or not
		/// </summary>
		private bool[,] m_progressInfos;

		#endregion // private members
	}
}
