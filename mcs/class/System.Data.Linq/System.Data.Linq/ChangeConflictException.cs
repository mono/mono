namespace System.Data.Linq
{
    public class ChangeConflictException : Exception
    {
        public ChangeConflictException()
        {
        }

        public ChangeConflictException(string message)
            : base(message)
        {
        }

        public ChangeConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}