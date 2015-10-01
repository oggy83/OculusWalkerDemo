using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace Oggy
{
	public class ModelComponent : GameEntityComponent
    {
        #region inner classes

        public class Context
        {
            /// <summary>
            /// get/set enable to write shadow
            /// </summary>
            public bool EnableCastShadow { get; set; }

            /* TODO
    		/// <summary>
	    	/// get/set enable to use shadow 
	    	/// </summary>
	    	public bool EnableReceiveShadow { get; set; }
	    	*/

            /// <summary>
            /// get/set a draw model resource
            /// </summary>
            public DrawModel DrawModel { get; set; }

			/// <summary>
			/// get/set a debug model resource
			/// </summary>
			public DrawModel DebugModel { get; set; }

            public Context()
            {
                EnableCastShadow = false;
                DrawModel = null;
				DebugModel = null;
            }
        }

        #endregion // inner classed

        #region properties

        public ModelComponent.Context ModelContext { get; set; }
				  
		#endregion // properties

		public ModelComponent(GameEntityComponent.UpdateLines updateLine)
		: base(updateLine)
		{
            ModelContext = new Context();
			m_layoutC = null;
			m_skeletonC = null;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			if (ModelContext.DrawModel == null)
            {
				return;
			}

			var drawSys = DrawSystem.GetInstance();
			var cullingSys = CullingSystem.GetInstance();
            var context = drawSys.GetDrawContext();
			var dbg = DrawSystem.GetInstance().DebugCtrl;
			var layout = m_layoutC.Transform;

			CullingSystem.FrustumCullingResult result = cullingSys.CheckFrustumCulling(ModelContext.DrawModel.BoundingBox, layout);
			if (!result.IsVisible)
			{
				// out of view-volume
			}
			else
			{
				// Add command for draw mdoel
				DrawSystem.MaterialData material;
				foreach (var node in ModelContext.DrawModel.NodeList)
				{
					if (UpdateLine == GameEntityComponent.UpdateLines.PreDraw)
					{
						// use draw buffer
						drawSys.GetDrawBuffer().AppendStaticModel(layout, result.Z, ref node.Mesh, ref node.Material);
					}
					else if (UpdateLine == GameEntityComponent.UpdateLines.Draw)
					{
						Matrix[] boneMatrices = null;
						if (m_skeletonC != null)
						{
							// has skeleton
							boneMatrices = m_skeletonC.Skeleton.GetAllSkinningTransforms();
						}

						material = node.Material;
						var tex = material.DiffuseTex0;
						context.DrawModel(layout, Color4.White, node.Mesh, tex, DrawSystem.RenderMode.Opaque, boneMatrices);
					}
				}
			}

            /*
			// Add command for debug mdoel
			if (dbg.IsEnableDrawTangentFrame && ModelContext.DebugModel != null)
			{
				foreach (var node in ModelContext.DebugModel.NodeList)
				{
					var command = DrawCommand.CreateDrawDebugCommand(node.Material, layout, node.Mesh);
					drawSys.AddDrawCommand(command);
				}
			}
            */

            /*
            // Add command for draw shadow
            if (ModelContext.EnableCastShadow)
            {
                foreach (var node in ModelContext.DrawModel.NodeList)
                {
					var command = DrawCommand.CreateDrawShadowCommand(layout, node.Mesh);
					drawSys.AddDrawCommand(command);
                }

            }
            */

			// draw aabb
			if (dbg.IsEnableAabb)
			{
				if (m_dbgBoundingBoxModel == null)
				{
					// create model in first draw time
					var model = DrawModel.CreateBox("aabb", ModelContext.DrawModel.BoundingBox, Color.Pink);
					m_dbgBoundingBoxModel = model;
				}

				var mesh = m_dbgBoundingBoxModel.NodeList[0].Mesh;
				DrawSystem.MaterialData material = m_dbgBoundingBoxModel.NodeList[0].Material;
				context.DrawDebugModel(layout, mesh, DrawSystem.RenderMode.Transparency);
			}

			// draw bones for debug
			if (dbg.IsEnableDrawBone)
			{
				if (m_skeletonC != null)
				{
					m_skeletonC.Skeleton.DrawDebugModel(layout);
				}
			}
			
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "ModelCompoment depends on LayoutCompoment");

			m_skeletonC = Owner.FindComponent<SkeletonComponent>();
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			m_skeletonC = null;
			m_layoutC = null;

			base.OnRemoveFromEntity(entity);
		}

		#region private members

		/// <summary>
		/// layout compoment
		/// </summary>
		private LayoutComponent m_layoutC;

		/// <summary>
		/// skeleton compoment (optional)
		/// </summary>
		private SkeletonComponent m_skeletonC;

		/// <summary>
		/// bounding box of this model
		/// </summary>
		private DrawModel m_dbgBoundingBoxModel = null;

		#endregion // private members
	}
}
