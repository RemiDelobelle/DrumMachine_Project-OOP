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
using DrumMachine;

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

        int msWait = 417; //100bpm = 1.67bps ~= 417ms
        bool loopActive = false;
        Timer timerLoop = new Timer();

        Instrument _crash = new Instrument("Crash.Wav");
        Instrument _hihat = new Instrument("Hihat.wav");
        Instrument _snare = new Instrument("Snare.Wav");
        Instrument _kick = new Instrument("Kick.Wav");

        MultimediaTimer timer = new MultimediaTimer();
        public MainWindow()
        {
            InitializeComponent();

            timerLoop.Interval = msWait;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            ////BELANGRIJK
            ////Bij Timer: gebruik 'Priority.Normal' voor hogere precisie
            ////BELANGRIJK
            //int i = 0;
            //Stopwatch stopwatch = new Stopwatch(); // use higher precision timer
            //Timer timer = new Timer(msWait);
            //timer.Elapsed += (sender, e) =>
            //{
            //    while (true) // repeat infinitely
            //    {
            //        stopwatch.Start(); // start the timer
            //        if (i < crashMem.Length)
            //        {
            //            if (crashMem[i] == 1)
            //            {
            //                Thread t = new Thread(() => _crash.Play());
            //                t.Start();
            //            }
            //            if (hihatMem[i] == 1)
            //            {
            //                Thread t = new Thread(() => _hihat.Play());
            //                t.Start();
            //            }
            //            if (i < snareMem.Length && snareMem[i] == 1)
            //            {
            //                Thread t = new Thread(() => _snare.Play());
            //                t.Start();
            //            }
            //            if (kickMem[i] == 1)
            //            {
            //                Thread t = new Thread(() => _kick.Play());
            //                t.Start();
            //            }
            //            i++;
            //        }
            //        else
            //        {
            //            i = 0;
            //        }
            //        stopwatch.Stop(); // stop the timer
            //        int elapsedMs = (int)stopwatch.Elapsed.TotalMilliseconds; // calculate elapsed time
            //        int remainingMs = msWait - elapsedMs; // calculate remaining time
            //        if (remainingMs > 0)
            //        {
            //            Thread.Sleep(remainingMs); // wait for the remaining time
            //        }
            //        stopwatch.Reset(); // reset the timer
            //    }
            //};
            //timer.Start();

            //loopActive = true;
            //int i = 0;
            //Stopwatch stopwatch = new Stopwatch();
            //MultimediaTimer timer = new MultimediaTimer();
            //timer.Interval = msWait;
            //timer.Start();
            //while (loopActive)
            //{
            //    timer.Elapsed += (sender, e) =>
            //    {
            //        if (crashMem[i] == 1)
            //        {
            //            Task.Run(() => _crash.Play());
            //        }
            //        if (hihatMem[i] == 1)
            //        {
            //            Task.Run(() => _hihat.Play());
            //        }
            //        if (i < snareMem.Length && snareMem[i] == 1)
            //        {
            //            Task.Run(() => _snare.Play());
            //        }
            //        if (kickMem[i] == 1)
            //        {
            //            Task.Run(() => _kick.Play());
            //        }
            //        i++;
            //        if (i >= crashMem.Length)
            //            i = 0;
            //    };
            //}
            //timer.Stop();



            loopActive = true;
            int i = 0;
            Stopwatch stopwatch = new Stopwatch();
            timer.Interval = msWait;
            timer.Start();
            timer.Elapsed += (sender, e) =>
            {
                stopwatch.Start();
                if (i < crashMem.Length)
                {
                    Parallel.ForEach(Enumerable.Range(0, 4), (index) =>
                    {
                        if (crashMem[i] == 1 && index == 0)
                        {
                            _crash.Play();
                        }
                        if (hihatMem[i] == 1 && index == 1)
                        {
                            _hihat.Play();
                        }
                        if (snareMem[i] == 1 && index == 2)
                        {
                            _snare.Play();
                        }
                        if (kickMem[i] == 1 && index == 3)
                        {
                            _kick.Play();
                        }
                    });
                    i++;
                }
                else
                {
                    i = 0;
                }
                stopwatch.Stop();
                int elapsedMs = (int)stopwatch.Elapsed.TotalMilliseconds;
                int remainingMs = msWait - elapsedMs;
                if (remainingMs > 0)
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    while (s.ElapsedMilliseconds < remainingMs)
                    { }
                    s.Reset();
                }
                stopwatch.Reset();
            };
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            
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
            int index = int.Parse(button.Name.Substring(7)) - 1; // extract index from button name

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
    }
}
