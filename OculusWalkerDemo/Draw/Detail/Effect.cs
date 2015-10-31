using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Diagnostics;


namespace Oggy
{
	/// <summary>
	/// vertex/pixel shader + vertex declarations
	/// </summary>
	public class Effect : ResourceBase
	{
		public InputLayout Layout { get; set; }
		public VertexShader VertexShader { get; set; }
		public PixelShader PixelShader { get; set;  }

		public Effect(String uid)
			: base(uid)
		{
			Layout = null;
			VertexShader = null;
			PixelShader = null;
		}

		public Effect(String uid, DrawSystem.D3DData d3d, InputElement[] inputElements, String vertexShader, String pixelShader)
			: this(uid)
		{
			var include = new _IncludeImpl("Shader");
			var vertexByteCode = ShaderBytecode.CompileFromFile(vertexShader, "main", "vs_4_0", ShaderFlags.Debug, EffectFlags.None, null, include);
			var pixelByteCode = ShaderBytecode.CompileFromFile(pixelShader, "main", "ps_4_0", ShaderFlags.Debug, EffectFlags.None, null, include);

            VertexShader = new VertexShader(d3d.Device, vertexByteCode);
            PixelShader = new PixelShader(d3d.Device, pixelByteCode);
			Layout = new InputLayout(d3d.Device, ShaderSignature.GetInputSignature(vertexByteCode), inputElements);

			_AddDisposable(VertexShader);
			_AddDisposable(PixelShader);
			_AddDisposable(Layout);
		}

		class _IncludeImpl : CallbackBase, Include
		{
			private string m_initialDir;

			public _IncludeImpl(string initialDir)
			{
				m_initialDir = initialDir;
			}

			public Stream Open(IncludeType type, string fileName, Stream parentStream)
			{
				return new FileStream(Path.Combine(m_initialDir, fileName), FileMode.Open, FileAccess.Read);
			}

			public void Close(Stream stream)
			{
				stream.Dispose();
			}
		}
	}
}
