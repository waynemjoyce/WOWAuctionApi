using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WOWApi.Components
{
    public partial class WOWApiPanel : System.Windows.Forms.Panel
    {
        public WOWApiPanel()
        {
            InitializeComponent();
            this.pnlBack.Size = this.Size;
        }

        [
        Category("WOWApi"),
        Description("The caption.")
        ]
        public Label WOWApiCaption
        {
            get { return this.lblBackCaption; }
            set { this.lblBackCaption = value; }
        }

        [
        Category("WOWApi"),
        Description("The icon.")
        ]
        public System.Drawing.Image WOWApiIcon
        {
            get { return this.picCaption.Image; }
            set { this.picCaption.Image = value; }
        }

        [
        Category("WOWApi"),
        Description("The title backcolor.")
        ]
        public System.Drawing.Color WOWApiTitleBackColor
        {
            get { return this.pnlCaption.BackColor; }
            set 
            {
                this.pnlCaption.BackColor = value;
                this.lblBackCaption.BackColor = value;
            }
        }

        [
        Category("WOWApi"),
        Description("The title backcolor.")
        ]
        public System.Drawing.Color WOWApiBodyBackColor
        {
            get { return this.pnlBack.BackColor; }
            set
            {
                this.pnlBack.BackColor = value;
            }
        }

        //System.Drawing.Bitmap
    }
}
