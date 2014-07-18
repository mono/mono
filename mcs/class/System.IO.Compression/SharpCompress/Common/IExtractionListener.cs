namespace SharpCompress.Common
{
    internal interface IExtractionListener
    {
        void FireFilePartExtractionBegin(string name, long size, long compressedSize);
        void FireCompressedBytesRead(long currentPartCompressedBytes, long compressedReadBytes);
    }
}