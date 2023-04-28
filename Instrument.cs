using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace DrumMachine_Project_OOP
{
    internal class Instrument
    {
        protected WaveStream instrument;
        protected WaveOut soundPlayer;
        public Instrument(string soundfile)
        {
            instrument = new AudioFileReader(soundfile);
            soundPlayer = new WaveOut();
            soundPlayer.Init(instrument);
        }
        public virtual void Play()
        {
            if (soundPlayer.PlaybackState is PlaybackState.Playing)
                soundPlayer.Stop();
            instrument.CurrentTime = new TimeSpan(0L); //L-suffix = long //mag ook: '0' //eenheid in ticks: 10,000 ticks = 1 sec
            soundPlayer.Play();
        }
    }
}
