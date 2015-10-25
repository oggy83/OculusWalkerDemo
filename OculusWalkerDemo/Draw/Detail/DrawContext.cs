using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Oggy
{
	/// <summary>
	/// This class process DrawCommand and render.
	/// Change of render state is minimized by comparing new and last command,
	/// This means that draw command sorting is important to get high performance.
	/// </summary>
	public partial class DrawContext : IDisposable, IDrawContext
	{
        public const int MaxBoneMatrices = 32;

		public DrawContext(DeviceContext context, CommonInitParam initParam)
		{
			m_initParam = initParam;
			m_context = context;

			var d3d = m_initParam.D3D;
			m_mainVtxConst = DrawUtil.CreateConstantBuffer<_MainVertexShaderConst>(d3d, m_maxInstanceCount);
            m_boneVtxConst = DrawUtil.CreateConstantBuffer(d3d, Utilities.SizeOf<Matrix>() * MaxBoneMatrices);
			m_modelVtxConst = DrawUtil.CreateConstantBuffer<_ModelVertexShaderConst>(d3d, 1);
			m_mainPixConst = DrawUtil.CreateConstantBuffer<_MainPixelShaderConst>(d3d, m_maxInstanceCount);
            m_modelPixConst = DrawUtil.CreateConstantBuffer<_ModelPixelShaderConst>(d3d, 1);

			m_instanceMainVtxConst = new _MainVertexShaderConst[m_maxInstanceCount];
			m_instanceMainPixConst = new _MainPixelShaderConst[m_maxInstanceCount];
		}

		virtual public void Dispose()
		{
			if (m_context.TypeInfo == DeviceContextType.Deferred)
			{
				m_context.Dispose();
			}

            m_modelPixConst.Dispose();
			m_mainPixConst.Dispose();
			m_modelVtxConst.Dispose();
            m_boneVtxConst.Dispose();
			m_mainVtxConst.Dispose();
		}

        virtual public void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, MaterialBase material, DrawSystem.RenderMode renderMode, Matrix[] boneMatrices)
        {
            _SetModelParams(mesh, material, renderMode);

            // update vertex shader resouce
            var vdata = new _MainVertexShaderConst()
            {
                // hlsl is column-major memory layout, so we must transpose matrix
                worldMat = Matrix.Transpose(worldTrans),
            };
            m_context.UpdateSubresource(ref vdata, m_mainVtxConst);

            // update pixel shader resouce
            var pdata = new _MainPixelShaderConst()
            {
                instanceColor = color
            };
            m_context.UpdateSubresource(ref pdata, m_mainPixConst);

            // update a bone constant buffer
            if (boneMatrices != null)
            {
                // UpdateSubresouce supports only to upate entire of resource.
                // so, we must copy boneMatrices to m_tmpBoneMatrices.
                // It seems inefficient. we search for better solutions.

                Debug.Assert(boneMatrices.Length <= m_tmpBoneMatrices.Length);
                for (int i = 0; i < boneMatrices.Length; ++i)
                {
                    m_tmpBoneMatrices[i] = boneMatrices[i];
                    m_tmpBoneMatrices[i].Transpose();
                }

                m_context.UpdateSubresource<Matrix>(m_tmpBoneMatrices, m_boneVtxConst);
            }
            else
            {
                // low performance @todo
                for (int i = 0; i < m_tmpBoneMatrices.Length; ++i)
                {
                    m_tmpBoneMatrices[i] = Matrix.Identity;
                }

                m_context.UpdateSubresource<Matrix>(m_tmpBoneMatrices, m_boneVtxConst);
            }

            // draw
            m_context.Draw(mesh.VertexCount, 0);
            m_drawCallCount++;
        }

        virtual public void DrawDebugModel(Matrix worldTrans, DrawSystem.MeshData mesh, DrawSystem.RenderMode renderMode)
		{
			_SetModelParams(mesh, null, renderMode);

			// update vertex shader resouce
			{
				var vdata = new _MainVertexShaderConst()
				{
					// hlsl is column-major memory layout, so we must transpose matrix
					worldMat = Matrix.Transpose(worldTrans),
				};
				m_context.UpdateSubresource(ref vdata, m_mainVtxConst);
			}

			// update pixel shader resouce
			{
				var pdata = new _MainPixelShaderConst()
				{
					instanceColor = Color4.White,
				};
				m_context.UpdateSubresource(ref pdata, m_mainPixConst);
			}

			// do not use bone matrices
			// m_context.UpdateSubresource<Matrix>(m_tmpBoneMatrices, m_boneVtxConst);

			// draw
			m_context.Draw(mesh.VertexCount, 0);
			m_drawCallCount++;
		}

        virtual public void BeginDrawInstance(DrawSystem.MeshData mesh, MaterialBase material, DrawSystem.RenderMode renderMode)
		{
			_SetModelParams(mesh, material, renderMode);
		}

		virtual public void AddInstance(Matrix worldTrans, Color4 color)
		{
			// hlsl is column-major memory layout, so we must transpose matrix
			m_instanceMainVtxConst[m_nextInstanceIndex] = new _MainVertexShaderConst() { worldMat = Matrix.Transpose(worldTrans) };
			m_instanceMainPixConst[m_nextInstanceIndex] = new _MainPixelShaderConst() { instanceColor = color };

			m_nextInstanceIndex++;
			if (m_nextInstanceIndex == m_maxInstanceCount)
			{
				// update vertex shader resouce
				m_context.UpdateSubresource<_MainVertexShaderConst>(m_instanceMainVtxConst, m_mainVtxConst);

				// update pixel shader resouce
				m_context.UpdateSubresource<_MainPixelShaderConst>(m_instanceMainPixConst, m_mainPixConst);

				// low performance @todo
				for (int i = 0; i < m_tmpBoneMatrices.Length; ++i)
				{
					m_tmpBoneMatrices[i] = Matrix.Identity;
				}
				m_context.UpdateSubresource<Matrix>(m_tmpBoneMatrices, m_boneVtxConst);

				// draw
				m_context.DrawInstanced(m_lastVertexCount, m_nextInstanceIndex, 0, 0);
				m_drawCallCount++;

				m_nextInstanceIndex = 0;
			}
		}

		virtual public void EndDrawInstance()
		{
			if (m_nextInstanceIndex != 0)
			{
				// update vertex shader resouce
				m_context.UpdateSubresource<_MainVertexShaderConst>(m_instanceMainVtxConst, m_mainVtxConst);

				// update pixel shader resouce
				m_context.UpdateSubresource<_MainPixelShaderConst>(m_instanceMainPixConst, m_mainPixConst);

				// low performance @todo
				for (int i = 0; i < m_tmpBoneMatrices.Length; ++i)
				{
					m_tmpBoneMatrices[i] = Matrix.Identity;
				}
				m_context.UpdateSubresource<Matrix>(m_tmpBoneMatrices, m_boneVtxConst);

				// draw
				m_context.DrawInstanced(m_lastVertexCount, m_nextInstanceIndex, 0, 0);
				m_drawCallCount++;

				m_nextInstanceIndex = 0;
			}
		}

		virtual public CommandList FinishCommandList()
		{
			return m_context.FinishCommandList(true);
		}

		virtual public void ExecuteCommandList(CommandList commandList)
		{
			m_context.ExecuteCommandList(commandList, true);
			commandList.Dispose();
		}

        public Matrix GetHeadMatrix()
        {
            return Matrix.Identity;
        }

		public DrawSystem.EyeData GetEyeData()
		{
			Debug.Assert(false, "DrawContext.GetEyeData() is not implemented");
			return new DrawSystem.EyeData();
		}

		public void SetWorldParams(RenderTarget renderTarget, DrawSystem.WorldData data)
		{
			m_worldData = data;

			// set fixed param
			{
				m_context.Rasterizer.State = m_initParam.RasterizerState;

				int width = renderTarget.Resolution.Width;
				int height = renderTarget.Resolution.Height;
				m_context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
				m_context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);

			}

			// bind shader 
			m_context.VertexShader.SetConstantBuffer(0, m_mainVtxConst);
            m_context.VertexShader.SetConstantBuffer(1, m_boneVtxConst);
			m_context.VertexShader.SetConstantBuffer(2, m_modelVtxConst);
			m_context.VertexShader.SetConstantBuffer(3, m_initParam.WorldVtxConst);
			m_context.PixelShader.SetConstantBuffer(0, m_mainPixConst);
            m_context.PixelShader.SetConstantBuffer(1, m_modelPixConst);
			m_context.PixelShader.SetConstantBuffer(2, m_initParam.WorldPixConst);

			m_drawCallCount = 0;
			m_nextInstanceIndex = 0;
		}

		public void UpdateWorldParams(DeviceContext context, DrawSystem.WorldData worldData)
		{
			// nothing
		}

		public void UpdateEyeParams(DeviceContext context, RenderTarget renderTarget, DrawSystem.EyeData eyeData)
		{
			var vdata = new _WorldVertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				vpMat = Matrix.Transpose(eyeData.ViewMatrix * eyeData.ProjectionMatrix),
			};
			context.UpdateSubresource(ref vdata, m_initParam.WorldVtxConst);

			// init pixel shader resource
			var pdata = new _WorldPixelShaderConst()
			{
				ambientCol = new Color4(m_worldData.AmbientColor),
				fogCol = new Color4(m_worldData.FogColor),
				light1Col = new Color4(m_worldData.DirectionalLight.Color),
				cameraPos = new Vector4(eyeData.EyePosition, 1.0f),
				light1Dir = new Vector4(m_worldData.DirectionalLight.Direction, 0.0f),
			};
			m_context.UpdateSubresource(ref pdata, m_initParam.WorldPixConst);
		}

		public void ClearRenderTarget(RenderTarget renderTarget)
		{
			m_context.ClearDepthStencilView(renderTarget.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
			m_context.ClearRenderTargetView(renderTarget.TargetView, new Color4(m_worldData.FogColor));
		}

		#region private members

		private CommonInitParam m_initParam;
		private DeviceContext m_context = null;
		private DrawSystem.WorldData m_worldData;
		private int m_drawCallCount = 0;

		// draw param 
		private Buffer m_mainVtxConst = null;
        private Buffer m_boneVtxConst = null;
		private Buffer m_modelVtxConst = null;
		private Buffer m_mainPixConst = null;
        private Buffer m_modelPixConst = null;
        private Matrix[] m_tmpBoneMatrices = new Matrix[MaxBoneMatrices];

		// previous draw setting
		private DrawSystem.RenderMode? m_lastRenderMode = null;
        private TextureView m_lastTextureRes = null;
        private Vector2? m_lastTextureUvScale = null;
		private bool? m_lastEnableSkinning = null;
		private PrimitiveTopology? m_lastTopology = null;
		private VertexBufferBinding[] m_lastVertexBuffers = null;
		private int m_lastVertexCount = 0;
        private MaterialBase.MaterialTypes? m_materialType = null;

		// DrawInstancedModel context
		private int m_nextInstanceIndex = 0;
		private int m_maxInstanceCount = 32;
		private _MainVertexShaderConst[] m_instanceMainVtxConst;
		private _MainPixelShaderConst[] m_instanceMainPixConst;
		

		#endregion // private members

		#region private methods

        private void _SetModelParams(DrawSystem.MeshData mesh, MaterialBase material, DrawSystem.RenderMode renderMode)
		{
			// update texture resouce
			DrawSystem.TextureData tex;
			if (material == null)
			{
				tex = DrawSystem.TextureData.Null();
			}
			else
			{
				material.GetTextureData(DrawSystem.TextureTypes.Diffuse0, out tex);
			}

			if (m_lastTextureRes == null || m_lastTextureRes.IsDisposed() || m_lastTextureRes != tex.Resource)
			{
                // update resource
                int slotIndex = 0;
                if (tex.Resource != null)
                {
                    m_context.PixelShader.SetShaderResource(slotIndex, tex.Resource.View);
                    m_context.PixelShader.SetSampler(slotIndex, tex.Resource.SamplerState);
                }
                else
                {
                    // set null texture
                    m_context.PixelShader.SetShaderResource(slotIndex, null);
                    m_context.PixelShader.SetSampler(slotIndex, null);
                }

				m_lastTextureRes = tex.Resource;
			}

            // update texture uv scale
            if (m_lastTextureUvScale == null || !m_lastTextureUvScale.Value.Equals(tex.UvScale))
            {
                var modelPixConst = new _ModelPixelShaderConst()
                {
                    uvScale1 = tex.UvScale,
                    uvScale2 = new Vector2(1, 1),   // @todo
                };
                m_context.UpdateSubresource(ref modelPixConst, m_modelPixConst);
				
                m_lastTextureUvScale = tex.UvScale;
			}

			// update model vertex shader param
			bool isEnableSkinning = mesh.Buffers.Count() == 3;
			if (m_lastEnableSkinning == null || !m_lastEnableSkinning.Value.Equals(isEnableSkinning))
			{
				var modelVtxConst = new _ModelVertexShaderConst()
				{
					isEnableSkinning  = isEnableSkinning,
				};
				m_context.UpdateSubresource(ref modelVtxConst, m_modelVtxConst);

				m_lastEnableSkinning = isEnableSkinning;
			}


			// update material
			MaterialBase.MaterialTypes materialType = material == null ? MaterialBase.MaterialTypes.Debug : material.Type;
            if (m_materialType == null || m_materialType != materialType)
            {
				switch (materialType)
				{
					case MaterialBase.MaterialTypes.Standard:
						{
							Effect effect = null;
							effect = m_initParam.Repository.FindResource<Effect>("Std");
							m_context.InputAssembler.InputLayout = effect.Layout;
							m_context.VertexShader.Set(effect.VertexShader);
							m_context.PixelShader.Set(effect.PixelShader);
						}
						break;

					case MaterialBase.MaterialTypes.Debug:
						{
							Effect effect = null;
							effect = m_initParam.Repository.FindResource<Effect>("Debug");
							m_context.InputAssembler.InputLayout = effect.Layout;
							m_context.VertexShader.Set(effect.VertexShader);
							m_context.PixelShader.Set(effect.PixelShader);
						}
						break;

					case MaterialBase.MaterialTypes.Marker:
						Debug.Assert(false, "unsupported material");
						break;
				}

				m_materialType = materialType;
            }

			// update render mode
			if (m_lastRenderMode == null || m_lastRenderMode != renderMode)
			{
				m_context.OutputMerger.BlendState = m_initParam.BlendStates[(int)renderMode];
				m_context.OutputMerger.DepthStencilState = m_initParam.DepthStencilStates[(int)renderMode];

				m_lastRenderMode = renderMode;
			}

			// update input assembler
			if (m_lastTopology == null || m_lastTopology != mesh.Topology)
			{
				m_context.InputAssembler.PrimitiveTopology = mesh.Topology;
				m_lastTopology = mesh.Topology;
			}
			if (m_lastVertexBuffers == null || m_lastVertexBuffers != mesh.Buffers)
			{
				m_context.InputAssembler.SetVertexBuffers(0, mesh.Buffers);
				m_lastVertexBuffers = mesh.Buffers;
				m_lastVertexCount = mesh.VertexCount;
			}
		}

		#endregion // private methods

	}
}
