using Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

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

            return file.IsNullOrEmpty() ? Array.Empty<byte>() : Convert.FromHexString(File.ReadAllText(file));
        }

        private string FindFileCommand(byte code)
        {
            try
            {
                return Directory.GetFiles(_dirPath, $"{code:X2}.txt").FirstOrDefault(); ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;;
            }
        }
    }
}
