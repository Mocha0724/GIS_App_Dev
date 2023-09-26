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
            menuStrip1.ImageScalingSize = new Size(28, 28);
            menuStrip1.Items.AddRange(new ToolStripItem[] { 文件ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(11, 3, 0, 3);
            menuStrip1.Size = new Size(1467, 38);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            文件ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 加载图层ToolStripMenuItem, 新建图层ToolStripMenuItem });
            文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            文件ToolStripMenuItem.Size = new Size(72, 33);
            文件ToolStripMenuItem.Text = "文件";
            // 
            // 加载图层ToolStripMenuItem
            // 
            加载图层ToolStripMenuItem.Name = "加载图层ToolStripMenuItem";
            加载图层ToolStripMenuItem.Size = new Size(213, 40);
            加载图层ToolStripMenuItem.Text = "加载图层";
            // 
            // 新建图层ToolStripMenuItem
            // 
            新建图层ToolStripMenuItem.Name = "新建图层ToolStripMenuItem";
            新建图层ToolStripMenuItem.Size = new Size(213, 40);
            新建图层ToolStripMenuItem.Text = "新建图层";
            // 
            // mapControl1
            // 
            mapControl1.BackColor = SystemColors.Window;
            mapControl1.Location = new Point(221, 134);
            mapControl1.Margin = new Padding(11, 8, 11, 8);
            mapControl1.Name = "mapControl1";
            mapControl1.Size = new Size(1057, 626);
            mapControl1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1467, 894);
            Controls.Add(mapControl1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 3, 4, 3);
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