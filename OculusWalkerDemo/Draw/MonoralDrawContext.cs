﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace Oggy
{
	public class MonoralDrawContext : IDrawContext
	{
		public MonoralDrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, DrawContext context)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_context = context;
		}

		public void Dispose()
		{
			m_context.Dispose();
		}

		public RenderTarget StartPass(DrawSystem.WorldData data)
		{
			var renderTarget = m_repository.GetDefaultRenderTarget();

            // calc projection matrix
            int width = renderTarget.Resolution.Width;
            int height = renderTarget.Resolution.Height;
            float aspect = (float)width / (float)height;
            float fov = (float)Math.PI / 3;
            var proj = Matrix.PerspectiveFovLH(fov, aspect, data.NearClip, data.FarClip);

            m_context.SetWorldParams(renderTarget, data);
            m_context.UpdateWorldParams(m_d3d.Device.ImmediateContext, data);
            m_context.UpdateEyeParams(m_d3d.Device.ImmediateContext, renderTarget, Matrix.Identity, proj);
			m_context.ClearRenderTarget(renderTarget);

			return renderTarget;
		}

		public void EndPass()
		{
			int syncInterval = 1;// 0 => immediately return, 1 => vsync
			m_d3d.SwapChain.Present(syncInterval, PresentFlags.None);
		}

        public void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode, Matrix[] boneMatrices)
		{
			m_context.DrawModel(worldTrans, color, mesh, tex, renderMode, boneMatrices);
		}

        public void DrawDebugModel(Matrix worldTrans, DrawSystem.MeshData mesh, DrawSystem.RenderMode renderMode)
        {
            m_context.DrawDebugModel(worldTrans, mesh, renderMode);
        }

        public void BeginDrawInstance(DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode)
		{
			m_context.BeginDrawInstance(mesh, tex, renderMode);
		}

		public void AddInstance(Matrix worldTrans, Color4 color)
		{
			m_context.AddInstance(worldTrans, color);
		}

		public void EndDrawInstance()
		{
			m_context.EndDrawInstance();
		}

		public CommandList FinishCommandList()
		{
			Debug.Assert(false, "MonoralDrawContext do not support FinishCommandList()");
			return null;
		}

		public void ExecuteCommandList(CommandList commandList)
		{
			m_context.ExecuteCommandList(commandList);
		}

        public Matrix GetHeadMatrix()
        {
            return Matrix.Identity;
        }

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private DrawContext m_context = null;

		#endregion // private members
	}
}
