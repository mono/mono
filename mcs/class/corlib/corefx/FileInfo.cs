using System;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace System.IO
{
	public partial class FileInfo
	{
		private FileInfo(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public FileSecurity GetAccessControl()
		{
			return File.GetAccessControl(FullPath, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public FileSecurity GetAccessControl(AccessControlSections includeSections)
		{
			return File.GetAccessControl(FullPath, includeSections);
		}

		public void SetAccessControl(FileSecurity fileSecurity)
		{
			File.SetAccessControl(FullPath, fileSecurity);
		}

		// Reference source code in Mono still uses this
		internal FileInfo(string fullPath, bool ignoreThis)
		{
			_name = Path.GetFileName(fullPath);
			OriginalPath = _name;
			FullPath = fullPath;
		}

		public override String Name {
			get { return _name; }
		}

	}
}
