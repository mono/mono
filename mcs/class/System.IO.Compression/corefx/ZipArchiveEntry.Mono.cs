using System.Diagnostics;

namespace System.IO.Compression
{
	public partial class ZipArchiveEntry
	{
		internal static readonly ZipVersionMadeByPlatform CurrentZipPlatform = Path.PathSeparator == '/' ? 
			ZipVersionMadeByPlatform.Unix : ZipVersionMadeByPlatform.Windows;

		internal static string ParseFileName(string path, ZipVersionMadeByPlatform madeByPlatform)
		{
			switch (madeByPlatform) {
			case ZipVersionMadeByPlatform.Windows:
				return GetFileName_Windows(path);
			case ZipVersionMadeByPlatform.Unix:
				return GetFileName_Unix(path);
			default:
				return ParseFileName (path, CurrentZipPlatform);
			}
		}
	}
}