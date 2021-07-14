namespace Grammar.Explorer.Forms
{
    partial class GrammarSelector
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
            System.Windows.Forms.Button cancelButton;
            this.okButton = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.managerBox = new System.Windows.Forms.ListBox();
            this.messageBox = new System.Windows.Forms.TextBox();
            cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // cancelButton
            //
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(239, 189);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseMnemonic = false;
            cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Enabled = false;
            this.okButton.Location = new System.Drawing.Point(158, 189);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // browseButton
            //
            this.browseButton.Location = new System.Drawing.Point(320, 189);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "&Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            //
            // managerBox
            //
            this.managerBox.DisplayMember = "FullName";
            this.managerBox.Location = new System.Drawing.Point(13, 13);
            this.managerBox.Name = "managerBox";
            this.managerBox.Size = new System.Drawing.Size(382, 82);
            this.managerBox.TabIndex = 0;
            this.managerBox.SelectedIndexChanged += new System.EventHandler(this.managerBox_SelectedIndexChanged);
            //
            // messageBox
            //
            this.messageBox.CausesValidation = false;
            this.messageBox.Location = new System.Drawing.Point(13, 101);
            this.messageBox.Multiline = true;
            this.messageBox.Name = "messageBox";
            this.messageBox.ReadOnly = true;
            this.messageBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.messageBox.Size = new System.Drawing.Size(382, 82);
            this.messageBox.TabIndex = 4;
            this.messageBox.TabStop = false;
            //
            // GrammarSelector
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = cancelButton;
            this.ClientSize = new System.Drawing.Size(407, 224);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.managerBox);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GrammarSelector";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Grammar Selector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.ListBox managerBox;
        private System.Windows.Forms.TextBox messageBox;
    }
}