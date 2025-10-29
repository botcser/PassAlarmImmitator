using System.Collections.Generic;

namespace Device
{
    public class Commands
    {
        public List<Command> GetCommandsList = new List<Command>();
        public List<Command> SetCommandsList = new List<Command>();

        public Commands(IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands)
        {
            getCommands.ForEach(cmd =>
            {
                GetCommandsList.Add(new Command(datagramProto.MakeRequestDatagram(cmd.Item1), cmd.Item2, cmd.Item1, cmd.Item3));
            });

            setCommands.ForEach(cmd =>
            {
                SetCommandsList.Add(new Command(cmd.Item1, cmd.Item3));
            });
        }
    }
}
