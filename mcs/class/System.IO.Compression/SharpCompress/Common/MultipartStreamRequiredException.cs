namespace SharpCompress.Common
{
    internal class MultipartStreamRequiredException : ExtractionException
    {
        public MultipartStreamRequiredException(string message)
            : base(message)
        {
        }
    }
}