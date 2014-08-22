using System.Globalization;
using System.Text;

namespace SharpCompress.Common
{
    internal class ArchiveEncoding
    {
        /// <summary>
        /// Default encoding to use when archive format doesn't specify one.
        /// </summary>
        public static Encoding Default;

        /// <summary>
        /// Encoding used by encryption schemes which don't comply with RFC 2898.
        /// </summary>
        public static Encoding Password;

        static ArchiveEncoding()
        {
#if PORTABLE || NETFX_CORE
            Default = Encoding.UTF8;
            Password = Encoding.UTF8;
#else
            Default = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            Password = Encoding.Default;
#endif
        }
    }
}