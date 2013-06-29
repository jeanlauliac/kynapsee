namespace Kynapsee
{
    partial class GesturesForm
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
            this.listGestures = new System.Windows.Forms.ListBox();
            this.buttonCapture = new System.Windows.Forms.Button();
            this.textGesture = new System.Windows.Forms.TextBox();
            this.labelLastGesture = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.pictureSkeleton = new Kynapsee.SkeletonImage();
            this.SuspendLayout();
            // 
            // listGestures
            // 
            this.listGestures.FormattingEnabled = true;
            this.listGestures.ItemHeight = 15;
            this.listGestures.Location = new System.Drawing.Point(448, 44);
            this.listGestures.Name = "listGestures";
            this.listGestures.Size = new System.Drawing.Size(200, 319);
            this.listGestures.TabIndex = 3;
            this.listGestures.SelectedIndexChanged += new System.EventHandler(this.listGestures_SelectedIndexChanged);
            // 
            // buttonCapture
            // 
            this.buttonCapture.Location = new System.Drawing.Point(654, 15);
            this.buttonCapture.Name = "buttonCapture";
            this.buttonCapture.Size = new System.Drawing.Size(75, 23);
            this.buttonCapture.TabIndex = 4;
            this.buttonCapture.Text = "Capture";
            this.buttonCapture.UseVisualStyleBackColor = true;
            this.buttonCapture.Click += new System.EventHandler(this.buttonCapture_Click);
            // 
            // textGesture
            // 
            this.textGesture.Location = new System.Drawing.Point(448, 14);
            this.textGesture.Name = "textGesture";
            this.textGesture.Size = new System.Drawing.Size(200, 23);
            this.textGesture.TabIndex = 5;
            // 
            // labelLastGesture
            // 
            this.labelLastGesture.AutoSize = true;
            this.labelLastGesture.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastGesture.Location = new System.Drawing.Point(12, 377);
            this.labelLastGesture.Name = "labelLastGesture";
            this.labelLastGesture.Size = new System.Drawing.Size(188, 30);
            this.labelLastGesture.TabIndex = 6;
            this.labelLastGesture.Text = "Reading gestures...";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(654, 134);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 7;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(654, 105);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 8;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(654, 73);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 26);
            this.buttonImport.TabIndex = 9;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(654, 44);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 10;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // pictureSkeleton
            // 
            this.pictureSkeleton.BackColor = System.Drawing.Color.Black;
            this.pictureSkeleton.Location = new System.Drawing.Point(12, 14);
            this.pictureSkeleton.Name = "pictureSkeleton";
            this.pictureSkeleton.Size = new System.Drawing.Size(421, 353);
            this.pictureSkeleton.TabIndex = 1;
            this.pictureSkeleton.TabStop = false;
            this.pictureSkeleton.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureSkeleton_Paint);
            // 
            // GesturesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 421);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.labelLastGesture);
            this.Controls.Add(this.textGesture);
            this.Controls.Add(this.buttonCapture);
            this.Controls.Add(this.listGestures);
            this.Controls.Add(this.pictureSkeleton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GesturesForm";
            this.Text = "Manage Gestures";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GesturesForm_FormClosed);
            this.Load += new System.EventHandler(this.GesturesForm_Load);
            this.Shown += new System.EventHandler(this.GesturesForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SkeletonImage pictureSkeleton;
        private System.Windows.Forms.ListBox listGestures;
        private System.Windows.Forms.Button buttonCapture;
        private System.Windows.Forms.TextBox textGesture;
        private System.Windows.Forms.Label labelLastGesture;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonDelete;
    }
}