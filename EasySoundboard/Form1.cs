using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySoundboard
{
    public partial class Form1 : Form
    {
        private SourceMixer mixer;
        private WasapiCapture soundIn;
        private WasapiOut soundOut;
        private WasapiOut monitoringOut;

        private SourceCache cache = new SourceCache();

        public Form1()
        {
            InitializeComponent();
            ReloadDevices();
            ReloadSounds();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbInputDev.SelectedItem != null && cbOutputDev.SelectedItem != null)
            {
                cbInputDev.Enabled = cbOutputDev.Enabled = button1.Enabled = button2.Enabled = checkBox1.Enabled = false;

                soundIn = new WasapiCapture(true, AudioClientShareMode.Shared, 30);
                soundIn.Device = (MMDevice)cbInputDev.SelectedItem;
                soundIn.Initialize();
                soundIn.Start();

                mixer = new SourceMixer(soundIn.WaveFormat.Channels, soundIn.WaveFormat.SampleRate);

                var waveSource = new SoundInSource(soundIn) { FillWithZeros = true };
                mixer.AddSource(waveSource.ToSampleSource());

                var mixedSource = mixer.ToWaveSource();

                soundOut = new WasapiOut();
                soundOut.Device = (MMDevice)cbOutputDev.SelectedItem;
                soundOut.Initialize(mixedSource);
                soundOut.Play();

                if (checkBox1.Checked)
                {
                    var monitorSource = new SoundInSource(soundIn) { FillWithZeros = true };
                    monitoringOut = new WasapiOut();
                    monitoringOut.Initialize(monitorSource);
                    monitoringOut.Play();
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReloadDevices();
        }

        private void ReloadDevices()
        {
            cbInputDev.Items.Clear();
            cbOutputDev.Items.Clear();
            foreach (var device in MMDeviceEnumerator.EnumerateDevices(DataFlow.All))
            {
                if (device.DeviceState != DeviceState.Active)
                    continue;

                if (device.DataFlow == DataFlow.Capture && !cbInputDev.Items.Contains(device))
                    cbInputDev.Items.Add(device);
                else if (!cbOutputDev.Items.Contains(device))
                    cbOutputDev.Items.Add(device);
            }
        }

        private void PlayInStream(string path)
        {
            if (mixer == null)
                return;

            var source = cache.Load(path, soundIn.WaveFormat.SampleRate, trackBar1.Value);
            if (source == null)
                return;
            mixer.AddSource(source);
        }

        private void ReloadSounds()
        {
            flowLayoutPanel1.Controls.Clear();
            foreach (var soundFile in Directory.EnumerateFiles("./sounds/", "*.mp3"))
            {
                var fi = new FileInfo(soundFile);
                var button = new Button()
                {
                    Tag = fi.FullName,
                    Text = fi.Name.Remove(fi.Name.LastIndexOf(".")),
                    Width = flowLayoutPanel1.Width - 25
                };
                button.Click += (a, b) =>
                {
                    PlayInStream(fi.FullName);
                };
                flowLayoutPanel1.Controls.Add(button);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            soundIn?.Dispose();
            soundOut?.Dispose();
            monitoringOut?.Dispose();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ReloadSounds();
        }


    }
}
