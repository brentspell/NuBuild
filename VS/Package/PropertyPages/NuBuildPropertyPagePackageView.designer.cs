namespace NuBuild.VS
{
   partial class NuBuildPropertyPagePackageView
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
         this.chkVersionFileName = new System.Windows.Forms.CheckBox();
         this.cbVersionSource = new System.Windows.Forms.ComboBox();
         this.lVersionSource = new System.Windows.Forms.Label();
         this.cbTargetFramework = new System.Windows.Forms.ComboBox();
         this.lTargetFramework = new System.Windows.Forms.Label();
         this.gbFramework = new System.Windows.Forms.GroupBox();
         this.chkAddBinariesToSubfolder = new System.Windows.Forms.CheckBox();
         this.gbVersioning = new System.Windows.Forms.GroupBox();
         this.gbTransformation = new System.Windows.Forms.GroupBox();
         this.chkTransformOnBuild = new System.Windows.Forms.CheckBox();
         this.gbFramework.SuspendLayout();
         this.gbVersioning.SuspendLayout();
         this.gbTransformation.SuspendLayout();
         this.SuspendLayout();
         // 
         // chkVersionFileName
         // 
         this.chkVersionFileName.AutoSize = true;
         this.chkVersionFileName.Location = new System.Drawing.Point(9, 46);
         this.chkVersionFileName.Name = "chkVersionFileName";
         this.chkVersionFileName.Size = new System.Drawing.Size(106, 17);
         this.chkVersionFileName.TabIndex = 3;
         this.chkVersionFileName.Text = "Version file name";
         // 
         // cbVersionSource
         // 
         this.cbVersionSource.DisplayMember = "Description";
         this.cbVersionSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbVersionSource.FormattingEnabled = true;
         this.cbVersionSource.Location = new System.Drawing.Point(110, 19);
         this.cbVersionSource.Name = "cbVersionSource";
         this.cbVersionSource.Size = new System.Drawing.Size(161, 21);
         this.cbVersionSource.TabIndex = 2;
         this.cbVersionSource.ValueMember = "Key";
         // 
         // lVersionSource
         // 
         this.lVersionSource.AutoSize = true;
         this.lVersionSource.Location = new System.Drawing.Point(6, 22);
         this.lVersionSource.Name = "lVersionSource";
         this.lVersionSource.Size = new System.Drawing.Size(80, 13);
         this.lVersionSource.TabIndex = 1;
         this.lVersionSource.Text = "Version source:";
         // 
         // cbTargetFramework
         // 
         this.cbTargetFramework.DisplayMember = "Description";
         this.cbTargetFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbTargetFramework.FormattingEnabled = true;
         this.cbTargetFramework.Location = new System.Drawing.Point(110, 19);
         this.cbTargetFramework.Name = "cbTargetFramework";
         this.cbTargetFramework.Size = new System.Drawing.Size(161, 21);
         this.cbTargetFramework.TabIndex = 6;
         this.cbTargetFramework.ValueMember = "Key";
         // 
         // lTargetFramework
         // 
         this.lTargetFramework.AutoSize = true;
         this.lTargetFramework.Location = new System.Drawing.Point(6, 22);
         this.lTargetFramework.Name = "lTargetFramework";
         this.lTargetFramework.Size = new System.Drawing.Size(93, 13);
         this.lTargetFramework.TabIndex = 5;
         this.lTargetFramework.Text = "Target framework:";
         // 
         // gbFramework
         // 
         this.gbFramework.Controls.Add(this.chkAddBinariesToSubfolder);
         this.gbFramework.Controls.Add(this.lTargetFramework);
         this.gbFramework.Controls.Add(this.cbTargetFramework);
         this.gbFramework.Location = new System.Drawing.Point(3, 81);
         this.gbFramework.Name = "gbFramework";
         this.gbFramework.Size = new System.Drawing.Size(345, 72);
         this.gbFramework.TabIndex = 4;
         this.gbFramework.TabStop = false;
         this.gbFramework.Text = "Framework";
         // 
         // chkAddBinariesToSubfolder
         // 
         this.chkAddBinariesToSubfolder.AutoSize = true;
         this.chkAddBinariesToSubfolder.Location = new System.Drawing.Point(9, 46);
         this.chkAddBinariesToSubfolder.Name = "chkAddBinariesToSubfolder";
         this.chkAddBinariesToSubfolder.Size = new System.Drawing.Size(196, 17);
         this.chkAddBinariesToSubfolder.TabIndex = 7;
         this.chkAddBinariesToSubfolder.Text = "Add referenced binaries to subfolder";
         this.chkAddBinariesToSubfolder.UseVisualStyleBackColor = true;
         // 
         // gbVersioning
         // 
         this.gbVersioning.Controls.Add(this.cbVersionSource);
         this.gbVersioning.Controls.Add(this.chkVersionFileName);
         this.gbVersioning.Controls.Add(this.lVersionSource);
         this.gbVersioning.Location = new System.Drawing.Point(3, 3);
         this.gbVersioning.Name = "gbVersioning";
         this.gbVersioning.Size = new System.Drawing.Size(345, 72);
         this.gbVersioning.TabIndex = 0;
         this.gbVersioning.TabStop = false;
         this.gbVersioning.Text = "Versioning";
         // 
         // gbTransformation
         // 
         this.gbTransformation.Controls.Add(this.chkTransformOnBuild);
         this.gbTransformation.Location = new System.Drawing.Point(3, 159);
         this.gbTransformation.Name = "gbTransformation";
         this.gbTransformation.Size = new System.Drawing.Size(345, 46);
         this.gbTransformation.TabIndex = 8;
         this.gbTransformation.TabStop = false;
         this.gbTransformation.Text = "Transformation";
         // 
         // chkTransformOnBuild
         // 
         this.chkTransformOnBuild.AutoSize = true;
         this.chkTransformOnBuild.Location = new System.Drawing.Point(9, 19);
         this.chkTransformOnBuild.Name = "chkTransformOnBuild";
         this.chkTransformOnBuild.Size = new System.Drawing.Size(113, 17);
         this.chkTransformOnBuild.TabIndex = 9;
         this.chkTransformOnBuild.Text = "Transform on build";
         this.chkTransformOnBuild.UseVisualStyleBackColor = true;
         // 
         // NuBuildPropertyPagePackageView
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.gbTransformation);
         this.Controls.Add(this.gbVersioning);
         this.Controls.Add(this.gbFramework);
         this.Name = "NuBuildPropertyPagePackageView";
         this.Size = new System.Drawing.Size(355, 213);
         this.gbFramework.ResumeLayout(false);
         this.gbFramework.PerformLayout();
         this.gbVersioning.ResumeLayout(false);
         this.gbVersioning.PerformLayout();
         this.gbTransformation.ResumeLayout(false);
         this.gbTransformation.PerformLayout();
         this.ResumeLayout(false);

		}

		#endregion

      private System.Windows.Forms.CheckBox chkVersionFileName;
      private System.Windows.Forms.ComboBox cbVersionSource;
      private System.Windows.Forms.Label lVersionSource;
      private System.Windows.Forms.ComboBox cbTargetFramework;
      private System.Windows.Forms.Label lTargetFramework;
      private System.Windows.Forms.GroupBox gbFramework;
      private System.Windows.Forms.GroupBox gbVersioning;
      private System.Windows.Forms.CheckBox chkAddBinariesToSubfolder;
      private System.Windows.Forms.GroupBox gbTransformation;
      private System.Windows.Forms.CheckBox chkTransformOnBuild;
	}
}