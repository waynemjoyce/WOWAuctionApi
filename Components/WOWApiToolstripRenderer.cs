using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WOWApi
{
    public class WOWApiToolStripRenderer : ToolStripProfessionalRenderer
    {
        public Color ToolStripBackColor = Color.Red;
        public WOWApiToolStripRenderer()
        {
            this.RoundedEdges = false;
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e) { }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var btn = e.Item as ToolStripButton;
            if (btn != null && btn.CheckOnClick && btn.Checked)
            {
                Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
                try
                {
                    e.Graphics.FillRectangle(new SolidBrush(ToolStripBackColor), bounds);
                }
                catch
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(81, 81, 81)), bounds);
                }
            }
            else base.OnRenderButtonBackground(e);
        }
    }
}
