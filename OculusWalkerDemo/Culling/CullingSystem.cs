using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Diagnostics;

namespace Oggy
{
	public class CullingSystem
	{
		#region static

		private static CullingSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new CullingSystem();
		}

		static public void Dispose()
		{
			if (s_singleton == null)
			{
				return;
			}

			if (s_singleton.m_dbgModelEntity != null)
			{
				s_singleton.m_dbgModelEntity.Dispose();
			}

			s_singleton = null;
		}

		static public CullingSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region public types

		public struct FrustumCullingResult
		{
			public bool IsVisible;

			/// <summary>
			/// if IsVisible is true, Z is set 
			/// </summary>
			public float Z;		
		}

		#endregion // public types

		public void UpdateFrustum()
		{
			var drawSys = DrawSystem.GetInstance();
			var cameraSys = CameraSystem.GetInstance();

			// frustum culling use always the ingame camera. this means that you can look down culling in editor mode 
			var camera = cameraSys.IngameCamera;
			m_frustumMatrix = camera.GetCameraData().GetViewMatrix() * drawSys.GetDrawContext().GetHeadMatrix() * drawSys.ComputeProjectionTransform();
		}

		public FrustumCullingResult CheckFrustumCulling(Aabb aabb, Matrix worldTransform)
		{
			var wvpTrans = worldTransform * m_frustumMatrix;

			FrustumCullingResult result;
			result.IsVisible = aabb.IsInFrustum(wvpTrans);
			result.Z = result.IsVisible 
				? wvpTrans.TranslationVector.Z
				: 0.0f;
			return result;
		}

		#region private methods

		private CullingSystem()
		{
			_RegisterDebugEntity();
		}

		[Conditional("DEBUG")]
		private void _RegisterDebugEntity()
		{
			var entitySys = EntitySystem.GetInstance();

			var entity = new GameEntity("frustum");

			var layoutC = new LayoutComponent();
			layoutC.Transform = Matrix.Identity;
			entity.AddComponent(layoutC);

			var drawC = new EasyDrawComponent((IDrawContext context, Matrix layout, DrawModel lastDrawModel) =>
			{
				if (lastDrawModel != null)
				{
					lastDrawModel.Dispose();
				}

				var cameraSys = CameraSystem.GetInstance();
				if (cameraSys.IngameCamera != cameraSys.ActiveCamera)
				{
					// frustum is visible in editor mode
					var drawModel = DrawModel.CreateFrustum("frustum", _GetFrustumMatrix(), Color.Cyan);
					foreach (var node in drawModel.NodeList)
					{
						context.DrawDebugModel(layout, node.Mesh, DrawSystem.RenderMode.Transparency);
					}

					return drawModel;
				}
				else
				{
					return null;
				}
			});
			entity.AddComponent(drawC);

			m_dbgModelEntity = entity;
		}

		private Matrix _GetFrustumMatrix()
		{
			return m_frustumMatrix;
		}
				
		#endregion // private methods

		#region private members

		/// <summary>
		/// the product of view matrix and projection transform for frustum culling
		/// </summary>
		private Matrix m_frustumMatrix = Matrix.Identity;

		private GameEntity m_dbgModelEntity;

		#endregion // private members

	}
}
