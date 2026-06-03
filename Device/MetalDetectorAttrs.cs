using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRAPROM.MyCore.Device
{
    public class MetalDetectorAttrs
    {
        public ushort Id { get; set; }
        public string ModelName { get; set; }

        public string Name = "";
        public List<short> AvailableZonesCount = new List<short>();
        public List<int> GridCellDefinitions = new List<int>();
        public int RealCoilsCount;

        public MetalDetectorAttrs(ushort id, string modelName, List<short> availableZonesCount, List<int> gridCellDefinitions, int realCoilsCount)
        {
            Id = id;
            ModelName = modelName;
            AvailableZonesCount = availableZonesCount;
            GridCellDefinitions = gridCellDefinitions;
            RealCoilsCount = realCoilsCount;
        }
    }
}
