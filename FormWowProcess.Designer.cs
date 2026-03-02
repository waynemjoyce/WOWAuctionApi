namespace WOWApi
{
    partial class FormWowProcess
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWowProcess));
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.btnActivate = new System.Windows.Forms.Button();
            this.btnUse = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstProcesses
            // 
            this.lstProcesses.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.ItemHeight = 14;
            this.lstProcesses.Location = new System.Drawing.Point(12, 13);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(120, 102);
            this.lstProcesses.TabIndex = 0;
            // 
            // btnActivate
            // 
            this.btnActivate.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnActivate.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnActivate.ForeColor = System.Drawing.Color.Black;
            this.btnActivate.Image = ((System.Drawing.Image)(resources.GetObject("btnActivate.Image")));
            this.btnActivate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnActivate.Location = new System.Drawing.Point(138, 13);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new System.Drawing.Size(90, 26);
            this.btnActivate.TabIndex = 173;
            this.btnActivate.Text = " Activate";
            this.btnActivate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnActivate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // btnUse
            // 
            this.btnUse.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnUse.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnUse.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUse.ForeColor = System.Drawing.Color.Black;
            this.btnUse.Image = ((System.Drawing.Image)(resources.GetObject("btnUse.Image")));
            this.btnUse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUse.Location = new System.Drawing.Point(138, 45);
            this.btnUse.Name = "btnUse";
            this.btnUse.Size = new System.Drawing.Size(90, 26);
            this.btnUse.TabIndex = 174;
            this.btnUse.Text = " Use";
            this.btnUse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUse.UseVisualStyleBackColor = true;
            this.btnUse.Click += new System.EventHandler(this.btnUse_Click);
            // 
            // button2
            // 
            this.button2.Cursor = System.Windows.Forms.Cursors.Default;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(138, 89);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 26);
            this.button2.TabIndex = 175;
            this.button2.Text = " Cancel";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // FormWowProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(21)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(239, 130);
            this.ControlBox = false;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnUse);
            this.Controls.Add(this.btnActivate);
            this.Controls.Add(this.lstProcesses);
            this.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWowProcess";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select WOW Process For Team";
            this.Load += new System.EventHandler(this.FormWowProcess_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstProcesses;
        private System.Windows.Forms.Button btnActivate;
        private System.Windows.Forms.Button btnUse;
        private System.Windows.Forms.Button button2;
    }
}