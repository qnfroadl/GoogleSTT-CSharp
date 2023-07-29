namespace WaktaverseSTT
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            micButton = new Button();
            fileOpenButton = new Button();
            openFileDialog1 = new OpenFileDialog();
            label1 = new Label();
            pathLabel = new Label();
            fileSTTTestButton = new Button();
            volumeMeter1 = new NAudio.Gui.VolumeMeter();
            listBox1 = new ListBox();
            statusStrip1 = new StatusStrip();
            statusLabel1 = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // micButton
            // 
            micButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            micButton.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            micButton.Location = new Point(12, 64);
            micButton.Name = "micButton";
            micButton.Size = new Size(678, 72);
            micButton.TabIndex = 4;
            micButton.Text = "Monitoring Start";
            micButton.UseVisualStyleBackColor = true;
            micButton.Click += micButton_Click;
            // 
            // fileOpenButton
            // 
            fileOpenButton.Location = new Point(12, 12);
            fileOpenButton.Name = "fileOpenButton";
            fileOpenButton.Size = new Size(75, 23);
            fileOpenButton.TabIndex = 5;
            fileOpenButton.Text = "File Select";
            fileOpenButton.UseVisualStyleBackColor = true;
            fileOpenButton.Click += fileOpenButton_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // label1
            // 
            label1.Location = new Point(12, 38);
            label1.Name = "label1";
            label1.Size = new Size(41, 23);
            label1.TabIndex = 6;
            label1.Text = "Path: ";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pathLabel
            // 
            pathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pathLabel.Location = new Point(62, 38);
            pathLabel.Name = "pathLabel";
            pathLabel.Size = new Size(628, 23);
            pathLabel.TabIndex = 7;
            pathLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // fileSTTTestButton
            // 
            fileSTTTestButton.Location = new Point(93, 12);
            fileSTTTestButton.Name = "fileSTTTestButton";
            fileSTTTestButton.Size = new Size(104, 23);
            fileSTTTestButton.TabIndex = 8;
            fileSTTTestButton.Text = "File STT Test";
            fileSTTTestButton.UseVisualStyleBackColor = true;
            fileSTTTestButton.Click += fileSTTTestButton_Click;
            // 
            // volumeMeter1
            // 
            volumeMeter1.Amplitude = 0F;
            volumeMeter1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            volumeMeter1.ForeColor = Color.LawnGreen;
            volumeMeter1.Location = new Point(12, 142);
            volumeMeter1.MaxDb = 0F;
            volumeMeter1.MinDb = -60F;
            volumeMeter1.Name = "volumeMeter1";
            volumeMeter1.Orientation = Orientation.Horizontal;
            volumeMeter1.Size = new Size(678, 38);
            volumeMeter1.TabIndex = 1;
            volumeMeter1.Text = "volumeMeter1";
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 186);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(675, 274);
            listBox1.TabIndex = 9;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel1 });
            statusStrip1.Location = new Point(0, 468);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(699, 22);
            statusStrip1.TabIndex = 10;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel1
            // 
            statusLabel1.Name = "statusLabel1";
            statusLabel1.Size = new Size(0, 17);
            statusLabel1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(699, 490);
            Controls.Add(statusStrip1);
            Controls.Add(listBox1);
            Controls.Add(fileSTTTestButton);
            Controls.Add(pathLabel);
            Controls.Add(label1);
            Controls.Add(fileOpenButton);
            Controls.Add(micButton);
            Controls.Add(volumeMeter1);
            Name = "MainForm";
            Text = "WaktaverseSTT";
            SizeChanged += MainForm_SizeChanged;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button micButton;
        private Button fileOpenButton;
        private OpenFileDialog openFileDialog1;
        private Label label1;
        private Label pathLabel;
        private Button fileSTTTestButton;
        private NAudio.Gui.VolumeMeter volumeMeter1;
        private ListBox listBox1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel1;
    }
}