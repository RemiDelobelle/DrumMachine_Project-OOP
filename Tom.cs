﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DrumMachine_Project_OOP
{
    internal class Tom : Instrument
    {
        private string soundFile;
        public string SoundFile
        {
            get { return soundFile; }
            set
            {
                soundFile = value;
                SetSoundFile(soundFile);
            }
        }

        public Tom()
        {
        }

        private void SetSoundFile(string soundfile)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundFilePath = Path.Combine(baseDirectory, "Sounds", "Tom", soundfile);

            try
            {
                instrument = new AudioFileReader(soundFilePath);
                soundPlayer.Init(instrument);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Tom-Sound file not found: " + soundFilePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the sound file:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadSoundsToComboBox(ComboBox comboBox)
        {
            string soundsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Tom");
            string[] wavFiles = Directory.GetFiles(soundsFolderPath, "*.wav");
            foreach (string filePath in wavFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                comboBox.Items.Add(fileName);
            }
            comboBox.SelectedItem = "Tom_Low";
        }
    }
}