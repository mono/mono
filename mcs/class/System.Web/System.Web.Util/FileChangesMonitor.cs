/**
 * 
 * Namespace: System.Web.Util
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

using System;
using System.IO;
using System.Web;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;

namespace System.Web.Util
{
	internal class FileChangesMonitor
	{
		private static string BIN    = "bin";
		private static string BINDIR = "bin/";
		private static int MAXLEN = 260;
		
		private FileChangeEventHandler       cbRename;
		private NativeFileChangeEventHandler cbSubDirs;
		
		private int       monitoredSubdirs;
		private string    rootDir;
		private Hashtable allDirs;
		private GCHandle  rootcbSubDirs;
		
		private ReaderWriterLock rwLock;
		
		public FileChangesMonitor()
		{
			allDirs = new Hashtable(WebHashCodeProvider.Default, WebEqualComparer.Default);
			rwLock  = new ReaderWriterLock();
		}
		
		/// <param name="file">Name of the file</param>
		/// <param name="mTime">Last modification date</param>
		/// <param name="length">Legnth of the file</param>
		[MonoTODO]
		public void GetFileAttributes(string file, out DateTime mTime, long length)
		{
			if(!Path.IsPathRooted(file))
			{
				throw new HttpException(HttpRuntime.FormatResourceString("Path_must_be_rooted"));
			}
			// TODO: finish this
			mTime = DateTime.Now;
			throw new NotImplementedException();
		}
	}
}
