namespace Patcher
{
    partial class PatcherWindow
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
            this.AssemblyPathBox = new System.Windows.Forms.TextBox();
            this.SelectAssemblyButton = new System.Windows.Forms.Button();
            this.SelectFirstPassButton = new System.Windows.Forms.Button();
            this.FirstPassPathBox = new System.Windows.Forms.TextBox();
            this.SelectUnityEngineButton = new System.Windows.Forms.Button();
            this.UnityEnginePathBox = new System.Windows.Forms.TextBox();
            this.SelectUIButton = new System.Windows.Forms.Button();
            this.UIPathBox = new System.Windows.Forms.TextBox();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.RestoreButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.PatchButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AssemblyPathBox
            // 
            this.AssemblyPathBox.AcceptsTab = true;
            this.AssemblyPathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AssemblyPathBox.Location = new System.Drawing.Point(12, 12);
            this.AssemblyPathBox.Name = "AssemblyPathBox";
            this.AssemblyPathBox.Size = new System.Drawing.Size(586, 20);
            this.AssemblyPathBox.TabIndex = 0;
            this.AssemblyPathBox.Validating += new System.ComponentModel.CancelEventHandler(this.AssemblyPathBox_Validating);
            // 
            // SelectAssemblyButton
            // 
            this.SelectAssemblyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectAssemblyButton.Location = new System.Drawing.Point(604, 10);
            this.SelectAssemblyButton.Name = "SelectAssemblyButton";
            this.SelectAssemblyButton.Size = new System.Drawing.Size(187, 23);
            this.SelectAssemblyButton.TabIndex = 1;
            this.SelectAssemblyButton.Text = "Select Assembly-CSharp.dll";
            this.SelectAssemblyButton.UseVisualStyleBackColor = true;
            this.SelectAssemblyButton.Click += new System.EventHandler(this.SelectAssemblyButton_Click);
            // 
            // SelectFirstPassButton
            // 
            this.SelectFirstPassButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectFirstPassButton.Location = new System.Drawing.Point(604, 36);
            this.SelectFirstPassButton.Name = "SelectFirstPassButton";
            this.SelectFirstPassButton.Size = new System.Drawing.Size(187, 23);
            this.SelectFirstPassButton.TabIndex = 3;
            this.SelectFirstPassButton.Text = "Select Assembly-CSharp-firstpass.dll";
            this.SelectFirstPassButton.UseVisualStyleBackColor = true;
            this.SelectFirstPassButton.Click += new System.EventHandler(this.SelectFirstPassButton_Click);
            // 
            // FirstPassPathBox
            // 
            this.FirstPassPathBox.AcceptsTab = true;
            this.FirstPassPathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FirstPassPathBox.Location = new System.Drawing.Point(12, 38);
            this.FirstPassPathBox.Name = "FirstPassPathBox";
            this.FirstPassPathBox.Size = new System.Drawing.Size(586, 20);
            this.FirstPassPathBox.TabIndex = 2;
            // 
            // SelectUnityEngineButton
            // 
            this.SelectUnityEngineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectUnityEngineButton.Location = new System.Drawing.Point(604, 62);
            this.SelectUnityEngineButton.Name = "SelectUnityEngineButton";
            this.SelectUnityEngineButton.Size = new System.Drawing.Size(187, 23);
            this.SelectUnityEngineButton.TabIndex = 5;
            this.SelectUnityEngineButton.Text = "Select UnityEngine.dll";
            this.SelectUnityEngineButton.UseVisualStyleBackColor = true;
            this.SelectUnityEngineButton.Click += new System.EventHandler(this.SelectUnityEngineButton_Click);
            // 
            // UnityEnginePathBox
            // 
            this.UnityEnginePathBox.AcceptsTab = true;
            this.UnityEnginePathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UnityEnginePathBox.Location = new System.Drawing.Point(12, 64);
            this.UnityEnginePathBox.Name = "UnityEnginePathBox";
            this.UnityEnginePathBox.Size = new System.Drawing.Size(586, 20);
            this.UnityEnginePathBox.TabIndex = 4;
            // 
            // SelectUIButton
            // 
            this.SelectUIButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectUIButton.Location = new System.Drawing.Point(604, 89);
            this.SelectUIButton.Name = "SelectUIButton";
            this.SelectUIButton.Size = new System.Drawing.Size(187, 23);
            this.SelectUIButton.TabIndex = 7;
            this.SelectUIButton.Text = "Select UnityEngine.UI.dll";
            this.SelectUIButton.UseVisualStyleBackColor = true;
            this.SelectUIButton.Click += new System.EventHandler(this.SelectUIButton_Click);
            // 
            // UIPathBox
            // 
            this.UIPathBox.AcceptsTab = true;
            this.UIPathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UIPathBox.Location = new System.Drawing.Point(12, 91);
            this.UIPathBox.Name = "UIPathBox";
            this.UIPathBox.Size = new System.Drawing.Size(586, 20);
            this.UIPathBox.TabIndex = 6;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(12, 148);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(779, 23);
            this.ProgressBar.TabIndex = 8;
            // 
            // RestoreButton
            // 
            this.RestoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RestoreButton.Location = new System.Drawing.Point(12, 119);
            this.RestoreButton.Name = "RestoreButton";
            this.RestoreButton.Size = new System.Drawing.Size(255, 23);
            this.RestoreButton.TabIndex = 9;
            this.RestoreButton.Text = "Restore from backup";
            this.RestoreButton.UseVisualStyleBackColor = true;
            this.RestoreButton.Click += new System.EventHandler(this.RestoreButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateButton.Location = new System.Drawing.Point(273, 119);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(255, 23);
            this.UpdateButton.TabIndex = 10;
            this.UpdateButton.Text = "Update framework";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // PatchButton
            // 
            this.PatchButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PatchButton.Location = new System.Drawing.Point(534, 119);
            this.PatchButton.Name = "PatchButton";
            this.PatchButton.Size = new System.Drawing.Size(257, 23);
            this.PatchButton.TabIndex = 11;
            this.PatchButton.Text = "Patch";
            this.PatchButton.UseVisualStyleBackColor = true;
            this.PatchButton.Click += new System.EventHandler(this.PatchButton_Click);
            // 
            // PatcherWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(803, 181);
            this.Controls.Add(this.PatchButton);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.RestoreButton);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.SelectUIButton);
            this.Controls.Add(this.UIPathBox);
            this.Controls.Add(this.SelectUnityEngineButton);
            this.Controls.Add(this.UnityEnginePathBox);
            this.Controls.Add(this.SelectFirstPassButton);
            this.Controls.Add(this.FirstPassPathBox);
            this.Controls.Add(this.SelectAssemblyButton);
            this.Controls.Add(this.AssemblyPathBox);
            this.Name = "PatcherWindow";
            this.Text = "Planetbase Patcher";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox AssemblyPathBox;
        private System.Windows.Forms.Button SelectAssemblyButton;
        private System.Windows.Forms.Button SelectFirstPassButton;
        private System.Windows.Forms.TextBox FirstPassPathBox;
        private System.Windows.Forms.Button SelectUnityEngineButton;
        private System.Windows.Forms.TextBox UnityEnginePathBox;
        private System.Windows.Forms.Button SelectUIButton;
        private System.Windows.Forms.TextBox UIPathBox;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Button RestoreButton;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Button PatchButton;
    }
}

