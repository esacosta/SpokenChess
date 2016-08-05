using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Speech.Synthesis;


namespace SpeechToText
{
  public partial class MainWindow : Window
  {
    SpeechRecognitionEngine speechRecognitionEngine = null;
    List<Word> words = new List<Word>();

    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    private const int MOUSEEVENTF_MOVE = 0x0001;
    private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const int MOUSEEVENTF_LEFTUP = 0x0004;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const int MOUSEEVENTF_RIGHTUP = 0x0010;
    private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

    static Area m_Area = null;

    //-----------------------------------------------------------------------------------------
    public static void Move(int xDelta, int yDelta)
    {
      mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
    }

    //-----------------------------------------------------------------------------------------
    public static void MoveTo(int x, int y)
    {
      var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
      int xA = (int)(65536.0 / screenBounds.Width * x - 1);
      int yA = (int)(65536.0 / screenBounds.Height* y - 1);
      mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, xA, yA, 0, 0);
    }

    //-----------------------------------------------------------------------------------------
    public static void LeftClick()
    {
      mouse_event(MOUSEEVENTF_LEFTDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
      mouse_event(MOUSEEVENTF_LEFTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
    }

    //-----------------------------------------------------------------------------------------
    public MainWindow()
    {
      InitializeComponent();
    }

    //--------------------------------------------------------------------------------------------
    private SpeechRecognitionEngine createSpeechEngine(string preferredCulture = "")
    {
      if (preferredCulture.Length > 0)
      {
        foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
        {
          if (config.Culture.ToString() == preferredCulture)
          {
            speechRecognitionEngine = new SpeechRecognitionEngine(config);
            break;
          }
        }
      }
      else 
      {
        speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0].Culture);
      }

      if (speechRecognitionEngine == null)
      {
        System.Windows.MessageBox.Show("El idioma no está instalado en la máquina, se continuará con el idioma por defecto: "
            + SpeechRecognitionEngine.InstalledRecognizers()[0].Culture.ToString());
        speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
      }

      return speechRecognitionEngine;
    }

    //--------------------------------------------------------------------------------------------
    private void loadGrammarAndCommands()
    {
      try
      {
        /*PromptBuilder builder = new PromptBuilder();
        builder.AppendTextWithPronunciation("A", "ah");
        builder.AppendTextWithPronunciation("B", "beh");
        builder.AppendTextWithPronunciation("C", "seh");
        builder.AppendTextWithPronunciation("D", "deh");
        builder.AppendTextWithPronunciation("E", "eh");
        builder.AppendTextWithPronunciation("F", "efeh");
        builder.AppendTextWithPronunciation("G", "heh");
        builder.AppendTextWithPronunciation("H", "ahcheh");
        */
        Choices texts = new Choices();
        string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\spanish.txt");
        foreach (string line in lines)
        {
          if (line.StartsWith("--") || line == String.Empty)
            continue;
          var parts = line.Split(new char[] { '|' });

          string attachtext = parts[1].Trim();
          if (parts[1].Contains(','))
          {
            if (m_Area != null)
            {
              string[] comas = parts[1].Split(new char[] { ',' });
              if (comas.Length == 2)
              {
                int nX = int.Parse(comas[0]);
                int nY = int.Parse(comas[1]);
                int PointX = m_Area.m_InitDownX + m_Area.m_EscaqueWidth * nX  + (m_Area.m_EscaqueWidth / 2);
                int PointY = m_Area.m_InitDownY - m_Area.m_EscaqueHeight * nY - (m_Area.m_EscaqueHeight / 2);

                string click = string.Format("{0},{1}", PointX, PointY);
                attachtext = click;

              }
            }
          }

          words.Add(new Word() { Text = parts[0].Trim(), AttachedText = attachtext, IsShellCommand = (parts[2] == "true") });
          texts.Add(parts[0].Trim());
        }

        Grammar wordsList = new Grammar(new GrammarBuilder(texts));
        speechRecognitionEngine.LoadGrammar(wordsList);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    //--------------------------------------------------------------------------------------------
    private string getKnownTextOrExecute(string command)
    {
      try
      {
        var cmd = words.Where(c => c.Text == command).First();
        
        if (cmd.IsShellCommand)
        {
          var parts = cmd.AttachedText.Split(new char[] { ',' });
          int nX = int.Parse(parts[0]);
          int nY = int.Parse(parts[1]);

          string strRet = string.Format("Ha dicho: {0}", cmd.Text);

          /*SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
          _synthesizer.SetOutputToDefaultAudioDevice();
          _synthesizer.Speak(strRet);
          */
          MoveTo(nX, nY);
          LeftClick();
          
          m_Area.SaveSelection(false);

          if (System.Windows.Forms.Clipboard.ContainsImage())
          {
            imgClipboard.Source = System.Windows.Clipboard.GetImage();
          }
          
          return strRet;
        }
        else
        {
          SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
          _synthesizer.SetOutputToDefaultAudioDevice();
          _synthesizer.Speak(cmd.Text);
          return cmd.AttachedText;
        }
      }
      catch (Exception)
      {
        return command;
      }
    }
    
    //--------------------------------------------------------------------------------------------
    void engine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
    {
      //txtSpoken.Text += "\r Hipotesis" + getKnownTextOrExecute(e.Result.Text);
      //scvText.ScrollToEnd();
    }

    //--------------------------------------------------------------------------------------------
    void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
      txtSpoken.Text += "\r" + getKnownTextOrExecute(e.Result.Text);
      scvText.ScrollToEnd();
    }

    //--------------------------------------------------------------------------------------------
    void engine_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
    {
      prgLevel.Value = e.AudioLevel;
    }

    //--------------------------------------------------------------------------------------------
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (speechRecognitionEngine != null)
      {
        speechRecognitionEngine.RecognizeAsyncStop();
        speechRecognitionEngine.Dispose();
      }
    }
    
    //--------------------------------------------------------------------------------------------
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    //--------------------------------------------------------------------------------------------
    private void SelectArea(object sender, RoutedEventArgs e)
    {
      m_Area = new Area();
      m_Area.InstanceRef = this;
      m_Area.ShowDialog();

      txtSpoken.Text += "\r" + m_Area.m_strText;
      scvText.ScrollToEnd();

      if (System.Windows.Forms.Clipboard.ContainsImage())
      {
        imgClipboard.Source = System.Windows.Clipboard.GetImage();
      }

      try
      {
        speechRecognitionEngine = createSpeechEngine(""); //es-ES
        speechRecognitionEngine.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(engine_AudioLevelUpdated);
        speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);
        speechRecognitionEngine.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(engine_SpeechHypothesized);
        speechRecognitionEngine.SetInputToDefaultAudioDevice();

        loadGrammarAndCommands();

        speechRecognitionEngine.BabbleTimeout = TimeSpan.FromSeconds(10.0);
        speechRecognitionEngine.EndSilenceTimeout = TimeSpan.FromSeconds(10.0);
        speechRecognitionEngine.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(10.0);
        speechRecognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(10.0);

        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(ex.Message, "Error con el componente de voz");
      }

      btnArea.IsEnabled = false;
    }
  }
}
