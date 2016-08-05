﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
namespace SpeechToText
{
  public partial class Area : Form
  {
    //----------------------------------------------------------------------------
    public enum CursPos : int
    {
      WithinSelectionArea = 0,
      OutsideSelectionArea,
      TopLine,
      BottomLine,
      LeftLine,
      RightLine,
      TopLeft,
      TopRight,
      BottomLeft,
      BottomRight
    }

    //----------------------------------------------------------------------------
    public enum ClickAction : int
    {
      NoClick = 0,
      Dragging,
      Outside,
      TopSizing,
      BottomSizing,
      LeftSizing,
      TopLeftSizing,
      BottomLeftSizing,
      RightSizing,
      TopRightSizing,
      BottomRightSizing
    }

    //----------------------------------------------------------------------------
    public ClickAction CurrentAction;
    public bool LeftButtonDown = false;
    public bool RectangleDrawn = false;
    public bool ReadyToDrag = false;
    string ScreenPath;
    public Point ClickPoint = new Point();
    public Point CurrentTopLeft = new Point();
    public Point CurrentBottomRight = new Point();
    public Point DragClickRelative = new Point();
    public int RectangleHeight = new int();
    public int RectangleWidth = new int();
    public int m_InitDownX = 0;
    public int m_InitDownY = 0;
    public int m_EscaqueWidth = 0;
    public int m_EscaqueHeight = 0;
    public string m_strText = "";
    Graphics g;
    Pen MyPen = new Pen(Color.Black, 3);
    SolidBrush TransparentBrush = new SolidBrush(Color.White);
    Pen EraserPen = new Pen(Color.FromArgb(255, 192, 128), 3);
    SolidBrush eraserBrush = new SolidBrush(Color.FromArgb(255, 192, 128));

    //----------------------------------------------------------------------------
    protected override void OnMouseClick(MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        e = null;
      }
      base.OnMouseClick(e);
    }

    //----------------------------------------------------------------------------
    private MainWindow m_InstanceRef = null;
    public MainWindow InstanceRef
    {
      get
      {
        return m_InstanceRef;
      }
      set
      {
        m_InstanceRef = value;
      }
    }

    //----------------------------------------------------------------------------
    public Area()
    {
      InitializeComponent();
      this.MouseDown += new MouseEventHandler(mouse_Click);
      this.MouseDoubleClick += new MouseEventHandler(mouse_DClick);
      this.MouseUp += new MouseEventHandler(mouse_Up);
      this.MouseMove += new MouseEventHandler(mouse_Move);
      this.KeyUp += new KeyEventHandler(key_press);
      g = this.CreateGraphics();
    }

    //----------------------------------------------------------------------------
    public void SaveSelection(bool showCursor)
    {
      Point curPos = new Point(Cursor.Position.X - CurrentTopLeft.X, Cursor.Position.Y - CurrentTopLeft.Y);
      Size curSize = new Size();
      curSize.Height = Cursor.Current.Size.Height;
      curSize.Width = Cursor.Current.Size.Width;
      ScreenPath = "";
      if (!ScreenShot.saveToClipboard)
      {
        saveFileDialog1.DefaultExt = "bmp";
        saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|jpg files (*.jpg)|*.jpg|gif files (*.gif)|*.gif|tiff files (*.tiff)|*.tiff|png files (*.png)|*.png";
        saveFileDialog1.Title = "Save screenshot to...";
        saveFileDialog1.ShowDialog();
        ScreenPath = saveFileDialog1.FileName;
      }
      if (ScreenPath != "" || ScreenShot.saveToClipboard)
      {
        System.Threading.Thread.Sleep(250);
        Point StartPoint = new Point(CurrentTopLeft.X, CurrentTopLeft.Y);
        Rectangle bounds = new Rectangle(CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);
        string fi = "";
        if (ScreenPath != "")
        {
          fi = new FileInfo(ScreenPath).Extension;
        }
        ScreenShot.CaptureImage(showCursor, curSize, curPos, StartPoint, Point.Empty, bounds, ScreenPath, fi);
        if (ScreenShot.saveToClipboard)
        {
          m_InitDownX = ClickPoint.X;
          m_InitDownY = ClickPoint.Y + RectangleHeight;
          m_EscaqueWidth = RectangleWidth / 8;
          m_EscaqueHeight = RectangleHeight / 8;
          string text = string.Format("Anchura {0}, Altura {1} Escaque ({2}, {3}), inicio ({4}, {5})", RectangleWidth, RectangleHeight, RectangleWidth / 8, RectangleHeight / 8, ClickPoint.X, ClickPoint.Y);
          m_strText = text;
        }
        else
        {
          MessageBox.Show("Rectángulo grabado", "", MessageBoxButtons.OK);
        }
        this.InstanceRef.Show();
        this.Close();
      }
      else
      {
        MessageBox.Show("Grabación cancelada", "", MessageBoxButtons.OK);
        this.InstanceRef.Show();
        this.Close();
      }
    }

