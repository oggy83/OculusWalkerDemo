﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
	public class StereoDrawContext : IDrawContext
	{
		public StereoDrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, DrawContext context)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_context = context;
			m_hmd = hmd;

            // Create render targets for each HMD eye
            m_textureSets = new HmdSwapTextureSet[2];
            var eyeResolution = hmd.EyeResolution;
            var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
            for (int index = 0; index < 2; ++index)
            {
                var textureSet = m_hmd.CreateSwapTextureSet(d3d.Device, eyeResolution.Width, eyeResolution.Height);
                m_textureSets[index] = textureSet;

                var renderTargetList = RenderTarget.FromSwapTextureSet(m_d3d, resNames[index], textureSet);
                foreach (var renderTarget in renderTargetList)
                {
                    m_repository.AddResource(renderTarget);
                }
            }

            // Create temporaly render target
            var tmpRt = RenderTarget.CreateRenderTarget(d3d, "Temp", eyeResolution.Width, eyeResolution.Height);
            m_repository.AddResource(tmpRt);

            // Create texture for Mirroring
            Size defaultRenderTargetSize = m_repository.GetDefaultRenderTarget().Resolution;
            m_mirrorTexture = m_hmd.CreateMirrorTexture(m_d3d.Device, defaultRenderTargetSize.Width, defaultRenderTargetSize.Height);

			m_commandListTable = new List<CommandList>();
		}

		public void Dispose() 
		{
			foreach (var commandList in m_commandListTable)
			{
				commandList.Dispose();
			}

            m_mirrorTexture.Dispose();

            foreach (var textureSet in m_textureSets)
            {
                textureSet.Dispose();
            }

			m_context.Dispose();
		}

		public RenderTarget StartPass(DrawSystem.WorldData data)
		{
            m_worldData = data;
            var renderTarget = m_repository.FindResource<RenderTarget>("Temp");
			var eyeOffset = m_hmd.GetEyePoses();
            var proj = _CalcProjection(1, m_worldData.NearClip, m_worldData.FarClip);

			m_context.SetWorldParams(renderTarget, data);
			m_hmd.BeginScene();

			m_context.UpdateWorldParams(m_d3d.Device.ImmediateContext, data);
            m_context.UpdateEyeParams(m_d3d.Device.ImmediateContext, renderTarget, eyeOffset[1], proj);// set right eye settings
			m_context.ClearRenderTarget(renderTarget);
			m_isContextDirty = true;

			return renderTarget;
		}

		public void EndPass()
		{
            int texIndex = m_textureSets[0].CurrentIndex;
            var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye" + texIndex), m_repository.FindResource<RenderTarget>("OVRRightEye" + texIndex) };
            var tmpRenderTarget = m_repository.FindResource<RenderTarget>("Temp");
            var eyeOffset = m_hmd.GetEyePoses();
            var proj = _CalcProjection(0, m_worldData.NearClip, m_worldData.FarClip);

            if (m_isContextDirty)
            {
                var prevCommandList = m_context.FinishCommandList();
                m_commandListTable.Add(prevCommandList);
                m_isContextDirty = false;
            }

            // render right eye image to temp buffer
            foreach (var commandList in m_commandListTable)
            {
                m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);
            }

            // copy temp buffer to right eye buffer
            m_d3d.Device.ImmediateContext.CopyResource(tmpRenderTarget.TargetTexture, renderTargets[1].TargetTexture);

            // set left eye settings
            m_context.UpdateEyeParams(m_d3d.Device.ImmediateContext, renderTargets[0], eyeOffset[0], proj);

            // render left eye image to temp buffer
            foreach (var commandList in m_commandListTable)
            {
                m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);
            }

            // copy temp buffer to left eye buffer
            m_d3d.Device.ImmediateContext.CopyResource(tmpRenderTarget.TargetTexture, renderTargets[0].TargetTexture);

            m_hmd.EndScene(m_textureSets[0], m_textureSets[1]);

            // delete command list
            foreach (var commandList in m_commandListTable)
            {
                commandList.Dispose();
            }
            m_commandListTable.Clear();

            // mirroring
            var defaultRenderTarget = m_repository.GetDefaultRenderTarget();
            m_d3d.Device.ImmediateContext.CopyResource(m_mirrorTexture.GetResource(), defaultRenderTarget.TargetTexture);

            int syncInterval = 0;// 0 => immediately return, 1 => vsync
            m_d3d.SwapChain.Present(syncInterval, PresentFlags.None);
		}

        public void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode, Matrix[] boneMatrices)
		{
			m_isContextDirty = true;
			m_context.DrawModel(worldTrans, color, mesh, tex, renderMode, boneMatrices);
		}

        public void DrawDebugModel(Matrix worldTrans, DrawSystem.MeshData mesh, DrawSystem.RenderMode renderMode)
        {
            m_isContextDirty = true;
            m_context.DrawDebugModel(worldTrans, mesh, renderMode);
        }

        public void BeginDrawInstance(DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode)
		{
			m_isContextDirty = true;
			m_context.BeginDrawInstance(mesh, tex, renderMode);
		}

		public void AddInstance(Matrix worldTrans, Color4 color)
		{
			m_isContextDirty = true;
			m_context.AddInstance(worldTrans, color);
		}

		public void EndDrawInstance()
		{
			m_isContextDirty = true;
			m_context.EndDrawInstance();
		}

		public CommandList FinishCommandList()
		{
			Debug.Assert(false, "MonoralDrawContext do not support FinishCommandList()");
			return null;
		}

		public void ExecuteCommandList(CommandList commandList)
		{
			if (m_isContextDirty)
			{
				var prevCommandList = m_context.FinishCommandList();
				m_commandListTable.Add(prevCommandList);
				m_isContextDirty = false;
			}

			m_commandListTable.Add(commandList);
		}

        public Matrix GetHeadMatrix()
        {
          return m_hmd.GetHeadMatrix();
        }

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private DrawContext m_context = null;
		private HmdDevice m_hmd = null;
		private List<CommandList> m_commandListTable = null;
		private bool m_isContextDirty = false;
        private DrawSystem.WorldData m_worldData;
        private HmdSwapTextureSet[] m_textureSets = null;
        private HmdMirrorTexture m_mirrorTexture = null;

		#endregion // private members

        #region private methods

        private Matrix _CalcProjection(int eyeIndex, float nearClip, float farClip)
        {
            var fov = m_hmd.GetEyeFovs()[eyeIndex];
            uint flag = (uint)LibOVR.ovrProjectionModifier.None;

            LibOVR.ovrMatrix4f ovrProj = LibOVR.ovrMatrix4f_Projection(fov, nearClip, farClip, flag);
            Matrix tmp = new Matrix();
            tmp.M11 = ovrProj.M11;
            tmp.M12 = ovrProj.M21;
            tmp.M13 = ovrProj.M31;
            tmp.M14 = ovrProj.M41;
            tmp.M21 = ovrProj.M12;
            tmp.M22 = ovrProj.M22;
            tmp.M23 = ovrProj.M32;
            tmp.M24 = ovrProj.M42;
            tmp.M31 = ovrProj.M13;
            tmp.M32 = ovrProj.M23;
            tmp.M33 = ovrProj.M33;
            tmp.M34 = ovrProj.M43;
            tmp.M41 = ovrProj.M14;
            tmp.M42 = ovrProj.M24;
            tmp.M43 = ovrProj.M34;
            tmp.M44 = ovrProj.M44;
            return tmp;
        }

        #endregion // private methods
	}
}
