/**
 * 
 * Namespace: System.Web.Utils
 * Class:     FileChangesMonitor
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * 
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.Utils
{
	internal class FileChangesMonitor
	{
		private static string BIN    = "bin";
		private static string BINDIR = "bin/";
		private static string MAXLEN = 260;
		
		private FileChangeEventHandler rename;
		private NativeFileChangeEventHandler subDir;
		
		private int    monitoredSubdirs;
		private string rootDir;
		//private 
	}
}
