﻿namespace testMQ
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readIMEIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAppsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveNextItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movePreviusItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appSwitchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopScanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.upToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.playScripToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(1275, 817);
            this.splitContainer1.SplitterDistance = 489;
            this.splitContainer1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(489, 817);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.controlToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(782, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calibrationToolStripMenuItem,
            this.readIMEIToolStripMenuItem,
            this.goToSettingsToolStripMenuItem,
            this.closeAppsToolStripMenuItem,
            this.testToolStripMenuItem,
            this.recordToolStripMenuItem,
            this.playScripToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // calibrationToolStripMenuItem
            // 
            this.calibrationToolStripMenuItem.Name = "calibrationToolStripMenuItem";
            this.calibrationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.calibrationToolStripMenuItem.Text = "Calibration";
            this.calibrationToolStripMenuItem.Click += new System.EventHandler(this.calibrationToolStripMenuItem_Click);
            // 
            // readIMEIToolStripMenuItem
            // 
            this.readIMEIToolStripMenuItem.Name = "readIMEIToolStripMenuItem";
            this.readIMEIToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.readIMEIToolStripMenuItem.Text = "Read IMEI";
            this.readIMEIToolStripMenuItem.Click += new System.EventHandler(this.readIMEIToolStripMenuItem_Click);
            // 
            // goToSettingsToolStripMenuItem
            // 
            this.goToSettingsToolStripMenuItem.Name = "goToSettingsToolStripMenuItem";
            this.goToSettingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.goToSettingsToolStripMenuItem.Text = "Go to Settings";
            this.goToSettingsToolStripMenuItem.Click += new System.EventHandler(this.goToSettingsToolStripMenuItem_Click);
            // 
            // closeAppsToolStripMenuItem
            // 
            this.closeAppsToolStripMenuItem.Name = "closeAppsToolStripMenuItem";
            this.closeAppsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeAppsToolStripMenuItem.Text = "Close Apps";
            this.closeAppsToolStripMenuItem.Click += new System.EventHandler(this.closeAppsToolStripMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.testToolStripMenuItem.Text = "Test";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // recordToolStripMenuItem
            // 
            this.recordToolStripMenuItem.Name = "recordToolStripMenuItem";
            this.recordToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.recordToolStripMenuItem.Text = "Record";
            this.recordToolStripMenuItem.Click += new System.EventHandler(this.recordToolStripMenuItem_Click);
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveNextItemToolStripMenuItem,
            this.movePreviusItemToolStripMenuItem,
            this.selectItemToolStripMenuItem,
            this.tapToolStripMenuItem,
            this.goToHomeToolStripMenuItem,
            this.appSwitchToolStripMenuItem,
            this.stopScanningToolStripMenuItem,
            this.scrollToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.controlToolStripMenuItem.Text = "Control";
            // 
            // moveNextItemToolStripMenuItem
            // 
            this.moveNextItemToolStripMenuItem.Name = "moveNextItemToolStripMenuItem";
            this.moveNextItemToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.moveNextItemToolStripMenuItem.Text = "Move Next Item";
            this.moveNextItemToolStripMenuItem.Click += new System.EventHandler(this.moveNextItemToolStripMenuItem_Click);
            // 
            // movePreviusItemToolStripMenuItem
            // 
            this.movePreviusItemToolStripMenuItem.Name = "movePreviusItemToolStripMenuItem";
            this.movePreviusItemToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.movePreviusItemToolStripMenuItem.Text = "Move Previus Item";
            this.movePreviusItemToolStripMenuItem.Click += new System.EventHandler(this.movePreviusItemToolStripMenuItem_Click);
            // 
            // selectItemToolStripMenuItem
            // 
            this.selectItemToolStripMenuItem.Name = "selectItemToolStripMenuItem";
            this.selectItemToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.selectItemToolStripMenuItem.Text = "Select Item";
            this.selectItemToolStripMenuItem.Click += new System.EventHandler(this.selectItemToolStripMenuItem_Click);
            // 
            // tapToolStripMenuItem
            // 
            this.tapToolStripMenuItem.Name = "tapToolStripMenuItem";
            this.tapToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.tapToolStripMenuItem.Text = "Tap";
            this.tapToolStripMenuItem.Click += new System.EventHandler(this.tapToolStripMenuItem_Click);
            // 
            // goToHomeToolStripMenuItem
            // 
            this.goToHomeToolStripMenuItem.Name = "goToHomeToolStripMenuItem";
            this.goToHomeToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.goToHomeToolStripMenuItem.Text = "Go To Home";
            this.goToHomeToolStripMenuItem.Click += new System.EventHandler(this.goToHomeToolStripMenuItem_Click);
            // 
            // appSwitchToolStripMenuItem
            // 
            this.appSwitchToolStripMenuItem.Name = "appSwitchToolStripMenuItem";
            this.appSwitchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.appSwitchToolStripMenuItem.Text = "App Switch";
            this.appSwitchToolStripMenuItem.Click += new System.EventHandler(this.appSwitchToolStripMenuItem_Click);
            // 
            // stopScanningToolStripMenuItem
            // 
            this.stopScanningToolStripMenuItem.Name = "stopScanningToolStripMenuItem";
            this.stopScanningToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.stopScanningToolStripMenuItem.Text = "Stop Scanning";
            this.stopScanningToolStripMenuItem.Click += new System.EventHandler(this.stopScanningToolStripMenuItem_Click);
            // 
            // scrollToolStripMenuItem
            // 
            this.scrollToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.upToolStripMenuItem,
            this.downToolStripMenuItem,
            this.leftToolStripMenuItem,
            this.rightToolStripMenuItem});
            this.scrollToolStripMenuItem.Name = "scrollToolStripMenuItem";
            this.scrollToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.scrollToolStripMenuItem.Text = "Scroll";
            // 
            // upToolStripMenuItem
            // 
            this.upToolStripMenuItem.Name = "upToolStripMenuItem";
            this.upToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.upToolStripMenuItem.Text = "Up";
            this.upToolStripMenuItem.Click += new System.EventHandler(this.upToolStripMenuItem_Click);
            // 
            // downToolStripMenuItem
            // 
            this.downToolStripMenuItem.Name = "downToolStripMenuItem";
            this.downToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.downToolStripMenuItem.Text = "Down";
            this.downToolStripMenuItem.Click += new System.EventHandler(this.downToolStripMenuItem_Click);
            // 
            // leftToolStripMenuItem
            // 
            this.leftToolStripMenuItem.Name = "leftToolStripMenuItem";
            this.leftToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.leftToolStripMenuItem.Text = "Left";
            this.leftToolStripMenuItem.Click += new System.EventHandler(this.leftToolStripMenuItem_Click);
            // 
            // rightToolStripMenuItem
            // 
            this.rightToolStripMenuItem.Name = "rightToolStripMenuItem";
            this.rightToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.rightToolStripMenuItem.Text = "Right";
            this.rightToolStripMenuItem.Click += new System.EventHandler(this.rightToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // playScripToolStripMenuItem
            // 
            this.playScripToolStripMenuItem.Name = "playScripToolStripMenuItem";
            this.playScripToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.playScripToolStripMenuItem.Text = "Play Script";
            this.playScripToolStripMenuItem.Click += new System.EventHandler(this.playScripToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1275, 817);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem controlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveNextItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem movePreviusItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToHomeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem appSwitchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readIMEIToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem closeAppsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopScanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scrollToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem upToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playScripToolStripMenuItem;
    }
}

