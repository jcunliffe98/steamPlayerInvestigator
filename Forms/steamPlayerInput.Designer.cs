namespace steamPlayerInvestigator
{
    partial class steamPlayerInput
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
            this.manualConfirmButton = new System.Windows.Forms.Button();
            this.inputLabel = new System.Windows.Forms.Label();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.automaticConfirmButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // manualConfirmButton
            // 
            this.manualConfirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manualConfirmButton.Location = new System.Drawing.Point(373, 204);
            this.manualConfirmButton.Name = "manualConfirmButton";
            this.manualConfirmButton.Size = new System.Drawing.Size(218, 29);
            this.manualConfirmButton.TabIndex = 0;
            this.manualConfirmButton.Text = "Manual Investigation";
            this.manualConfirmButton.UseVisualStyleBackColor = true;
            this.manualConfirmButton.Click += new System.EventHandler(this.manualConfirmButton_ClickAsync);
            // 
            // inputLabel
            // 
            this.inputLabel.AutoSize = true;
            this.inputLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputLabel.Location = new System.Drawing.Point(236, 149);
            this.inputLabel.Name = "inputLabel";
            this.inputLabel.Size = new System.Drawing.Size(285, 20);
            this.inputLabel.TabIndex = 1;
            this.inputLabel.Text = "Input a Steam ID or Steam Profile URL";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputTextBox.Location = new System.Drawing.Point(156, 172);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(435, 26);
            this.inputTextBox.TabIndex = 2;
            // 
            // automaticConfirmButton
            // 
            this.automaticConfirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.automaticConfirmButton.Location = new System.Drawing.Point(156, 204);
            this.automaticConfirmButton.Name = "automaticConfirmButton";
            this.automaticConfirmButton.Size = new System.Drawing.Size(211, 29);
            this.automaticConfirmButton.TabIndex = 3;
            this.automaticConfirmButton.Text = "Automatic Investigation";
            this.automaticConfirmButton.UseVisualStyleBackColor = true;
            this.automaticConfirmButton.Click += new System.EventHandler(this.automaticConfirmButton_ClickAsync);
            // 
            // steamPlayerInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.automaticConfirmButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.inputLabel);
            this.Controls.Add(this.manualConfirmButton);
            this.Name = "steamPlayerInput";
            this.Text = "Steam Player Investigator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button manualConfirmButton;
        private System.Windows.Forms.Label inputLabel;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button automaticConfirmButton;
    }
}

