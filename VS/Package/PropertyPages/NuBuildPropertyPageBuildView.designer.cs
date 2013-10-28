namespace NuBuild.VS
{
   partial class NuBuildPropertyPageBuildView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
         this.chkIncludePdbs = new System.Windows.Forms.CheckBox();
         this.lOutputPath = new System.Windows.Forms.Label();
         this.gbOutput = new System.Windows.Forms.GroupBox();
         this.tbOutputPath = new System.Windows.Forms.TextBox();
         this.gbOutput.SuspendLayout();
         this.SuspendLayout();
         // 
         // chkIncludePdbs
         // 
         this.chkIncludePdbs.AutoSize = true;
         this.chkIncludePdbs.Location = new System.Drawing.Point(9, 46);
         this.chkIncludePdbs.Name = "chkIncludePdbs";
         this.chkIncludePdbs.Size = new System.Drawing.Size(107, 17);
         this.chkIncludePdbs.TabIndex = 3;
         this.chkIncludePdbs.Text = "Include PDB files";
         // 
         // lOutputPath
         // 
         this.lOutputPath.AutoSize = true;
         this.lOutputPath.Location = new System.Drawing.Point(6, 22);
         this.lOutputPath.Name = "lOutputPath";
         this.lOutputPath.Size = new System.Drawing.Size(66, 13);
         this.lOutputPath.TabIndex = 1;
         this.lOutputPath.Text = "Output path:";
         // 
         // gbOutput
         // 
         this.gbOutput.Controls.Add(this.tbOutputPath);
         this.gbOutput.Controls.Add(this.chkIncludePdbs);
         this.gbOutput.Controls.Add(this.lOutputPath);
         this.gbOutput.Location = new System.Drawing.Point(3, 3);
         this.gbOutput.Name = "gbOutput";
         this.gbOutput.Size = new System.Drawing.Size(345, 72);
         this.gbOutput.TabIndex = 0;
         this.gbOutput.TabStop = false;
         this.gbOutput.Text = "Output";
         // 
         // tbOutputPath
         // 
         this.tbOutputPath.Location = new System.Drawing.Point(110, 19);
         this.tbOutputPath.Name = "tbOutputPath";
         this.tbOutputPath.Size = new System.Drawing.Size(161, 20);
         this.tbOutputPath.TabIndex = 2;
         // 
         // NuBuildPropertyPageBuildView
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.gbOutput);
         this.Name = "NuBuildPropertyPageBuildView";
         this.Size = new System.Drawing.Size(355, 82);
         this.gbOutput.ResumeLayout(false);
         this.gbOutput.PerformLayout();
         this.ResumeLayout(false);

		}

		#endregion

      private System.Windows.Forms.CheckBox chkIncludePdbs;
      private System.Windows.Forms.Label lOutputPath;
      private System.Windows.Forms.GroupBox gbOutput;
      private System.Windows.Forms.TextBox tbOutputPath;
	}
}