    //----------------------------------------------------------------------------
    public void key_press(object sender, KeyEventArgs e)
    {
      if (e.KeyCode.ToString() == "S" && (RectangleDrawn && (CursorPosition() == CursPos.WithinSelectionArea || CursorPosition() == CursPos.OutsideSelectionArea)))
      {
        SaveSelection(true);
      }
    }

    //----------------------------------------------------------------------------
    private void mouse_DClick(object sender, MouseEventArgs e)
    {
      if (RectangleDrawn && (CursorPosition() == CursPos.WithinSelectionArea || CursorPosition() == CursPos.OutsideSelectionArea))
      {
        SaveSelection(false);
      }
    }

    //----------------------------------------------------------------------------
    private void mouse_Click(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        SetClickAction();
        LeftButtonDown = true;
        ClickPoint = new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
        if (RectangleDrawn)
        {
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          DragClickRelative.X = Cursor.Position.X - CurrentTopLeft.X;
          DragClickRelative.Y = Cursor.Position.Y - CurrentTopLeft.Y;
          this.Close();
        }
      }
    }

    //----------------------------------------------------------------------------
    private void mouse_Up(object sender, MouseEventArgs e)
    {
      RectangleDrawn = true;
      LeftButtonDown = false;
      CurrentAction = ClickAction.NoClick;
    }

    //----------------------------------------------------------------------------
    private void mouse_Move(object sender, MouseEventArgs e)
    {
      if (LeftButtonDown && !RectangleDrawn)
        DrawSelection();
      if (RectangleDrawn)
      {
        CursorPosition();
        if (CurrentAction == ClickAction.Dragging)
          DragSelection();
        if (CurrentAction != ClickAction.Dragging && CurrentAction != ClickAction.Outside)
        {
          ResizeSelection();
          if (RectangleDrawn)
          {
            RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
            RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
            DragClickRelative.X = Cursor.Position.X - CurrentTopLeft.X;
            DragClickRelative.Y = Cursor.Position.Y - CurrentTopLeft.Y;
            SaveSelection(false);
            this.Close();
          }
        }
      }
    }

    //----------------------------------------------------------------------------
    private CursPos CursorPosition()
    {
      if (((Cursor.Position.X > CurrentTopLeft.X - 10 && Cursor.Position.X < CurrentTopLeft.X + 10)) && ((Cursor.Position.Y > CurrentTopLeft.Y + 10) && (Cursor.Position.Y < CurrentBottomRight.Y - 10)))
      {
        this.Cursor = Cursors.SizeWE;
        return CursPos.LeftLine;
      }
      if (((Cursor.Position.X >= CurrentTopLeft.X - 10 && Cursor.Position.X <= CurrentTopLeft.X + 10)) && ((Cursor.Position.Y >= CurrentTopLeft.Y - 10) && (Cursor.Position.Y <= CurrentTopLeft.Y + 10)))
      {
        this.Cursor = Cursors.SizeNWSE;
        return CursPos.TopLeft;
      }
      if (((Cursor.Position.X >= CurrentTopLeft.X - 10 && Cursor.Position.X <= CurrentTopLeft.X + 10)) && ((Cursor.Position.Y >= CurrentBottomRight.Y - 10) && (Cursor.Position.Y <= CurrentBottomRight.Y + 10)))
      {
        this.Cursor = Cursors.SizeNESW;
        return CursPos.BottomLeft;
      }
      if (((Cursor.Position.X > CurrentBottomRight.X - 10 && Cursor.Position.X < CurrentBottomRight.X + 10)) && ((Cursor.Position.Y > CurrentTopLeft.Y + 10) && (Cursor.Position.Y < CurrentBottomRight.Y - 10)))
      {
        this.Cursor = Cursors.SizeWE;
        return CursPos.RightLine;
      }
      if (((Cursor.Position.X >= CurrentBottomRight.X - 10 && Cursor.Position.X <= CurrentBottomRight.X + 10)) && ((Cursor.Position.Y >= CurrentTopLeft.Y - 10) && (Cursor.Position.Y <= CurrentTopLeft.Y + 10)))
      {
        this.Cursor = Cursors.SizeNESW;
        return CursPos.TopRight;
      }
      if (((Cursor.Position.X >= CurrentBottomRight.X - 10 && Cursor.Position.X <= CurrentBottomRight.X + 10)) && ((Cursor.Position.Y >= CurrentBottomRight.Y - 10) && (Cursor.Position.Y <= CurrentBottomRight.Y + 10)))
      {
        this.Cursor = Cursors.SizeNWSE;
        return CursPos.BottomRight;
      }
      if (((Cursor.Position.Y > CurrentTopLeft.Y - 10) && (Cursor.Position.Y < CurrentTopLeft.Y + 10)) && ((Cursor.Position.X > CurrentTopLeft.X + 10 && Cursor.Position.X < CurrentBottomRight.X - 10)))
      {
        this.Cursor = Cursors.SizeNS;
        return CursPos.TopLine;
      }
      if (((Cursor.Position.Y > CurrentBottomRight.Y - 10) && (Cursor.Position.Y < CurrentBottomRight.Y + 10)) && ((Cursor.Position.X > CurrentTopLeft.X + 10 && Cursor.Position.X < CurrentBottomRight.X - 10)))
      {
        this.Cursor = Cursors.SizeNS;
        return CursPos.BottomLine;
      }
      if (
          (Cursor.Position.X >= CurrentTopLeft.X + 10 && Cursor.Position.X <= CurrentBottomRight.X - 10) && (Cursor.Position.Y >= CurrentTopLeft.Y + 10 && Cursor.Position.Y <= CurrentBottomRight.Y - 10))
      {
        this.Cursor = Cursors.Hand;
        return CursPos.WithinSelectionArea;
      }
      this.Cursor = Cursors.No;
      return CursPos.OutsideSelectionArea;
    }

    //----------------------------------------------------------------------------
    private void SetClickAction()
    {
      switch (CursorPosition())
      {
        case CursPos.BottomLine:
          CurrentAction = ClickAction.BottomSizing;
          break;
        case CursPos.TopLine:
          CurrentAction = ClickAction.TopSizing;
          break;
        case CursPos.LeftLine:
          CurrentAction = ClickAction.LeftSizing;
          break;
        case CursPos.TopLeft:
          CurrentAction = ClickAction.TopLeftSizing;
          break;
        case CursPos.BottomLeft:
          CurrentAction = ClickAction.BottomLeftSizing;
          break;
        case CursPos.RightLine:
          CurrentAction = ClickAction.RightSizing;
          break;
        case CursPos.TopRight:
          CurrentAction = ClickAction.TopRightSizing;
          break;
        case CursPos.BottomRight:
          CurrentAction = ClickAction.BottomRightSizing;
          break;
        case CursPos.WithinSelectionArea:
          CurrentAction = ClickAction.Dragging;
          break;
        case CursPos.OutsideSelectionArea:
          CurrentAction = ClickAction.Outside;
          break;
      }
    }

    //----------------------------------------------------------------------------
    private void ResizeSelection()
    {
      if (CurrentAction == ClickAction.LeftSizing)
      {
        if (Cursor.Position.X < CurrentBottomRight.X - 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentTopLeft.X = Cursor.Position.X;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.TopLeftSizing)
      {
        if (Cursor.Position.X < CurrentBottomRight.X - 10 && Cursor.Position.Y < CurrentBottomRight.Y - 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentTopLeft.X = Cursor.Position.X;
          CurrentTopLeft.Y = Cursor.Position.Y;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.BottomLeftSizing)
      {
        if (Cursor.Position.X < CurrentBottomRight.X - 10 && Cursor.Position.Y > CurrentTopLeft.Y + 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentTopLeft.X = Cursor.Position.X;
          CurrentBottomRight.Y = Cursor.Position.Y;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.RightSizing)
      {
        if (Cursor.Position.X > CurrentTopLeft.X + 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentBottomRight.X = Cursor.Position.X;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.TopRightSizing)
      {
        if (Cursor.Position.X > CurrentTopLeft.X + 10 && Cursor.Position.Y < CurrentBottomRight.Y - 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentBottomRight.X = Cursor.Position.X;
          CurrentTopLeft.Y = Cursor.Position.Y;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.BottomRightSizing)
      {
        if (Cursor.Position.X > CurrentTopLeft.X + 10 && Cursor.Position.Y > CurrentTopLeft.Y + 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentBottomRight.X = Cursor.Position.X;
          CurrentBottomRight.Y = Cursor.Position.Y;
          RectangleWidth = CurrentBottomRight.X - CurrentTopLeft.X;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.TopSizing)
      {
        if (Cursor.Position.Y < CurrentBottomRight.Y - 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentTopLeft.Y = Cursor.Position.Y;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
      if (CurrentAction == ClickAction.BottomSizing)
      {
        if (Cursor.Position.Y > CurrentTopLeft.Y + 10)
        {
          g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
          CurrentBottomRight.Y = Cursor.Position.Y;
          RectangleHeight = CurrentBottomRight.Y - CurrentTopLeft.Y;
          g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
        }
      }
    }

    //----------------------------------------------------------------------------
    private void DragSelection()
    {
      g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
      if (Cursor.Position.X - DragClickRelative.X > 0 && Cursor.Position.X - DragClickRelative.X + RectangleWidth < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width)
      {
        CurrentTopLeft.X = Cursor.Position.X - DragClickRelative.X;
        CurrentBottomRight.X = CurrentTopLeft.X + RectangleWidth;
      }
      else
        if (Cursor.Position.X - DragClickRelative.X > 0)
        {
          CurrentTopLeft.X = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - RectangleWidth;
          CurrentBottomRight.X = CurrentTopLeft.X + RectangleWidth;
        }
        else
        {
          CurrentTopLeft.X = 0;
          CurrentBottomRight.X = CurrentTopLeft.X + RectangleWidth;
        }
      if (Cursor.Position.Y - DragClickRelative.Y > 0 && Cursor.Position.Y - DragClickRelative.Y + RectangleHeight < System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height)
      {
        CurrentTopLeft.Y = Cursor.Position.Y - DragClickRelative.Y;
        CurrentBottomRight.Y = CurrentTopLeft.Y + RectangleHeight;
      }
      else
        
        if (Cursor.Position.Y - DragClickRelative.Y > 0)
        {
          CurrentTopLeft.Y = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - RectangleHeight;
          CurrentBottomRight.Y = CurrentTopLeft.Y + RectangleHeight;
        }
        else
        {
          CurrentTopLeft.Y = 0;
          CurrentBottomRight.Y = CurrentTopLeft.Y + RectangleHeight;
        }
      g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, RectangleWidth, RectangleHeight);
    }

    //----------------------------------------------------------------------------
    private void DrawSelection()
    {
      this.Cursor = Cursors.Arrow;
      g.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);
      if (Cursor.Position.X < ClickPoint.X)
      {
        CurrentTopLeft.X = Cursor.Position.X;
        CurrentBottomRight.X = ClickPoint.X;
      }
      else
      {
        CurrentTopLeft.X = ClickPoint.X;
        CurrentBottomRight.X = Cursor.Position.X;
      }
      if (Cursor.Position.Y < ClickPoint.Y)
      {
        CurrentTopLeft.Y = Cursor.Position.Y;
        CurrentBottomRight.Y = ClickPoint.Y;
      }
      else
      {
        CurrentTopLeft.Y = ClickPoint.Y;
        CurrentBottomRight.Y = Cursor.Position.Y;
      }
      g.DrawRectangle(MyPen, CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);
    }
  }
}