using System.Collections.Generic;

namespace IRAPROM.MyCore.Device
{
    public class Commands
    {
        public List<Command> GetCommandsList = new List<Command>();
        public List<Command> SetCommandsList = new List<Command>();

        public Commands(IDatagramProto datagramProto, List<(short, short, int, string)> getCommands, List<(short, short, int, string)> setCommands)
        {
            getCommands.ForEach(cmd =>
            {
                GetCommandsList.Add(new Command(datagramProto.MakeRequestDatagram(cmd.Item1), cmd.Item1, cmd.Item2, cmd.Item3, cmd.Item4));
            });

            setCommands.ForEach(cmd =>
            {
                SetCommandsList.Add(new Command(cmd.Item1, cmd.Item2, cmd.Item3, cmd.Item4));
            });
        }
    }
}