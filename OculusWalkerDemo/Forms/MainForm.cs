using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oggy
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public Panel GetRenderTarget()
        {
            return m_renderPanel;
        }

        #region private members

#if DEBUG
        private DebugDialog m_debugDialog = null;
#endif // DEBUG
        #endregion // private members

        #region private methods

        private void _OnLoad(object sender, EventArgs e)
        {
#if DEBUG
            m_debugDialog = new DebugDialog();
            m_debugDialog.Show();
            var debugMenu = m_debugDialog.GetSystemMenuItem();
            DrawSystem.GetInstance().CreateDebugMenu(debugMenu);
            GameSystem.GetInstance().CreateDebugMenu(debugMenu);
            CameraSystem.GetInstance().CreateDebugMenu(debugMenu);
#endif // DEBUG
        }

        private void _OnFormClosed(object sender, FormClosedEventArgs e)
        {
#if DEBUG
            m_debugDialog.Hide();
#endif // DEBUG
        }

        private void _OnKeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            if (e.KeyCode == Keys.F11)
            {
                HmdSystem.GetInstance().GetDevice().SwitchPerfDisplay();
            }
#endif
        }

        #endregion // private methods
    }
}
