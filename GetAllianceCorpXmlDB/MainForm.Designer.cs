namespace GetAllianceCorpXmlDB
{
    partial class MainForm
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.corporationDatabaseLabel = new System.Windows.Forms.Label();
            this.allianceDatabaseLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Alliance Database:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Corporation Database:";
            // 
            // corporationDatabaseLabel
            // 
            this.corporationDatabaseLabel.AutoSize = true;
            this.corporationDatabaseLabel.Location = new System.Drawing.Point(198, 22);
            this.corporationDatabaseLabel.Name = "corporationDatabaseLabel";
            this.corporationDatabaseLabel.Size = new System.Drawing.Size(74, 13);
            this.corporationDatabaseLabel.TabIndex = 2;
            this.corporationDatabaseLabel.Text = "UNSTARTED";
            // 
            // allianceDatabaseLabel
            // 
            this.allianceDatabaseLabel.AutoSize = true;
            this.allianceDatabaseLabel.Location = new System.Drawing.Point(198, 9);
            this.allianceDatabaseLabel.Name = "allianceDatabaseLabel";
            this.allianceDatabaseLabel.Size = new System.Drawing.Size(74, 13);
            this.allianceDatabaseLabel.TabIndex = 3;
            this.allianceDatabaseLabel.Text = "UNSTARTED";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 45);
            this.Controls.Add(this.allianceDatabaseLabel);
            this.Controls.Add(this.corporationDatabaseLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label corporationDatabaseLabel;
        private System.Windows.Forms.Label allianceDatabaseLabel;
    }
}

