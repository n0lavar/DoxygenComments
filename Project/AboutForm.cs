using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenComments
{
    public class AboutForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.LinkLabel sourceCodeLabel;
        private System.Windows.Forms.LinkLabel donateLabel;

        public AboutForm()
        {
            InitializeComponent();
        }

        private void donateLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://paypal.me/nickkhrapov");
        }
        private void sourceCode_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/n0lavar/DoxygenComments");
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.donateLabel = new System.Windows.Forms.LinkLabel();
            this.infoLabel = new System.Windows.Forms.Label();
            this.sourceCodeLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // donateLabel
            // 
            this.donateLabel.AutoSize = true;
            this.donateLabel.Location = new System.Drawing.Point(93, 125);
            this.donateLabel.Name = "donateLabel";
            this.donateLabel.Size = new System.Drawing.Size(42, 13);
            this.donateLabel.TabIndex = 0;
            this.donateLabel.TabStop = true;
            this.donateLabel.Text = "Donate";
            this.donateLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donateLabel_LinkClicked);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(289, 91);
            this.infoLabel.TabIndex = 1;
            this.infoLabel.Text = resources.GetString("infoLabel.Text");
            // 
            // sourceCodeLabel
            // 
            this.sourceCodeLabel.AutoSize = true;
            this.sourceCodeLabel.Location = new System.Drawing.Point(12, 125);
            this.sourceCodeLabel.Name = "sourceCodeLabel";
            this.sourceCodeLabel.Size = new System.Drawing.Size(68, 13);
            this.sourceCodeLabel.TabIndex = 2;
            this.sourceCodeLabel.TabStop = true;
            this.sourceCodeLabel.Text = "Source code";
            this.sourceCodeLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.sourceCode_LinkClicked);
            // 
            // AboutForm
            // 
            this.ClientSize = new System.Drawing.Size(314, 162);
            this.Controls.Add(this.sourceCodeLabel);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.donateLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AboutForm";
            this.Text = "About DoxygenComments";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
