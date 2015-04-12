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
    public partial class DebugDialog : Form
    {
        public DebugDialog()
        {
            InitializeComponent();
        }

        public ToolStripMenuItem GetSystemMenuItem()
        {
            return m_systemToolStripMenuItem;
        }
    }
}
