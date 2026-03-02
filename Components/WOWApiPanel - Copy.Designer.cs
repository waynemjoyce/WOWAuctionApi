namespace WOWApi.Components
{
    partial class WOWApiPanel_Backup
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WOWApiPanel));
            this.pnlCaption = new System.Windows.Forms.Panel();
            this.lblBackCaption = new System.Windows.Forms.Label();
            this.picCaption = new System.Windows.Forms.PictureBox();
            this.pnlBack = new System.Windows.Forms.Panel();
            this.pnlCaption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCaption)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlCaption
            // 
            this.pnlCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pnlCaption.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCaption.Controls.Add(this.picCaption);
            this.pnlCaption.Controls.Add(this.lblBackCaption);
            this.pnlCaption.Location = new System.Drawing.Point(0, 0);
            this.pnlCaption.Name = "pnlCaption";
            this.pnlCaption.Size = new System.Drawing.Size(220, 32);
            this.pnlCaption.TabIndex = 3;
            // 
            // lblBackCaption
            // 
            this.lblBackCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBackCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblBackCaption.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBackCaption.ForeColor = System.Drawing.Color.White;
            this.lblBackCaption.Location = new System.Drawing.Point(32, 0);
            this.lblBackCaption.Name = "lblBackCaption";
            this.lblBackCaption.Size = new System.Drawing.Size(187, 28);
            this.lblBackCaption.TabIndex = 1;
            this.lblBackCaption.Text = "Test Caption";
            this.lblBackCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picCaption
            // 
            this.picCaption.BackColor = System.Drawing.Color.Transparent;
            this.picCaption.Image = ((System.Drawing.Image)(resources.GetObject("picCaption.Image")));
            this.picCaption.Location = new System.Drawing.Point(3, 3);
            this.picCaption.Name = "picCaption";
            this.picCaption.Size = new System.Drawing.Size(24, 24);
            this.picCaption.TabIndex = 2;
            this.picCaption.TabStop = false;
            // 
            // pnlBack
            // 
            this.pnlBack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.pnlBack.Location = new System.Drawing.Point(0, 0);
            this.pnlBack.Name = "pnlBack";
            this.pnlBack.Size = new System.Drawing.Size(220, 250);
            this.pnlBack.TabIndex = 4;
            // 
            // WOWApiPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlCaption);
            this.Controls.Add(this.pnlBack);
            this.Name = "WOWApiPanel";
            this.Size = new System.Drawing.Size(220, 250);
            this.pnlCaption.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCaption)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCaption;
        private System.Windows.Forms.PictureBox picCaption;
        private System.Windows.Forms.Label lblBackCaption;
        private System.Windows.Forms.Panel pnlBack;
    }
}
