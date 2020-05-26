namespace ISXEVE_EntityIterationProfiler
{
	partial class Form1
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
			this.buttonIterateEntities = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonIterateEntities
			// 
			this.buttonIterateEntities.Location = new System.Drawing.Point(12, 12);
			this.buttonIterateEntities.Name = "buttonIterateEntities";
			this.buttonIterateEntities.Size = new System.Drawing.Size(260, 23);
			this.buttonIterateEntities.TabIndex = 0;
			this.buttonIterateEntities.Text = "Iterate some Entities";
			this.buttonIterateEntities.UseVisualStyleBackColor = true;
			this.buttonIterateEntities.Click += new System.EventHandler(this.buttonIterateEntities_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 46);
			this.Controls.Add(this.buttonIterateEntities);
			this.Name = "Form1";
			this.Text = "ISXEVE Entity Iteration Profiler";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonIterateEntities;
	}
}

