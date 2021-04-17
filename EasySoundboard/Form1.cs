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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySoundboard
{
    public partial class Form1 : Form
    {
        private SimpleMixer mixer;
        private WasapiCapture soundIn;
        private WasapiOut soundOut;
        private WasapiOut monitoringOut;

        public Form1()
        {
            InitializeComponent();
            ReloadDevices();
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

                mixer = new SimpleMixer(soundIn.WaveFormat.Channels, soundIn.WaveFormat.SampleRate);

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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            soundIn?.Dispose();
            soundOut?.Dispose();
            monitoringOut?.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var temporarySource = CodecFactory.Instance.GetCodec(@"C:\Users\twome\Downloads\ahhyooaaawhoaaa.mp3").ToSampleSource().ToStereo().ChangeSampleRate(soundIn.WaveFormat.SampleRate);
            mixer.AddSource(temporarySource);
        }
    }
}
