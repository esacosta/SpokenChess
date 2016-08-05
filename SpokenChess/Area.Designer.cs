namespace SpeechToText
{
  partial class Area
  {
    
    private System.ComponentModel.IContainer components = null;

   
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    
    private void InitializeComponent()
    {
      this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
      this.SuspendLayout();
      // 
      // Area
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
      this.ClientSize = new System.Drawing.Size(518, 416);
      this.ControlBox = false;
      this.Cursor = System.Windows.Forms.Cursors.Arrow;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "Area";
      this.Opacity = 0.4D;
      this.Text = "Form1";
      this.TransparencyKey = System.Drawing.Color.White;
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.ResumeLayout(false);

    }


    private System.Windows.Forms.SaveFileDialog saveFileDialog1;

  }
}

