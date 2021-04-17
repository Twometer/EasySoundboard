using CSCore;
using CSCore.Codecs;
using CSCore.Streams;
using System;
using System.Collections.Generic;

namespace EasySoundboard
{
    public class SourceCache
    {
        private Dictionary<string, ISampleSource> sources = new Dictionary<string, ISampleSource>();

        public ISampleSource Load(string path, int sampleRate, int volume)
        {
            if (sources.ContainsKey(path))
            {
                var source = sources[path];
                if (source.Length - source.Position > 50)
                {
                    return null;
                }
                source.Position = 0;
                return source;
            }
            else
            {
                var source = CodecFactory.Instance.GetCodec(path)
                    .ToSampleSource()
                    .ToStereo()
                    .ChangeSampleRate(sampleRate)
                    .AppendSource(x => new VolumeSource(x) { Volume = volume / 100.0f });

                sources[path] = source;
                return source;
            }
        }

    }
}
