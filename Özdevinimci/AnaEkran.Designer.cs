namespace Özdevinimci
{
    partial class AnaEkran
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
            this.Gösterge = new System.Windows.Forms.NotifyIcon(this.components);
            this.SağTuşMenü = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bilgisayarıKapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._Menü_ertele_5 = new System.Windows.Forms.ToolStripMenuItem();
            this._Menü_ertele_30 = new System.Windows.Forms.ToolStripMenuItem();
            this._Menü_ertele_60 = new System.Windows.Forms.ToolStripMenuItem();
            this._Menü_ertele_120 = new System.Windows.Forms.ToolStripMenuItem();
            this._Menü_ertele_0 = new System.Windows.Forms.ToolStripMenuItem();
            this.tarayıcıdaAçToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.çıkışToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SağTuşMenü.SuspendLayout();
            this.SuspendLayout();
            // 
            // Gösterge
            // 
            this.Gösterge.ContextMenuStrip = this.SağTuşMenü;
            this.Gösterge.Text = "notifyIcon1";
            this.Gösterge.Visible = true;
            // 
            // SağTuşMenü
            // 
            this.SağTuşMenü.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.SağTuşMenü.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.bilgisayarıKapatToolStripMenuItem,
            this.tarayıcıdaAçToolStripMenuItem,
            this.çıkışToolStripMenuItem});
            this.SağTuşMenü.Name = "SağTuşMenü";
            this.SağTuşMenü.ShowImageMargin = false;
            this.SağTuşMenü.ShowItemToolTips = false;
            this.SağTuşMenü.Size = new System.Drawing.Size(163, 82);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(159, 6);
            // 
            // bilgisayarıKapatToolStripMenuItem
            // 
            this.bilgisayarıKapatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._Menü_ertele_5,
            this._Menü_ertele_30,
            this._Menü_ertele_60,
            this._Menü_ertele_120,
            this._Menü_ertele_0});
            this.bilgisayarıKapatToolStripMenuItem.Name = "bilgisayarıKapatToolStripMenuItem";
            this.bilgisayarıKapatToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.bilgisayarıKapatToolStripMenuItem.Text = "Bilgisayarı kapat";
            // 
            // _Menü_ertele_5
            // 
            this._Menü_ertele_5.Name = "_Menü_ertele_5";
            this._Menü_ertele_5.Size = new System.Drawing.Size(224, 26);
            this._Menü_ertele_5.Text = "5 dk. ertele";
            this._Menü_ertele_5.Click += new System.EventHandler(this._Menü_ertele_x_Click);
            // 
            // _Menü_ertele_30
            // 
            this._Menü_ertele_30.Name = "_Menü_ertele_30";
            this._Menü_ertele_30.Size = new System.Drawing.Size(224, 26);
            this._Menü_ertele_30.Text = "30 dk. ertele";
            this._Menü_ertele_30.Click += new System.EventHandler(this._Menü_ertele_x_Click);
            // 
            // _Menü_ertele_60
            // 
            this._Menü_ertele_60.Name = "_Menü_ertele_60";
            this._Menü_ertele_60.Size = new System.Drawing.Size(224, 26);
            this._Menü_ertele_60.Text = "1 saat ertele";
            this._Menü_ertele_60.Click += new System.EventHandler(this._Menü_ertele_x_Click);
            // 
            // _Menü_ertele_120
            // 
            this._Menü_ertele_120.Name = "_Menü_ertele_120";
            this._Menü_ertele_120.Size = new System.Drawing.Size(224, 26);
            this._Menü_ertele_120.Text = "2 saat ertele";
            this._Menü_ertele_120.Click += new System.EventHandler(this._Menü_ertele_x_Click);
            // 
            // _Menü_ertele_0
            // 
            this._Menü_ertele_0.Name = "_Menü_ertele_0";
            this._Menü_ertele_0.Size = new System.Drawing.Size(224, 26);
            this._Menü_ertele_0.Text = "İptal et";
            this._Menü_ertele_0.Click += new System.EventHandler(this._Menü_ertele_x_Click);
            // 
            // tarayıcıdaAçToolStripMenuItem
            // 
            this.tarayıcıdaAçToolStripMenuItem.Name = "tarayıcıdaAçToolStripMenuItem";
            this.tarayıcıdaAçToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.tarayıcıdaAçToolStripMenuItem.Text = "Tarayıcıda aç";
            this.tarayıcıdaAçToolStripMenuItem.Click += new System.EventHandler(this.tarayıcıdaAçToolStripMenuItem_Click);
            // 
            // çıkışToolStripMenuItem
            // 
            this.çıkışToolStripMenuItem.Name = "çıkışToolStripMenuItem";
            this.çıkışToolStripMenuItem.Size = new System.Drawing.Size(162, 24);
            this.çıkışToolStripMenuItem.Text = "Çıkış";
            this.çıkışToolStripMenuItem.Click += new System.EventHandler(this.çıkışToolStripMenuItem_Click);
            // 
            // AnaEkran
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(251, 24);
            this.Name = "AnaEkran";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Özdevinimci";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnaEkran_FormClosed);
            this.SağTuşMenü.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon Gösterge;
        private System.Windows.Forms.ContextMenuStrip SağTuşMenü;
        private System.Windows.Forms.ToolStripMenuItem çıkışToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tarayıcıdaAçToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem bilgisayarıKapatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _Menü_ertele_5;
        private System.Windows.Forms.ToolStripMenuItem _Menü_ertele_30;
        private System.Windows.Forms.ToolStripMenuItem _Menü_ertele_60;
        private System.Windows.Forms.ToolStripMenuItem _Menü_ertele_120;
        private System.Windows.Forms.ToolStripMenuItem _Menü_ertele_0;
    }
}

