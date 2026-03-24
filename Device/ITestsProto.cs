using System.Threading.Tasks;
using IRAPROM.MyCore.Model.WP;

namespace PassAlarmSimulator.Device
{
    public interface ITestsProto
    {
        bool StaticTest(WorkParams workParams);

        void HandTest(WorkParams workParams);

        bool DynamicTest(WorkParams workParams, int milliSecondsTimeout, bool alarm);
    }
}
