using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Diagnostics;
using System.Windows.Forms;

namespace Oggy
{
	public class CameraSystem
	{
		#region static

		private static CameraSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new CameraSystem();
		}

		static public void Dispose()
		{
			// nothing
		}

		static public CameraSystem GetInstance()
		{
			return s_singleton;
		}

		public static readonly string FreeCameraName = "free";

		public static readonly string IngameCameraName = "ingame";

		#endregion // static

		#region properties

		/// <summary>
		/// Current active camera
		/// </summary>
		private ICamera m_camera = null;
		public ICamera ActiveCamera
		{
			get
			{
				return m_camera;
			}
		}

		public ICamera IngameCamera
		{
			get
			{
				return m_cameraTable[IngameCameraName];
			}
		}

		public ICamera FreeCamera
		{
			get
			{
				return m_cameraTable[FreeCameraName];
			}
		}

		#endregion // properties


		private CameraSystem()
		{
			m_cameraTable = new Dictionary<string, ICamera>();

			// register system camera
			RegisterCamera(FreeCameraName, new FreeCamera());
			RegisterCamera(IngameCameraName, new IngameCamera());
			ActivateCamera(FreeCameraName);
        }

		/// <summary>
		/// register a camera
		/// </summary>
		/// <param name="name">unique name</param>
		/// <param name="camera">camera</param>
		public void RegisterCamera(string name, ICamera camera)
		{
			if (m_cameraTable.Keys.Contains(name))
			{
				Debug.Fail(name + " is already registered");
				return;
			}

			m_cameraTable.Add(name, camera);
		}

		/// <summary>
		/// change an active camera
		/// </summary>
		/// <param name="name">name</param>
		/// <remarks>
		/// name is parameter of RegisterCamera()
		/// </remarks>
		public void ActivateCamera(string name)
		{
			ICamera camera = null;
			if (!m_cameraTable.TryGetValue(name, out camera))
			{
				Debug.Fail("not found camera : " + name);
				return;
			}

			if (camera == m_camera)
			{
				// same camera
				return;
			}

			var oldCamera = m_camera;
			if (oldCamera != null)
			{
				oldCamera.Deactivate();
			}

			m_camera = camera;
			m_camera.Activate(oldCamera);
		}

		public void Update(double dT)
		{
			if (m_camera != null)
			{
				m_camera.Update(dT);
			}
		}

		public DrawSystem.CameraData GetCameraData()
		{
			DrawSystem.CameraData result = new DrawSystem.CameraData();
			if (m_camera != null)
			{
				result = m_camera.GetCameraData();
			}

			return result;
		}

		public Matrix GetProjection()
		{
			// todo : calc fom RenderTarget
			Single fov = (Single)Math.PI / 4;
			Single aspect = 4.0f / 3;
			return Matrix.PerspectiveFovLH(fov, aspect, 0.1f, 100.0f);
		}

		[Conditional("DEBUG")]
		public void CreateDebugMenu(ToolStripMenuItem parent)
		{
            var menuItem = new ToolStripMenuItem("Camera");
            parent.DropDownItems.Add(menuItem);

			var item1 = new ToolStripRadioButtonMenuItem("free camera");
			item1.Click += (sender, e) => { ActivateCamera(FreeCameraName); };
			var item2 = new ToolStripRadioButtonMenuItem("ingame camera");
			item2.Click += (sender, e) => { ActivateCamera(IngameCameraName); };
            menuItem.DropDownItems.AddRange(new ToolStripItem[] { item1, item2 });

			item1.Checked = true;
		}

		#region private members

		
		/// <summary>
		/// registerred cameras
		/// </summary>
		private Dictionary<string, ICamera> m_cameraTable;

		#endregion // private members
	}
}
