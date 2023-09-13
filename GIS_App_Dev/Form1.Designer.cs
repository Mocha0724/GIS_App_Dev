namespace GIS_APP_DEV
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
            menuStrip1 = new MenuStrip();
            文件ToolStripMenuItem = new ToolStripMenuItem();
            加载图层ToolStripMenuItem = new ToolStripMenuItem();
            新建图层ToolStripMenuItem = new ToolStripMenuItem();
            mapControl1 = new MapControl();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { 文件ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(790, 25);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            文件ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 加载图层ToolStripMenuItem, 新建图层ToolStripMenuItem });
            文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            文件ToolStripMenuItem.Size = new Size(44, 21);
            文件ToolStripMenuItem.Text = "文件";
            // 
            // 加载图层ToolStripMenuItem
            // 
            加载图层ToolStripMenuItem.Name = "加载图层ToolStripMenuItem";
            加载图层ToolStripMenuItem.Size = new Size(124, 22);
            加载图层ToolStripMenuItem.Text = "加载图层";
            // 
            // 新建图层ToolStripMenuItem
            // 
            新建图层ToolStripMenuItem.Name = "新建图层ToolStripMenuItem";
            新建图层ToolStripMenuItem.Size = new Size(124, 22);
            新建图层ToolStripMenuItem.Text = "新建图层";
            // 
            // mapControl1
            // 
            mapControl1.BackColor = SystemColors.Window;
            mapControl1.Location = new Point(100, 38);
            mapControl1.Name = "mapControl1";
            mapControl1.Size = new Size(569, 380);
            mapControl1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(790, 543);
            Controls.Add(mapControl1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Form1";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem 文件ToolStripMenuItem;
        private ToolStripMenuItem 加载图层ToolStripMenuItem;
        private ToolStripMenuItem 新建图层ToolStripMenuItem;
        private MapControl mapControl1;
    }
}