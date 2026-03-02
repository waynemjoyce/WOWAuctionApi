using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WOWApi
{
    public partial class FormWowProcess : Form
    {
        public int WowProcess = 0;

        public FormWowProcess()
        {
            InitializeComponent();
        }

        private void btnUse_Click(object sender, EventArgs e)
        {
            if (lstProcesses.SelectedItems.Count > 0)
            {
                WowProcess = int.Parse(lstProcesses.SelectedItems[0].ToString());
            }
        }

        private void FormWowProcess_Load(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("Wow"))
            {
                lstProcesses.Items.Add(p.Id.ToString());
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            Win32.ActivateWow(int.Parse(lstProcesses.SelectedItems[0].ToString()));
        }
    }
}
