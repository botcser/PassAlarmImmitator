namespace IRAPROM.MyCore.Auxiliary
{
    public static class MDCommands
    {
        public const short METDET_CMD_NORMAL_GET_PASSAGES = 0x41; //Счетчики детектора

        public const short METDET_CMD_ALARM = 0x42; //Информация  об Alarm


        public const short COMM_SETPASSNUMPARA = 0x0009;

        public const short COMM_SETALARMPARA = 0x0006;
        public const short COMM_SETWORKMODE = 0x0005;

        public const short COMM_SETLEVEL = 0x0002;

        public const short COMM_SETWORKINGFREQ = 0x0004;

        public const short COMM_SETMODELS = 0x000A;

        public const short COMM_GET_SERIAL_PORT = 0x0020;

        public const short COMM_GetNetworkParams = 0x21;
        public const short COMM_GETLEVEL = 0x0022; //base sensitivity
        public const short COMM_GETSENS = 0x0023;
        public const short COMM_GETWORKINGFREQ = 0x0024;
        public const short COMM_GET_WORKING_MODE = 0x0025;
        public const short COMM_GETALARMPARAMS = 0x0026;
        
        public const short COMM_GETLICENCE_OR_MODEL = 0x0028;
        public const short COMM_GET_PASSAGE_COUNT = 0x0029;

        public const short COMM_GETMODELS_OR_SCENE_WORK_PROGRAM = 0x002A; // SCENE work program

        public const short COMM_SETSENS = 0x0003;

        public const short METDET_CMD_FINDANSWER = 0x1040; //Ответ на команду поиска устройств (матрехи?)

        public static byte[] FindMDDatagramMatreshka = { 0x40, 0x23, 0x24, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x30, 0x30, 0x0d, 0x0a };
        public static byte[] FindMDDatagramImpulse = { 0x5B, 0xaa, 0x40 };


        public const short CMD_SetNetworkParams = 0x01;



        public const short COMM_SETLICENCE = 0x0008;

        public const short COMM_GEREGSTATUS = 0x002C; //Status

        public const short CMD_MagneticField = 322;  //Информационные сообщения о магнитном поле
    }

}
