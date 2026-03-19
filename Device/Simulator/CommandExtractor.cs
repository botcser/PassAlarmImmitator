using System;
using System.IO;
using System.Linq;

namespace PassAlarmSimulator.Device.Simulator
{
    public class CommandExtractor
    {
        private readonly string _dirPath;

        public CommandExtractor(string dirPath)
        {
            _dirPath = dirPath;
        }

        public byte[] ExtractCommand(short code)
        {
            var file = FindFileCommand(code);

            return file == null ? Array.Empty<byte>() : Convert.FromHexString(File.ReadAllText(file));
        }

        private string FindFileCommand(short code)
        {
            return Directory.GetFiles(_dirPath, $"{code:X2}.txt").FirstOrDefault();
        }
    }
}
