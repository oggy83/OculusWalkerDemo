﻿using System;
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

		public ModelComponent()
		: base(GameEntityComponent.UpdateLines.PreDraw)
		{
            ModelContext = new Context();
			m_layoutC = null;
			//m_skeletonC = null;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			var drawSys = DrawSystem.GetInstance();
            var context = drawSys.GetDrawContext();
			//var dbg = DrawSystem.GetInstance().DebugCtrl;

			var layout = m_layoutC.Transform;

            if (ModelContext.DrawModel != null)
            {
                // Add command for draw mdoel
				if (ModelContext.DrawModel != null)
				{
					DrawSystem.MaterialData material;
					foreach (var node in ModelContext.DrawModel.NodeList)
					{
						material = node.Material;
                        var tex = material.DiffuseTex0;
                        context.DrawModel(layout, Color4.White, node.Mesh, tex, DrawSystem.RenderMode.Opaque);
                        /*
						if (node.HasBone && dbg.IsEnableDrawBoneWeight)
						{
							// setting as debug bone weight material
							material.Type = DrawSystem.MaterialTypes.DbgBoneWeight;
							material.DbgBoneIndex = dbg.BoneIndexForDraw;
						}

						Matrix[] boneMatrices = null;
						if (m_skeletonC != null)
						{
							boneMatrices = m_skeletonC.Skeleton.GetAllSkinningTransforms();
						}

						var command = node.IsDebug
							? DrawCommand.CreateDrawDebugCommand(material, layout, node.Mesh)
							: DrawCommand.CreateDrawModelCommand(material, layout, node.Mesh, boneMatrices);

						drawSys.AddDrawCommand(command);
                        */
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

                /*
				// draw bones for debug
				if (dbg.IsEnableDrawBone)
				{
					if (m_skeletonC != null)
					{
						m_skeletonC.Skeleton.DrawDebugModel(layout);
					}
				}
                */
            }
			
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_layoutC = Owner.FindComponent<LayoutComponent>();
			Debug.Assert(m_layoutC != null, "ModelCompoment depends on LayoutCompoment");

			//m_skeletonC = Owner.FindComponent<SkeletonComponent>();
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			//m_skeletonC = null;
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
		//private SkeletonComponent m_skeletonC;

		#endregion // private members
	}
}
