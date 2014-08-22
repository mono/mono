namespace SharpCompress.Common
{
    internal class IncompleteArchiveException : ArchiveException
    {
        public IncompleteArchiveException(string message)
            : base(message)
        {
        }
    }
}