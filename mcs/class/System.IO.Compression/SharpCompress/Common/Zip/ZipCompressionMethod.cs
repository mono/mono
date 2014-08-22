namespace SharpCompress.Common.Zip
{
    internal enum ZipCompressionMethod
    {
        None = 0,
        Deflate = 8,
        Deflate64 = 9,
        BZip2 = 12,
        LZMA = 14,
        PPMd = 98,
        WinzipAes = 0x63 //http://www.winzip.com/aes_info.htm
    }
}