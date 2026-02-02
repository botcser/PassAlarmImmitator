namespace Extensions
{
    public interface IStart
    {
        Task Start();

        void Shutdown();
    }
}
