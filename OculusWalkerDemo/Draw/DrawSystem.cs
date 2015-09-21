using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using System.Runtime.InteropServices;

namespace Oggy
{
	/// <summary>
	/// Basic functions about graphics sytem
	/// </summary>
    public partial class DrawSystem 
	{
		#region static

		private static DrawSystem s_singleton = null;

		static public void Initialize(IntPtr hWnd, Device device, SwapChain swapChain, HmdDevice hmd, bool bStereoRendering, int multiThreadCount)
		{
			s_singleton = new DrawSystem(hWnd, device, swapChain, hmd, bStereoRendering, multiThreadCount);
		}

		static public void Dispose()
		{
			s_singleton.m_passCtrl.Dispose();
            if (s_singleton.m_hmd != null)
            {
                s_singleton.m_hmd.Detach();
            }
			s_singleton.m_repository.Dispose();
			s_singleton = null;
		}

		static public DrawSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region properties

		/// <summary>
		/// get/set a camera which makes Transform for World => View
		/// </summary>
		public CameraData Camera
		{
			get
			{
				return m_world.Camera;
			}
			set
			{
				m_world.Camera = value;
			}
		}

		public Color3 AmbientColor
		{
			get
			{
				return m_world.AmbientCol;
			}
			set
			{
				m_world.AmbientCol = value;
			}
		}

		public Color3 FogColor
		{
			get
			{
				return m_world.FogCol;
			}
			set
			{
				m_world.FogCol = value;
			}
		}

        public float NearClip
        {
            get
            {
                return m_world.NearClip;
            }
            set
            {
                m_world.NearClip = value;
            }
        }

        public float FarClip
        {
            get
            {
                return m_world.FarClip;
            }
            set
            {
                m_world.FarClip = value;
            }
        }

		private D3DData m_d3d;
		public D3DData D3D
		{
			get
			{
				return m_d3d;
			}
		}

		private DrawResourceRepository m_repository = null;
		public DrawResourceRepository ResourceRepository
		{
			get
			{
				return m_repository;
			}
		}

        private DrawDebugCtrl m_debug = null;
        public DrawDebugCtrl DebugCtrl
        {
            get
            {
                return m_debug;
            }
        }

		#endregion // properties

		private DrawSystem(IntPtr hWnd, Device device, SwapChain swapChain, HmdDevice hmd, bool bStereoRendering, int multiThreadCount)
        {
			m_d3d = new D3DData
			{
				Device = device,
				SwapChain = swapChain,
				WindowHandle = hWnd,
			};

            m_debug = new DrawDebugCtrl();
			
			AmbientColor = new Color3(0, 0, 0);
			m_world.DirLight.Direction = new Vector3(0, 1, 0);
			m_world.DirLight.Color = new Color3(1, 1, 1);

			m_repository = new DrawResourceRepository(m_d3d);
			m_passCtrl = new DrawPassCtrl(m_d3d, m_repository, hmd, bStereoRendering, multiThreadCount);

			m_bStereoRendering = bStereoRendering;
			m_hmd = hmd;
            if (m_hmd != null)
            {
                m_hmd.Setup(m_d3d, m_repository.GetDefaultRenderTarget());
            }

			m_drawBuffer = new DrawBuffer();
			
		}

		public void SetDirectionalLight(DirectionalLightData light)
		{
			m_world.DirLight = light;
		}

		public void BeginScene()
		{
			DrawSystem.WorldData data = m_world;
			m_passCtrl.StartPass(data);
		}

        public IDrawContext GetDrawContext()
        {
            return m_passCtrl.Context;
        }

		public void EndScene()
		{
			m_passCtrl.EndPass();
		}

		public IDrawContext GetSubThreadContext(int index)
		{
			return m_passCtrl.GetSubThreadContext(index);
		}

		public DrawBuffer GetDrawBuffer()
		{
			return m_drawBuffer;
		}

        [Conditional("DEBUG")]
        public void CreateDebugMenu(ToolStripMenuItem parent)
        {
            var menuItem = new ToolStripMenuItem("Draw");
            parent.DropDownItems.Add(menuItem);

            m_debug.CreateDebugMenu(menuItem);
        }

		#region private members

		/// <summary>
		/// current world data
		/// </summary>
		private WorldData m_world;

		private HmdDevice m_hmd = null;

		private bool m_bStereoRendering;

		private DrawPassCtrl m_passCtrl;

		private DrawBuffer m_drawBuffer;

		#endregion // private members

	
	}
}
