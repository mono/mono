/**
 * Namespace: System.Web.Utils
 * Class:     FileChangedEventArgs
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.Utils
{
	internal class FileChangedEventArgs : EventArgs
	{
		private string filename;
		private FileAction action;

		public FileChangedEvent(FileAction action, string file)
		{
			this.action = action;
			this.filename = file;
		}

		public string FileName
		{
			get
			{
				return filename;
			}
		}

		public FileAction Action
		{
			get
			{
				return action;
			}
		}
	}
}
