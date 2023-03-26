using System;

namespace PianoPlayingMotionGenerator {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.executeBtn = new System.Windows.Forms.Button();
            this.console = new System.Windows.Forms.TextBox();
            this.handFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.rightHandFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.leftHandFile = new System.Windows.Forms.TextBox();
            this.printRowCheckBox = new System.Windows.Forms.CheckBox();
            this.runTestCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // executeBtn
            // 
            this.executeBtn.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.executeBtn.Enabled = false;
            this.executeBtn.Location = new System.Drawing.Point(988, 544);
            this.executeBtn.Name = "executeBtn";
            this.executeBtn.Size = new System.Drawing.Size(112, 55);
            this.executeBtn.TabIndex = 0;
            this.executeBtn.Text = "执行";
            this.executeBtn.UseVisualStyleBackColor = true;
            this.executeBtn.Click += new System.EventHandler(this.executeBtn_Click);
            // 
            // console
            // 
            this.console.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.console.BackColor = System.Drawing.Color.Black;
            this.console.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.console.ForeColor = System.Drawing.Color.White;
            this.console.Location = new System.Drawing.Point(12, 12);
            this.console.Multiline = true;
            this.console.Name = "console";
            this.console.ReadOnly = true;
            this.console.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.console.Size = new System.Drawing.Size(1088, 519);
            this.console.TabIndex = 1;
            // 
            // handFileDialog
            // 
            this.handFileDialog.Filter = "逗号分隔值|*.csv";
            this.handFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.handFileDialog_FileOk);
            // 
            // rightHandFile
            // 
            this.rightHandFile.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rightHandFile.BackColor = System.Drawing.Color.White;
            this.rightHandFile.ForeColor = System.Drawing.SystemColors.WindowText;
            this.rightHandFile.Location = new System.Drawing.Point(67, 561);
            this.rightHandFile.Name = "rightHandFile";
            this.rightHandFile.ReadOnly = true;
            this.rightHandFile.Size = new System.Drawing.Size(213, 25);
            this.rightHandFile.TabIndex = 2;
            this.rightHandFile.Click += new System.EventHandler(this.rightHandFile_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 564);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "右手：";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(299, 566);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "左手：";
            // 
            // leftHandFile
            // 
            this.leftHandFile.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.leftHandFile.BackColor = System.Drawing.Color.White;
            this.leftHandFile.Location = new System.Drawing.Point(357, 561);
            this.leftHandFile.Name = "leftHandFile";
            this.leftHandFile.ReadOnly = true;
            this.leftHandFile.Size = new System.Drawing.Size(213, 25);
            this.leftHandFile.TabIndex = 5;
            this.leftHandFile.Click += new System.EventHandler(this.leftHandFile_Click);
            // 
            // printRowCheckBox
            // 
            this.printRowCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.printRowCheckBox.Location = new System.Drawing.Point(595, 562);
            this.printRowCheckBox.Name = "printRowCheckBox";
            this.printRowCheckBox.Size = new System.Drawing.Size(140, 24);
            this.printRowCheckBox.TabIndex = 6;
            this.printRowCheckBox.Text = "输出行信息";
            this.printRowCheckBox.UseVisualStyleBackColor = true;
            this.printRowCheckBox.CheckedChanged += new System.EventHandler(this.printRowCheckBox_CheckedChanged);
            // 
            // runTestCheckBox
            // 
            this.runTestCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.runTestCheckBox.Location = new System.Drawing.Point(721, 562);
            this.runTestCheckBox.Name = "runTestCheckBox";
            this.runTestCheckBox.Size = new System.Drawing.Size(152, 24);
            this.runTestCheckBox.TabIndex = 7;
            this.runTestCheckBox.Text = "运行测试";
            this.runTestCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 611);
            this.Controls.Add(this.runTestCheckBox);
            this.Controls.Add(this.printRowCheckBox);
            this.Controls.Add(this.leftHandFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rightHandFile);
            this.Controls.Add(this.console);
            this.Controls.Add(this.executeBtn);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PianoPlayingMotionGenerator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox console;
        private System.Windows.Forms.Button executeBtn;
        private System.Windows.Forms.OpenFileDialog handFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox leftHandFile;
        private System.Windows.Forms.CheckBox printRowCheckBox;
        private System.Windows.Forms.TextBox rightHandFile;
        private System.Windows.Forms.CheckBox runTestCheckBox;

        #endregion

    }
}