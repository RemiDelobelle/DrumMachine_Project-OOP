using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NAudio.SoundFont;
using NAudio.Wave;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Media;
using System.Linq;
//using DrumMachine;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
//using Newtonsoft.Json;

namespace DrumMachine_Project_OOP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[] crashMem = new int[16];
        int[] hihatMem = new int[16];
        int[] snareMem = new int[16];
        int[] kickMem = new int[16];

        Instrument _crash = new Instrument("Crash.Wav");
        Instrument _hihat = new Instrument("Hihat.wav");
        Instrument _snare = new Instrument("Snare.Wav");
        Instrument _kick = new Instrument("Kick.Wav");
        const int _amountInstruments = 4;

        int _i = 0;
        DispatcherTimer _dispatcherTimer;
        int bpm = 100;
        int msWait = 150; //150ms tussen elke noot

        List<Button> soundBtnList = new List<Button>(); //Soudnbtns list
        List<Button> antiBtnList = new List<Button>();  //De rest

        public MainWindow()
        {
            InitializeComponent();

            FormulaMsWait(bpm);
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
        }

        private void _dispatcherTimer_Tick(object? sender, EventArgs e)     //loop sounds
        {
            if (crashMem[_i] == 1)
                _crash.Play();

            if (hihatMem[_i] == 1)
                _hihat.Play();

            if (snareMem[_i] == 1)
                _snare.Play();

            if (kickMem[_i] == 1)
                _kick.Play();
             
            _i++;
            if(_i == 16)
                _i = 0;   
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            antiBtnList.AddRange(new[]
            {
                btnChoose,
                btnLoad,
                btnPlay,
                btnStop,
                btnClear,
                btnSave,
                btnTempoUp,
                btnTempoDown
            });
            Debug.WriteLine("Buttons in antiBtnList:");
            foreach (Button item in antiBtnList)
            {   Debug.WriteLine(item.Name);    }

            FindButtons(this, soundBtnList);
            Debug.WriteLine("\nButtons in soundBtnList:");
            foreach (Button button in soundBtnList)
            {   Debug.WriteLine(button.Name);    }
            Debug.WriteLine("");
            DebugArrays();
        }
        private void FindButtons(DependencyObject parent, List<Button> soundBtnList)    //Add all soundBtns aan de list
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                //if (child is Button button && button.Name != "btnPlay" && button.Name != "btnStop" && button.Name != "btnClear")
                if (child is Button button && !antiBtnList.Contains(button))
                {
                    soundBtnList.Add(button);
                }
                else
                {
                    FindButtons(child, soundBtnList);
                }
            }
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)     //File kiezen om in te laden
        {
            Debug.WriteLine("btnChoose clicked");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                txtBxFilePath.Text = openFileDialog.FileName;
                Debug.WriteLine("FilePath success\n");
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)       //File inladen + Json omzetten naar array's
        {
            Debug.WriteLine("btnLoad clicked");
            if (crashMem != null || hihatMem != null || snareMem != null || kickMem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete existing groove?", "Delete groove?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Load Event canceled\n");
                    return;                    
                }
            }

            string filePath = txtBxFilePath.Text;

            try
            {
                string fileContent = File.ReadAllText(filePath);
                ConvertJsonToArrays(filePath);
                VisualizerSoundBtns();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("The selected file does not exist." + ex.Message, "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertJsonToArrays(string filePath)                        //JSON lezen omzetten naar arrays
        {
            string json = File.ReadAllText(filePath);
            Debug.WriteLine("Json converted to string");

            try
            {
                JArray jsonArray = JArray.Parse(json);

                if (jsonArray.Count < _amountInstruments)                                                                                                       //Verander var als uitbreiden
                {
                    MessageBox.Show("Invalid JSON file: Missing arrays", "Error");
                    return;
                }

                ValidateArray(jsonArray[0], "crashMem");
                ValidateArray(jsonArray[1], "hihatMem");
                ValidateArray(jsonArray[2], "snareMem");
                ValidateArray(jsonArray[3], "kickMem");

                crashMem = jsonArray[0]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'crashMem' is not in the right format.");
                hihatMem = jsonArray[1]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'hihatMem' is not in the right format.");
                snareMem = jsonArray[2]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'snareMem' is not in the right format.");
                kickMem = jsonArray[3]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'kickMem' is not in the right format.");
                DebugArrays();
                // Proceed with further processing or operations on the arrays
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON file: {ex.Message}", "Error");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Incorrect Format", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during array validation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ValidateArray(JToken token, string arrayName)
        {
            try
            {
                if (token == null || token.Type != JTokenType.Array || token.Count() != 16 ||
                    !token.Values().All(val => val.Type == JTokenType.Integer && (int)val == 0 || (int)val == 1))
                {
                    throw new InvalidOperationException($"Array '{arrayName}' is not in the right format.");
                }
                Debug.WriteLine("Arrays Validated");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error in validating array '{arrayName}': {ex.Message}");
            }
        }

        private void VisualizerSoundBtns()
        {
            for (int i = 0; i < crashMem.Length; i++)
            {
                if(crashMem[i] == 1)
                {
                    Button button = soundBtnList[i];
                    button.Background = Brushes.LightSkyBlue;                    
                }
            }
            for (int i = 0; i < hihatMem.Length; i++)
            {
                if (hihatMem[i] == 1)
                {
                    Button button = soundBtnList[i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }
            for (int i = 0; i < snareMem.Length; i++)
            {
                if (snareMem[i] == 1)
                {
                    Button button = soundBtnList[i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }
            for (int i = 0; i < kickMem.Length; i++)
            {
                if (kickMem[i] == 1)
                {
                    Button button = soundBtnList[i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }
            Debug.WriteLine("Buttons correctly colored\n");
        }


        private void btnPlay_Click(object sender, RoutedEventArgs e)        //Start loop
        {
            _dispatcherTimer.Start();
            Debug.WriteLine("btnPlay clicked\n");
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)        //Stop loop
        {
            _dispatcherTimer.Stop();
            _i = 0;
            Debug.WriteLine("btnStop clicked\n");
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)       //Verwijder inhoud array's + knoppen naar originele kleur
        {
            Debug.WriteLine("btnClear clicked");
            _dispatcherTimer.Stop();
            _i = 0;

            if (crashMem != null || hihatMem != null || snareMem != null || kickMem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete existing groove?", "Delete groove?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Load Event canceled\n");
                    return;
                }
            }

            // Find all the buttons in the visual tree except for the specific ones
            //FindButtons(this, soundBtnList);
            if (crashMem != null)
                Array.Clear(crashMem, 0, crashMem.Length);
            if (hihatMem != null)
                Array.Clear(hihatMem, 0, hihatMem.Length);
            if (snareMem != null)
                Array.Clear(snareMem, 0, snareMem.Length);
            if (kickMem != null)
                Array.Clear(kickMem, 0, kickMem.Length);
            Debug.WriteLine("Arrays cleared");

            foreach (Button button in soundBtnList)
            {
                button.Background = Brushes.LightGray;
            }

            for (int i = 0; i < soundBtnList.Count /*- 1*/; i += 4)
            {
                if (i >= 0 && i < soundBtnList.Count)
                {
                    Button button = soundBtnList[i];
                    button.Background = new SolidColorBrush(Color.FromRgb(0xA9, 0xA9, 0xA9));
                }
            }
            Debug.WriteLine("Button colors restored\n");
        }       


        private void btnSave_Click(object sender, RoutedEventArgs e)        //Zet array's om in Json-file + opent venster om locatie te kiezen
        {
            Debug.WriteLine("btnSave clicked");
            if (crashMem == null || hihatMem == null || snareMem == null || kickMem == null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to save a empty groove?", "Empty groove", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Save Event canceled\n");
                    return;
                }
            }

            // Create a SaveFileDialog instance
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "Save Arrays to JSON";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                SaveArraysToJson(new object[] { crashMem!, hihatMem!, snareMem!, kickMem! }, filename);
                Debug.WriteLine("Arrays succesfully saved\n");
            }
        }

        static void SaveArraysToJson(object[] arrays, string filename)
        {
            // Convert the arrays to JSON format
            string json = JsonSerializer.Serialize(arrays, new JsonSerializerOptions { WriteIndented = true });

            // Save the JSON to a file
            File.WriteAllText(filename, json);
            Console.WriteLine("JSON data saved to file: " + filename);
        }

        private void txtBxTempo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int numericValue))
            {
                e.Handled = true; // Prevent the invalid text from being entered
            }
        }

        private void txtBxTempo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtBxTempo.Text, out int newValue))
            {
                bpm = newValue;
                FormulaMsWait(bpm);
                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                }
            }
        }

        private void btnTempoUp_Click(object sender, RoutedEventArgs e)
        {
            bpm++;
            FormulaMsWait(bpm);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
            txtBxTempo.Text = bpm.ToString();
        }

        private void btnTempoDown_Click(object sender, RoutedEventArgs e)
        {
            bpm--;
            FormulaMsWait(bpm);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
            txtBxTempo.Text = bpm.ToString();
        }

        private void FormulaMsWait(int bpm)
        {
            double msWaitDouble = (60 * 1000) / (bpm * 4);
            msWait = (int)Math.Round(msWaitDouble);
            //100bpm --> 400noten/min --> 6.67noten/s
            //t = (1/6.67)*1000 ms
        }


        //SoundBtns --> array + achtergroundkleur van knoppen
        private void btnCrash_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(8)) - 1; // extract index from button name

            if (crashMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                crashMem[index] = 1;
            }
            else
            {
                if ((index == 0) || (index == 4) || (index == 8) || (index == 12))
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                crashMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + crash[index].ToString() + "]");
        }

        private void btnHihat_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(8)) - 1; // extract index from button name

            if (hihatMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                hihatMem[index] = 1;
            }
            else
            {
                if ((index == 0) || (index == 4) || (index == 8) || (index == 12))
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                hihatMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + hihat[index].ToString() + "]");
        }

        private void btnSnare_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(8)) - 1; // extract index from button name

            if (snareMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                snareMem[index] = 1;
            }
            else
            {
                if ((index == 0) || (index == 4) || (index == 8) || (index == 12))
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                snareMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + snare[index].ToString() + "]");
        }

        private void btnKick_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(7)) - 1; // extract index from button name //is '7', want kick is 4 letters, al de rest 5

            if (kickMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                kickMem[index] = 1;
            }
            else
            {
                if ((index == 0) || (index == 4) || (index == 8) || (index == 12))
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                kickMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + kick[index].ToString() + "]");
        }


        private void DebugArrays()
        {
            Debug.WriteLine("");

            Debug.Write("crashMem[]: {  ");
            int lastIndex = crashMem.Length - 1;
            int counter = 0;
            foreach (var item in crashMem)
            {
                Debug.Write(item.ToString());
                if (counter != lastIndex)
                { Debug.Write(", ");    }
                counter++;
            }
            Debug.Write("   }\n");
            counter = 0;
            Debug.Write("hihatMem[]: {  ");
            foreach (var item in hihatMem)
            {
                Debug.Write(item.ToString());
                if (counter != lastIndex)
                { Debug.Write(", "); }
                counter++;
            }
            Debug.Write("   }\n");
            counter = 0;
            Debug.Write("snareMem[]: {  ");
            foreach (var item in snareMem)
            {
                Debug.Write(item.ToString());
                if (counter != lastIndex)
                { Debug.Write(", "); }
                counter++;
            }
            Debug.Write("   }\n");
            counter = 0;
            Debug.Write("kickMem[]: {  ");
            foreach (var item in kickMem)
            {
                Debug.Write(item.ToString());
                if (counter != lastIndex)
                { Debug.Write(", "); }
                counter++;
            }
            Debug.Write("   }\n");
        }
    }
}
