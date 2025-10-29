namespace IRAPROM.MyCore.Model.MD
{
    public enum MetalDetectorSeries             // создано чтобы неООП ужилось с ООП
    {
        Unknown = 0,
        BlockPost,
        Matryoshka,
        Impulse
    }

    public static class MetalDetectorSeriesLib
    {
        public static string GetItemName(MetalDetectorSeries val)
        {
            switch (val)
            {
                case MetalDetectorSeries.BlockPost:
                {
                    return "Блокпост";
                }
                case MetalDetectorSeries.Matryoshka:
                {
                    return "Матрешка";
                }
                case MetalDetectorSeries.Impulse:
                {
                    return "Импульс";
                }
                case MetalDetectorSeries.Unknown:
                default:
                {
                    return "Unknown";
                }
            }
        }
        
        public static MetalDetectorSeries ModelType(MetalDetectorModel val)
        {
            switch (val)
            {
                case MetalDetectorModel.z400:
                case MetalDetectorModel.x400:
                case MetalDetectorModel.z600:
                case MetalDetectorModel.x600:
                case MetalDetectorModel.z1200:
                case MetalDetectorModel.x1200:
                case MetalDetectorModel.z1800:
                case MetalDetectorModel.x1800:
                    return MetalDetectorSeries.BlockPost;

                case MetalDetectorModel.PCV6300:
                case MetalDetectorModel.PCV9300:
                case MetalDetectorModel.PCVx900:
                case MetalDetectorModel.PCVx9300_MZ6MK:
                case MetalDetectorModel.PCV480016_PCVi1800mb:
                case MetalDetectorModel.PCVx480016_PCVi3300mb:
                case MetalDetectorModel.MV6:
                case MetalDetectorModel.MZ6MK:
                case MetalDetectorModel.PCV900:
                case MetalDetectorModel.PCV33006:
                case MetalDetectorModel.PCV4800:
                case MetalDetectorModel.PCVx1800:
                case MetalDetectorModel.PCVx3300:
                case MetalDetectorModel.PCVx4800_MZmb:
                case MetalDetectorModel.PCVx6300_MZmb:
                case MetalDetectorModel.PCV90011:
                case MetalDetectorModel.PCV180011:
                case MetalDetectorModel.PCV3300:
                case MetalDetectorModel.PCV480011:
                case MetalDetectorModel.PCV630011_MV6mb:
                case MetalDetectorModel.PCV930011:
                case MetalDetectorModel.PCVx90011:
                case MetalDetectorModel.PCVx180011:
                case MetalDetectorModel.PCVx330011:
                case MetalDetectorModel.PCVx480011:
                case MetalDetectorModel.PCVx630011:
                case MetalDetectorModel.PCVx930011:
                case MetalDetectorModel.PCV630016:
                case MetalDetectorModel.PCV930016:
                case MetalDetectorModel.PCVx630016:
                case MetalDetectorModel.PCVx930016:
                case MetalDetectorModel.MVx6:
                case MetalDetectorModel.MV11_hz:
                case MetalDetectorModel.MVx11_hz:
                case MetalDetectorModel.MV16_hz:
                case MetalDetectorModel.MVx16_hz:
                case MetalDetectorModel.PCV1800:
                    return MetalDetectorSeries.Matryoshka;

                case MetalDetectorModel.PC600MKX:
                case MetalDetectorModel.PC1800MKZ:
                case MetalDetectorModel.PC4400MK:
                case MetalDetectorModel.PC600MKZ:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    return MetalDetectorSeries.Impulse;

                case MetalDetectorModel.Unknown:
                default:
                    return MetalDetectorSeries.Unknown;
            }
        }

        public static MetalDetectorSeries GetSeries(MetalDetectorModel model)
        {
            var isDefined = Enum.IsDefined(typeof(MetalDetectorModel), model);

            return !isDefined ? MetalDetectorSeries.Unknown : ModelType(model);
        }

        public static MetalDetectorModel GetModel(short number)
        {
            return (MetalDetectorModel)number;
        }
    }
}
