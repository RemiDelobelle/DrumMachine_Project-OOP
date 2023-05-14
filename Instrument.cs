using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;

namespace DrumMachine_Project_OOP
{
    internal class Instrument
    {
        protected WaveStream instrument;
        protected WaveOut soundPlayer;
        public Instrument()
        {
            instrument = null;
            soundPlayer = new WaveOut();
        }

        public virtual void Play()
        {
            if (soundPlayer.PlaybackState is PlaybackState.Playing)
                soundPlayer.Stop();
            instrument.CurrentTime = new TimeSpan(0L); //L-suffix = long //mag ook: '0' //eenheid in ticks: 10,000 ticks = 1 sec
            soundPlayer.Play();
        }

        public virtual void Stop()
        {
            if (soundPlayer.PlaybackState is PlaybackState.Playing)
                soundPlayer.Stop();
        }
    }
}
