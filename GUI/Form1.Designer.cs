namespace GUI
{
    partial class Form1
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
            inputbox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            Resultbox = new TextBox();
            button1 = new Button();
            ErrorBox1 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            plotButton = new Button();
            labelXMin = new Label();
            labelXMax = new Label();
            labelStep = new Label();
            textXMin = new TextBox();
            textXMax = new TextBox();
            textStep = new TextBox();
            plotArea = new Panel();
            SuspendLayout();
            // 
            // inputbox
            // 
            inputbox.Location = new Point(172, 23);
            inputbox.Name = "inputbox";
            inputbox.Size = new Size(414, 27);
            inputbox.TabIndex = 0;
            // 
            // label1
            // 
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 23);
            label2.Name = "label2";
            label2.Size = new Size(142, 20);
            label2.TabIndex = 1;
            label2.Text = "Enter the expression";
            // 
            // Resultbox
            // 
            Resultbox.Location = new Point(172, 94);
            Resultbox.Name = "Resultbox";
            Resultbox.Size = new Size(574, 27);
            Resultbox.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(606, 23);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 4;
            button1.Text = "Evaluate";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ErrorBox1
            // 
            ErrorBox1.Location = new Point(172, 169);
            ErrorBox1.Name = "ErrorBox1";
            ErrorBox1.Size = new Size(574, 27);
            ErrorBox1.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(29, 97);
            label3.Name = "label3";
            label3.Size = new Size(45, 20);
            label3.TabIndex = 6;
            label3.Text = "result";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(29, 176);
            label4.Name = "label4";
            label4.Size = new Size(41, 20);
            label4.TabIndex = 7;
            label4.Text = "error";
            // 
            // plotButton
            // 
            plotButton.Location = new Point(706, 23);
            plotButton.Name = "plotButton";
            plotButton.Size = new Size(94, 29);
            plotButton.TabIndex = 8;
            plotButton.Text = "Plot";
            plotButton.UseVisualStyleBackColor = true;
            plotButton.Click += plotButton_Click;
            // 
            // Labels and inputs for range
            // 
            labelXMin.AutoSize = true;
            labelXMin.Location = new Point(12, 58);
            labelXMin.Name = "labelXMin";
            labelXMin.Size = new Size(49, 20);
            labelXMin.Text = "x min";

            textXMin.Location = new Point(67, 55);
            textXMin.Name = "textXMin";
            textXMin.Size = new Size(80, 27);
            textXMin.Text = "-10";

            labelXMax.AutoSize = true;
            labelXMax.Location = new Point(160, 58);
            labelXMax.Name = "labelXMax";
            labelXMax.Size = new Size(52, 20);
            labelXMax.Text = "x max";

            textXMax.Location = new Point(218, 55);
            textXMax.Name = "textXMax";
            textXMax.Size = new Size(80, 27);
            textXMax.Text = "10";

            labelStep.AutoSize = true;
            labelStep.Location = new Point(310, 58);
            labelStep.Name = "labelStep";
            labelStep.Size = new Size(35, 20);
            labelStep.Text = "dx";

            textStep.Location = new Point(351, 55);
            textStep.Name = "textStep";
            textStep.Size = new Size(80, 27);
            textStep.Text = "0.1";
            // 
            // plotArea
            // 
            plotArea.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            plotArea.Location = new Point(12, 212);
            plotArea.Name = "plotArea";
            plotArea.Size = new Size(776, 226);
            plotArea.TabIndex = 9;
            plotArea.BackColor = Color.White;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(812, 450);
            Controls.Add(plotArea);
            Controls.Add(textStep);
            Controls.Add(labelStep);
            Controls.Add(textXMax);
            Controls.Add(labelXMax);
            Controls.Add(textXMin);
            Controls.Add(labelXMin);
            Controls.Add(plotButton);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(ErrorBox1);
            Controls.Add(button1);
            Controls.Add(Resultbox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(inputbox);
            Name = "Form1";
            Text = "Function Plotter";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox inputbox;
        private Label label1;
        private Label label2;
        private TextBox Resultbox;
        private Button button1;
        private TextBox ErrorBox1;
        private Label label3;
        private Label label4;
        private Button plotButton;
        private Label labelXMin;
        private Label labelXMax;
        private Label labelStep;
        private TextBox textXMin;
        private TextBox textXMax;
        private TextBox textStep;
        private Panel plotArea;
    }
}
