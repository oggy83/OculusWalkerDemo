using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
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
	public partial class DrawContext
	{
		public struct CommonInitParam
		{
			public DrawSystem.D3DData D3D;
			public DrawResourceRepository Repository;
			public Buffer WorldVtxConst;
			public Buffer WorldPixConst;
			public RasterizerState RasterizerState;
			public BlendState[] BlendStates;
			public DepthStencilState[] DepthStencilStates;
		}

		#region private types

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _VertexShaderConst_Main
		{
			public Matrix worldMat;			// word matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _VertexShaderConst_Model
		{
			public bool isEnableSkinning;	// is enable skinning
			public float _dummy1;
			public float _dummy2;
			public float _dummy3;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _VertexShaderConst_World
		{
			public Matrix vpMat;			// view projection matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _PixelShaderConst_Main
		{
			public Color4 instanceColor;
		}

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        private struct _PixelShaderConst_Model
        {
            public Vector2 uvScale1;
            public Vector2 uvScale2;
        }

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _PixelShaderConst_ModelMinimap
		{
			public int width;
			public int height;
			private float _dummy1;
			private float _dummy2;
			public unsafe fixed int map[576];	// 24x24
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _PixelShaderConst_World
		{
			public Color4 ambientCol;	// ambient color
			public Color4 fogCol;		// fog color
			public Color4 light1Col;	// light1 color
			public Vector4 cameraPos;	// camera position in model coords
			public Vector4 light1Dir;	// light1 direction in model coords
		}

		#endregion // private types


	}
}
