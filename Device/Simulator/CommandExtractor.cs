using Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAlarmSimulator.Device.Simulator
{
    public class CommandExtractor
    {
        private readonly string _dirPath;

        public CommandExtractor(string dirPath)
        {
            _dirPath = dirPath;
        }

        public byte[] ExtractCommand(byte code)
        {
            var file = FindFileCommand(code);

            return file == null ? Array.Empty<byte>() : Convert.FromHexString(File.ReadAllText(file));
        }

        private string FindFileCommand(byte code)
        {
            return Directory.GetFiles(_dirPath, $"{code}.txt").FirstOrDefault();
        }
    }
}
