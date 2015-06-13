using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Oggy
{
	public class DrawDebugCtrl
	{
		#region properties 

		public bool IsEnableDrawTangentFrame
		{
			get;
			set;
		}

		public bool IsEnableDrawBoneWeight
		{
			get;
			set;
		}

		public bool IsEnableDrawBone
		{
			get;
			set;
		}

		public int BoneIndexForDraw
		{
			get;
			set;
		}

		#endregion // properties

		public DrawDebugCtrl()
		{
			IsEnableDrawTangentFrame = false;
			IsEnableDrawBoneWeight = false;
			IsEnableDrawBone = false;
			BoneIndexForDraw = 0;
		}

		
		[Conditional("DEBUG")]
		public void CreateDebugMenu(ToolStripMenuItem parent)
		{
			//_AddCheckButton(parent, "draw tangent frame", "IsEnableDrawTangentFrame");
			//_AddCheckButton(parent, "draw bone weight", "IsEnableDrawBoneWeight");
			_AddCheckButton(parent, "draw bone", "IsEnableDrawBone");

            /*
			// show bone index
			{
				var label = new Label();
				label.Text = "bone index";
				var item = new TextBox();
				item.CausesValidation = true;
				item.LostFocus += (sender, e) =>
				{
					int result;
					if (int.TryParse(item.Text, out result))
					{
						BoneIndexForDraw = result;
					}
					else
					{
						item.Clear();
					}
				};

				var layout = new FlowLayoutPanel();
				layout.Controls.Add(label);
				layout.Controls.Add(item);

				parent.DropDownItems.Add(new ToolStripControlHost(layout));
			}
            */
		}

		#region private methods

		private void _AddCheckButton(ToolStripMenuItem parent, string name, string propName)
		{
			var prop = GetType().GetProperty(propName);

			var item = new ToolStripMenuItem(name);
			item.Checked = (bool)prop.GetValue(this);
			item.CheckOnClick = true;
			item.Click += (sender, e) =>
			{
				prop.SetValue(this, !(bool)prop.GetValue(this));
			};
			parent.DropDownItems.Add(item);
		}

		#endregion // private methods
	}
}
