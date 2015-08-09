using System;
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
	public interface IDrawContext
	{
		void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode, Matrix[] boneMatrices);

        void DrawDebugModel(Matrix worldTrans, DrawSystem.MeshData mesh, DrawSystem.RenderMode renderMode);

        void BeginDrawInstance(DrawSystem.MeshData mesh, DrawSystem.TextureData tex, DrawSystem.RenderMode renderMode);

		void AddInstance(Matrix worldTrans, Color4 color);

		void EndDrawInstance();

		CommandList FinishCommandList();

		void ExecuteCommandList(CommandList commandList);

        /// <summary>
        /// get a matrix which transforms view coord system to head coord system
        /// </summary>
        /// <returns>
        /// head matrix
        /// </returns>
        /// <remarks>
        /// in case of the monoral mode, this method returns identity matrix.
        /// head matrix represents the center of position defined by two eye matrix.
        /// you can use this matrix instead of camera matrix to get the true view direction.
        /// </remarks>
        Matrix GetHeadMatrix();
	}
}
