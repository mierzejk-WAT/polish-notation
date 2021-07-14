namespace Grammar.Explorer.Forms
{
    using System;
    using System.Windows.Forms;

    partial class MainForm
    {
        private sealed class FocusLessButton : System.Windows.Forms.Button
        {
            protected override bool ShowFocusCues => false;

            protected override void OnGotFocus(EventArgs e)
            {
                this.BackColor = this.FlatAppearance.MouseOverBackColor;
                base.OnGotFocus(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);
                this.ResetBackColor();
            }
        }

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
            if (disposing)
            {
                this.components?.Dispose();
                this.outputWriter?.Dispose();
                this.savingCancellationSrc?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent(Type[] managerTypes = null)
        {
            System.Windows.Forms.Label varLabel;
            System.Windows.Forms.Label descLabel;
            System.Windows.Forms.GroupBox groupBox;
            System.Windows.Forms.Label eqLabel;
            this.varText = new System.Windows.Forms.Label();
            this.descText = new System.Windows.Forms.Label();
            this.sourceCode = new System.Windows.Forms.TextBox();
            this.compile = new System.Windows.Forms.Button();
            this.moduleBox = new System.Windows.Forms.ComboBox();
            this.varBox = new System.Windows.Forms.TextBox();
            this.calcButton = new System.Windows.Forms.Button();
            this.resultText = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.pythonCode = new System.Windows.Forms.TextBox();
            this.execButton = new Grammar.Explorer.Forms.MainForm.FocusLessButton();
            this.resultBox = new System.Windows.Forms.TextBox();
            this.loadButton = new System.Windows.Forms.Button();
            varLabel = new System.Windows.Forms.Label();
            descLabel = new System.Windows.Forms.Label();
            groupBox = new System.Windows.Forms.GroupBox();
            eqLabel = new System.Windows.Forms.Label();
            groupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // varLabel
            //
            varLabel.CausesValidation = false;
            varLabel.Location = new System.Drawing.Point(6, 32);
            varLabel.Name = "varLabel";
            varLabel.Size = new System.Drawing.Size(53, 13);
            varLabel.TabIndex = 4;
            varLabel.Text = "Variables:";
            varLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // descLabel
            //
            descLabel.CausesValidation = false;
            descLabel.Location = new System.Drawing.Point(6, 15);
            descLabel.Name = "descLabel";
            descLabel.Size = new System.Drawing.Size(63, 13);
            descLabel.TabIndex = 5;
            descLabel.Text = "Description:";
            descLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // groupBox
            //
            groupBox.CausesValidation = false;
            groupBox.Controls.Add(this.varText);
            groupBox.Controls.Add(varLabel);
            groupBox.Controls.Add(this.descText);
            groupBox.Controls.Add(descLabel);
            groupBox.Location = new System.Drawing.Point(173, 233);
            groupBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox.Name = "groupBox";
            groupBox.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox.Size = new System.Drawing.Size(499, 50);
            groupBox.TabIndex = 3;
            groupBox.TabStop = false;
            groupBox.Text = "Details";
            //
            // varText
            //
            this.varText.AutoSize = true;
            this.varText.CausesValidation = false;
            this.varText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varText.Location = new System.Drawing.Point(76, 32);
            this.varText.Name = "varText";
            this.varText.Size = new System.Drawing.Size(0, 13);
            this.varText.TabIndex = 3;
            this.varText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // descText
            //
            this.descText.AutoSize = true;
            this.descText.CausesValidation = false;
            this.descText.Location = new System.Drawing.Point(76, 15);
            this.descText.Name = "descText";
            this.descText.Size = new System.Drawing.Size(0, 13);
            this.descText.TabIndex = 1;
            this.descText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // eqLabel
            //
            eqLabel.Location = new System.Drawing.Point(77, 321);
            eqLabel.Name = "eqLabel";
            eqLabel.Size = new System.Drawing.Size(13, 13);
            eqLabel.TabIndex = 0;
            eqLabel.Text = "=";
            eqLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // sourceCode
            //
            this.sourceCode.CausesValidation = false;
            this.sourceCode.Location = new System.Drawing.Point(0, 0);
            this.sourceCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.sourceCode.Multiline = true;
            this.sourceCode.Name = "sourceCode";
            this.sourceCode.Size = new System.Drawing.Size(320, 227);
            this.sourceCode.TabIndex = 0;
            //
            // compile
            //
            this.compile.CausesValidation = false;
            this.compile.Location = new System.Drawing.Point(12, 233);
            this.compile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.compile.Name = "compile";
            this.compile.Size = new System.Drawing.Size(75, 22);
            this.compile.TabIndex = 1;
            this.compile.Text = "Compile";
            this.compile.UseVisualStyleBackColor = true;
            this.compile.Click += new System.EventHandler(this.compile_Click);
            //
            // moduleBox
            //
            this.moduleBox.CausesValidation = false;
            this.moduleBox.DisplayMember = "Name";
            this.moduleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.moduleBox.DropDownWidth = 160;
            this.moduleBox.Location = new System.Drawing.Point(12, 262);
            this.moduleBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.moduleBox.Name = "moduleBox";
            this.moduleBox.Size = new System.Drawing.Size(154, 21);
            this.moduleBox.TabIndex = 2;
            this.moduleBox.SelectedIndexChanged += new System.EventHandler(this.typeBox_SelectedIndexChanged);
            //
            // varBox
            //
            this.varBox.CausesValidation = false;
            this.varBox.Location = new System.Drawing.Point(71, 290);
            this.varBox.Name = "varBox";
            this.varBox.Size = new System.Drawing.Size(601, 20);
            this.varBox.TabIndex = 4;
            this.varBox.WordWrap = false;
            this.varBox.TextChanged += new System.EventHandler(this.varBox_TextChanged);
            //
            // calcButton
            //
            this.calcButton.CausesValidation = false;
            this.calcButton.Enabled = false;
            this.calcButton.Location = new System.Drawing.Point(12, 316);
            this.calcButton.Name = "calcButton";
            this.calcButton.Size = new System.Drawing.Size(59, 23);
            this.calcButton.TabIndex = 5;
            this.calcButton.Text = "Calculate";
            this.calcButton.UseVisualStyleBackColor = true;
            this.calcButton.Click += new System.EventHandler(this.calcButton_Click);
            //
            // resultText
            //
            this.resultText.AutoSize = true;
            this.resultText.CausesValidation = false;
            this.resultText.Location = new System.Drawing.Point(96, 321);
            this.resultText.Name = "resultText";
            this.resultText.Size = new System.Drawing.Size(0, 13);
            this.resultText.TabIndex = 0;
            this.resultText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // saveButton
            //
            this.saveButton.Enabled = false;
            this.saveButton.Location = new System.Drawing.Point(613, 316);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(59, 23);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            //
            // pythonCode
            //
            this.pythonCode.CausesValidation = false;
            this.pythonCode.Location = new System.Drawing.Point(364, 0);
            this.pythonCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pythonCode.Multiline = true;
            this.pythonCode.Name = "pythonCode";
            this.pythonCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.pythonCode.Size = new System.Drawing.Size(320, 138);
            this.pythonCode.TabIndex = 8;
            //
            // execButton
            //
            this.execButton.BackColor = System.Drawing.SystemColors.Control;
            this.execButton.CausesValidation = false;
            this.execButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.execButton.FlatAppearance.BorderSize = 0;
            this.execButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
            this.execButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.execButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.execButton.ForeColor = System.Drawing.SystemColors.Control;
            this.execButton.Image = global::Grammar.Explorer.Properties.Resources.python;
            this.execButton.Location = new System.Drawing.Point(320, 94);
            this.execButton.Margin = new System.Windows.Forms.Padding(0);
            this.execButton.Name = "execButton";
            this.execButton.Size = new System.Drawing.Size(44, 44);
            this.execButton.TabIndex = 9;
            this.execButton.UseMnemonic = false;
            this.execButton.UseVisualStyleBackColor = true;
            this.execButton.Click += new System.EventHandler(this.execButton_Click);
            //
            // resultBox
            //
            this.resultBox.CausesValidation = false;
            this.resultBox.Location = new System.Drawing.Point(365, 146);
            this.resultBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.resultBox.Multiline = true;
            this.resultBox.Name = "resultBox";
            this.resultBox.ReadOnly = true;
            this.resultBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultBox.Size = new System.Drawing.Size(319, 81);
            this.resultBox.TabIndex = 10;
            this.resultBox.TabStop = false;
            this.resultBox.Font = new System.Drawing.Font(@"Consolas", 9f);
            //
            // loadButton
            //
            this.loadButton.Location = new System.Drawing.Point(548, 316);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(59, 23);
            this.loadButton.TabIndex = 6;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            //
            // MainForm
            //
            this.Enabled = false;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(684, 345);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.execButton);
            this.Controls.Add(this.pythonCode);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.resultText);
            this.Controls.Add(eqLabel);
            this.Controls.Add(this.calcButton);
            this.Controls.Add(this.varBox);
            this.Controls.Add(groupBox);
            this.Controls.Add(this.moduleBox);
            this.Controls.Add(this.compile);
            this.Controls.Add(this.sourceCode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Formal Language Compiler & Evaluator";
            this.FormClosing += this.MainForm_FormClosing;
            this.Shown += this.GetFormShown(managerTypes);
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox sourceCode;
        private System.Windows.Forms.Button compile;
        private System.Windows.Forms.ComboBox moduleBox;
        private System.Windows.Forms.Label descText;
        private System.Windows.Forms.Label varText;
        private System.Windows.Forms.TextBox varBox;
        private System.Windows.Forms.Button calcButton;
        private System.Windows.Forms.Label resultText;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox pythonCode;
        private FocusLessButton execButton;
        private System.Windows.Forms.TextBox resultBox;
        private System.Windows.Forms.Button loadButton;
    }
}

