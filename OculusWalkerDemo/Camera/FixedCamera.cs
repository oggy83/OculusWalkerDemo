using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace Oggy
{
    public class FixedCamera : ICamera
    {
        public FixedCamera()
        {
            Vector3 eye, lookAt, up;
            eye = new Vector3(0, 2.5f, -10);
            lookAt = new Vector3(0, 2.5f, 0);
            up = Vector3.UnitY;
            m_camera = new DrawSystem.CameraData(eye, lookAt, up);
        }

        public void Update(double dt)
        {
            // nothing
        }

        public DrawSystem.CameraData GetCameraData()
        {
            return m_camera;
        }

        public void Activate(ICamera oldCamera)
        {
            // nothing
        }

        public void Deactivate()
        {
            // nothing
        }

        #region private members

        /// <summary>
        /// camera data
        /// </summary>
        private DrawSystem.CameraData m_camera;

        #endregion // private members
    }
}
