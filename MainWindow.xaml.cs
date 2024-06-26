﻿using System;
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
using System.Windows.Threading;
using Microsoft.Win32;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace DrumMachine_Project_OOP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[] crashMem = new int[64];
        int[] hihatMem = new int[64];
        int[] snareMem = new int[64];
        int[] kickMem = new int[64];
        int[] tomMem = new int[64];
        
        Crash _crash = new Crash();
        Hihat _hihat = new Hihat();
        Snare _snare = new Snare();
        Kick _kick = new Kick();
        Tom _tom = new Tom();
        const int _amountInstruments = 5;

        int _i = 0;
        DispatcherTimer _dispatcherTimer;
        int bpm = 100;
        int msWait = 150; //150ms tussen elke noot

        const int totalPolygonCount = 64;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((crashMem != null && !crashMem.All(val => val == 0)) ||
                (hihatMem != null && !hihatMem.All(val => val == 0)) ||
                (snareMem != null && !snareMem.All(val => val == 0)) ||
                (kickMem != null && !kickMem.All(val => val == 0)) ||
                (tomMem != null && !tomMem.All(val => val == 0)))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the existing groove?", "Delete groove?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Close Event canceled\n");
                    e.Cancel = true;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HideAllPolygons();

            _crash.LoadSoundsToComboBox(cmbBoxCrash);
            _hihat.LoadSoundsToComboBox(cmbBoxHihat);
            _snare.LoadSoundsToComboBox(cmbBoxSnare);
            _kick.LoadSoundsToComboBox(cmbBoxKick);
            _tom.LoadSoundsToComboBox(cmbBoxTom);


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

        private void HideAllPolygons()          //Verbergt alle driehoeken
        {
            for (int i = 1; i <= totalPolygonCount; i++)
            {
                string polygonName = "pg" + i;
                Polygon? polygon = FindName(polygonName) as Polygon;
                if (polygon != null)
                {
                    polygon.Visibility = Visibility.Collapsed;
                }
            }
            Debug.WriteLine("Alle driehoeken verborgen\n");
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

            if (tomMem[_i] == 1)
                _tom.Play();

            HidePolygon(_i);
            ShowPolygon(_i);

            _i++;

            if (_i == crashMem.Length)
                _i = 0;
            if (_i == 1)
                pg64.Visibility = Visibility.Collapsed;
        }

        private void HidePolygon(int index)         //Verberg driehoek
        {
            string polygonName = "pg" + (index);
            Polygon? polygon = FindName(polygonName) as Polygon;
            if (polygon != null)
            {
                polygon.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowPolygon(int index)         //Toon driehoek
        {
            string polygonName = "pg" + (index + 1);
            Polygon? polygon = FindName(polygonName) as Polygon;
            if (polygon != null)
            {
                polygon.Visibility = Visibility.Visible;
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
            if ((crashMem != null && !crashMem.All(val => val == 0)) ||
                (hihatMem != null && !hihatMem.All(val => val == 0)) ||
                (snareMem != null && !snareMem.All(val => val == 0)) ||
                (kickMem != null && !kickMem.All(val => val == 0)) ||
                (tomMem != null && !tomMem.All(val => val == 0)))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the existing groove?", "Delete groove?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
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
            catch (FileNotFoundException)
            {
                MessageBox.Show("The selected file does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading and processing the file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                ValidateArray(jsonArray[4], "tomMem");

                crashMem = jsonArray[0]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'crashMem' is not in the right format.");
                hihatMem = jsonArray[1]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'hihatMem' is not in the right format.");
                snareMem = jsonArray[2]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'snareMem' is not in the right format.");
                kickMem = jsonArray[3]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'kickMem' is not in the right format.");
                tomMem = jsonArray[4]?.ToObject<int[]>() ?? throw new InvalidOperationException("Array 'tomMem' is not in the right format.");
                DebugArrays();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON file :{ex.Message}", "Error");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Incorrect Format", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during array validation :{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ValidateArray(JToken token, string arrayName)
        {
            try
            {
                if (token == null || token.Type != JTokenType.Array || token.Count() != 64 ||                                                                   
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
            SoundBtnOriginalColor();

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
                    Button button = soundBtnList[crashMem.Length + i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }
            for (int i = 0; i < snareMem.Length; i++)
            {
                if (snareMem[i] == 1)
                {
                    Button button = soundBtnList[crashMem.Length * 2 + i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }            
            for (int i = 0; i < tomMem.Length; i++)
            {
                if (tomMem[i] == 1)
                {
                    Button button = soundBtnList[crashMem.Length * 3 + i];
                    button.Background = Brushes.LightSkyBlue;
                }
            }
            for (int i = 0; i < kickMem.Length; i++)
            {
                if (kickMem[i] == 1)
                {
                    Button button = soundBtnList[crashMem.Length * 4 + i];
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
            HideAllPolygons();
            StopSounds();
            Debug.WriteLine("btnStop clicked\n");
        }
        private void StopSounds()
        {
            _crash.Stop();
            _hihat.Stop();
            _snare.Stop();
            _kick.Stop();
            _tom.Stop();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)       //Verwijder inhoud array's + knoppen naar originele kleur
        {
            Debug.WriteLine("btnClear clicked");
            _dispatcherTimer.Stop();
            _i = 0;
            HideAllPolygons();
            StopSounds();

            if ((crashMem != null && !crashMem.All(val => val == 0)) ||
                (hihatMem != null && !hihatMem.All(val => val == 0)) ||
                (snareMem != null && !snareMem.All(val => val == 0)) ||
                (kickMem != null && !kickMem.All(val => val == 0)) ||
                (tomMem != null && !tomMem.All(val => val == 0)))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete existing groove?", "Delete groove?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Load Event canceled\n");
                    return;
                }
            }

            if (crashMem != null)
                Array.Clear(crashMem, 0, crashMem.Length);
            if (hihatMem != null)
                Array.Clear(hihatMem, 0, hihatMem.Length);
            if (snareMem != null)
                Array.Clear(snareMem, 0, snareMem.Length);
            if (kickMem != null)
                Array.Clear(kickMem, 0, kickMem.Length);
            if (tomMem != null)
                Array.Clear(tomMem, 0, tomMem.Length);
            Debug.WriteLine("Arrays cleared");
            SoundBtnOriginalColor();          
            Debug.WriteLine("Button colors restored\n");
        }
        
        private void SoundBtnOriginalColor()
        {
            foreach (Button button in soundBtnList)
            {
                button.Background = Brushes.LightGray;
            }

            for (int i = 0; i < soundBtnList.Count; i += 4)
            {
                if (i >= 0 && i < soundBtnList.Count)
                {
                    Button button = soundBtnList[i];
                    button.Background = new SolidColorBrush(Color.FromRgb(0xA9, 0xA9, 0xA9));
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)        //Zet array's om in Json-file + opent venster om locatie te kiezen
        {
            Debug.WriteLine("btnSave clicked");
            if ((crashMem == null || crashMem.All(val => val == 0)) &&
                (hihatMem == null || hihatMem.All(val => val == 0)) &&
                (snareMem == null || snareMem.All(val => val == 0)) &&
                (kickMem == null || kickMem.All(val => val == 0)) &&
                (tomMem == null || tomMem.All(val => val == 0)))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to save a empty groove?", "Empty groove", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    Debug.WriteLine("Save Event canceled\n");
                    return;
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "Save Arrays to JSON";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                SaveArraysToJson(new object[] { crashMem!, hihatMem!, snareMem!, kickMem!, tomMem! }, filename);
                Debug.WriteLine("Arrays succesfully saved\n");
                MessageBox.Show("Groove successfully saved!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        static void SaveArraysToJson(object[] arrays, string filename)          //Zet Json naar arrays
        {
            string json = JsonSerializer.Serialize(arrays, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filename, json);
            Console.WriteLine("JSON data saved to file: " + filename);
        }

        private void txtBxTempo_PreviewTextInput(object sender, TextCompositionEventArgs e)     //Enkel getallen in textbox
        {
            if (!int.TryParse(e.Text, out int numericValue))
            {
                e.Handled = true;
            }
        }

        private void txtBxTempo_TextChanged(object sender, TextChangedEventArgs e)          //berekent msWait + verandert interval Timer
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

        private void btnTempoUp_Click(object sender, RoutedEventArgs e)     //Verhoogt bpm met 1
        {
            try
            {
                bpm++;
                FormulaMsWait(bpm);
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                txtBxTempo.Text = bpm.ToString();
            }
            catch (Exception ex)
            {
                bpm = 1;
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                txtBxTempo.Text = bpm.ToString();
                MessageBox.Show("Bpm is no valid value: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTempoDown_Click(object sender, RoutedEventArgs e)       //Verlaagt bpm met 1
        {
            try
            {
                bpm--;
                FormulaMsWait(bpm);
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                txtBxTempo.Text = bpm.ToString();
            }
            catch (DivideByZeroException)
            {
                bpm = 1;
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                txtBxTempo.Text = bpm.ToString();
                MessageBox.Show("Bpm can't be '0': ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);               
            }
            catch (Exception ex)
            {
                bpm = 1;
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(msWait);
                txtBxTempo.Text = bpm.ToString();
                MessageBox.Show("Bpm is no valid value: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FormulaMsWait(int bpm)         //Berekent msWait via bpm
        {
            try
            {
                double msWaitDouble = (60 * 1000) / (bpm * 4);
                msWait = (int)Math.Round(msWaitDouble);
                //100bpm --> 400noten/min --> 6.67noten/s
                //t = (1/6.67)*1000 ms
            }
            catch (DivideByZeroException)
            {
                throw new DivideByZeroException();
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
        

        //Change instrument
        private void cmbBoxCrash_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxCrash.SelectedItem != null)
            {
                string? selectedSoundFileName = cmbBoxCrash.SelectedItem.ToString();
                string selectedSoundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Crash", selectedSoundFileName + ".wav");
                _crash.SoundFile = selectedSoundFilePath;
            }
        }

        private void cmbBoxHihat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxHihat.SelectedItem != null)
            {
                string? selectedSoundFileName = cmbBoxHihat.SelectedItem.ToString();
                string selectedSoundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Hihat", selectedSoundFileName + ".wav");
                _hihat.SoundFile = selectedSoundFilePath;
            }
        }

        private void cmbBoxSnare_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxSnare.SelectedItem != null)
            {
                string? selectedSoundFileName = cmbBoxSnare.SelectedItem.ToString();
                string selectedSoundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Snare", selectedSoundFileName + ".wav");
                _snare.SoundFile = selectedSoundFilePath;
            }
        }

        private void cmbBoxKick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxKick.SelectedItem != null)
            {
                string? selectedSoundFileName = cmbBoxKick.SelectedItem.ToString();
                string selectedSoundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Kick", selectedSoundFileName + ".wav");
                _kick.SoundFile = selectedSoundFilePath;
            }
        }

        private void cmbBoxTom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxTom.SelectedItem != null)
            {
                string? selectedSoundFileName = cmbBoxTom.SelectedItem.ToString();
                string selectedSoundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Tom", selectedSoundFileName + ".wav");
                _tom.SoundFile = selectedSoundFilePath;
            }
        }


        //SoundBtns --> array + achtergroundkleur van knoppen
        private void btnCrash_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(8)) - 1;    // haalt index uit button name

            if (crashMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                crashMem[index] = 1;
            }
            else
            {
                if (index == 0 || index%4 == 0)
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
            int index = int.Parse(button.Name.Substring(8)) - 1;    // haalt index uit button name

            if (hihatMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                hihatMem[index] = 1;
            }
            else
            {
                if (index == 0 || index % 4 == 0)
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
            int index = int.Parse(button.Name.Substring(8)) - 1;    // haalt index uit button name

            if (snareMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                snareMem[index] = 1;
            }
            else
            {
                if (index == 0 || index % 4 == 0)
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
            int index = int.Parse(button.Name.Substring(7)) - 1;    // haalt index uit button name //is '7', want kick is 4 letters, al de rest 5

            if (kickMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                kickMem[index] = 1;
            }
            else
            {
                if (index == 0 || index % 4 == 0)
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                kickMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + kick[index].ToString() + "]");
        }

        private void btnTom_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = int.Parse(button.Name.Substring(6)) - 1;    //haalt index uit button name //is '6', want tom is 3 letters, al de rest 5

            if (tomMem[index] == 0)
            {
                button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                tomMem[index] = 1;
            }
            else
            {
                if (index == 0 || index % 4 == 0)
                    button.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                else
                    button.Background = new SolidColorBrush(Colors.LightGray);
                tomMem[index] = 0;
            }
            //Debug.WriteLine(index + "[" + tom[index].ToString() + "]");
        }

        private void DebugArrays()      //Schrijft alle array-values naar debugger
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
            counter = 0;
            Debug.Write("tomMem[]: {  ");
            foreach (var item in tomMem)
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