using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WOWApi
{
    public partial class WOWApiToolStrip : ToolStrip
    {
        public WOWApiToolStrip()
        {
            InitializeComponent();
            this.Renderer = new WOWApiToolStripRenderer();
        }

        public WOWApiToolStrip(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
