using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
    public partial class ConfigForm : Form
    {
        public struct Result
        {
            public bool UseHmd;
        }

        public ConfigForm(HmdDevice hmd)
        {
            m_hasResult = false;
            m_result = new Result() { UseHmd = false };
            m_hmd = hmd;

            InitializeComponent();

            string labelFormat = "Detected device : {0}";
            if (m_hmd == null)
            {
                // disable use hmd option
                m_monoralMode.Checked = true;
                m_stereoMode.Checked = false;
                m_stereoMode.Enabled = false;
                m_detectedHmdLabel.Text = string.Format(labelFormat, "NONE");
                m_detectedHmdLabel.Enabled = false;
            }
            else
            {
                m_monoralMode.Checked = false;
                m_stereoMode.Checked = true;
                m_detectedHmdLabel.Text = string.Format(labelFormat, hmd.DisplayName);
            }
        }

        public bool HasResult()
        {
            return m_hasResult;
        }

        public Result GetResult()
        {
            Debug.Assert(HasResult(), "no result");
            return m_result;
        }

        #region private members 

        private bool m_hasResult;
        private Result m_result;
        private HmdDevice m_hmd;

        #endregion // private members

        #region private methods

        private void _OnClickStartButton(object sender, EventArgs e)
        {
            m_hasResult = true;
            m_result.UseHmd = m_stereoMode.Checked;
            Close();
        }

        #endregion // private methods
    }
}
