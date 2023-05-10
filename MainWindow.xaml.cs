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

        int msWait = 150; //100bpm = 1.67bps ~= 417ms

        Instrument _crash = new Instrument("Crash.Wav");
        Instrument _hihat = new Instrument("Hihat.wav");
        Instrument _snare = new Instrument("Snare.Wav");
        Instrument _kick = new Instrument("Kick.Wav");

        int _i = 0;
        DispatcherTimer _dispatcherTimer;

        List<Button> soundBtnList = new List<Button>();
        List<Button> antiBtnList = new List<Button>();

        public MainWindow()
        {
            InitializeComponent();

            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;

            antiBtnList.AddRange(new[]
            {
                btnChoose,
                btnLoad,
                btnPlay,
                btnStop,
                btnClear,
                btnSave,                
            });
            foreach (Button item in antiBtnList)
            {   Debug.WriteLine(item.Name);     }
        }

        private void _dispatcherTimer_Tick(object? sender, EventArgs e)
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

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{

        //}

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                txtBxFilePath.Text = openFileDialog.FileName;
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string filePath = txtBxFilePath.Text;

            try
            {
                string fileContent = File.ReadAllText(filePath);
                SaveArrays(filePath);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("The selected file does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveArrays(string filePath)
        {
            string json = File.ReadAllText(filePath);

            JArray jsonArray = JArray.Parse(json);

            // Extract the arrays
            try
            {
                crashMem = jsonArray[0]?.ToObject<int[]>() ?? throw NullArrayExc(nameof(crashMem));
                hihatMem = jsonArray[1]?.ToObject<int[]>() ?? throw NullArrayExc(nameof(hihatMem));
                snareMem = jsonArray[2]?.ToObject<int[]>() ?? throw NullArrayExc(nameof(snareMem)); 
                kickMem = jsonArray[3]?.ToObject<int[]>() ?? throw NullArrayExc(nameof(kickMem));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Invalid JSON-file, {ex.Message}", "Error in Array");
            }
            DebugNow();
        }
        private InvalidOperationException NullArrayExc(string arrayName)
        {
            return new InvalidOperationException($"Array '{arrayName}' is null.");
        }


        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            soundBtnList.Clear();

            // Find all the buttons in the visual tree except for the specific ones
            FindButtons(this, soundBtnList);

            Array.Clear(crashMem, 0, crashMem.Length);
            Array.Clear(hihatMem, 0, hihatMem.Length);
            Array.Clear(snareMem, 0, snareMem.Length);
            Array.Clear(kickMem, 0, kickMem.Length);

            foreach (Button button in soundBtnList)
            {
                button.Background = Brushes.LightGray;
            }

            for (int i = 0; i < soundBtnList.Count - 1; i += 4)
            {
                SetDarkGrayBackground(soundBtnList, i);
            }
        }

        private void FindButtons(DependencyObject parent, List<Button> soundBtnList)
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

        private void SetDarkGrayBackground(List<Button> buttonList, int index)
        {
            if (index >= 0 && index < buttonList.Count)
            {
                Button button = buttonList[index];
                button.Background = new SolidColorBrush(Color.FromRgb(0xA9, 0xA9, 0xA9));
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog instance
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "Save Arrays to JSON";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                SaveArraysToJson(new object[] { crashMem, hihatMem, snareMem, kickMem }, filename);
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

        private void DebugNow()
        {
            Debug.Write("crashMem[]: {  ");
            foreach (var item in crashMem)
            {
                Debug.Write(item.ToString());
                Debug.Write($", ");
            }
            Debug.Write("   }\n");
            Debug.Write("hihatMem[]: {  ");
            foreach (var item in hihatMem)
            {
                Debug.Write(item.ToString());
                Debug.Write($", ");
            }
            Debug.Write("   }\n");
            Debug.Write("snareMem[]: {  ");
            foreach (var item in snareMem)
            {
                Debug.Write(item.ToString());
                Debug.Write($", ");
            }
            Debug.Write("   }\n");
            Debug.Write("kickMem[]: {  ");
            foreach (var item in kickMem)
            {
                Debug.Write(item.ToString());
                Debug.Write($", ");
            }
            Debug.Write("   }\n");
        }
    }
}
