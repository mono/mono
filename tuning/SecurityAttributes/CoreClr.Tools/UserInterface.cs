namespace CoreClr.Tools
{
    public interface UserInterface
    {
        void Info(string format, params object[] args);

        void Warning(string format, params object[] args);
    }
}