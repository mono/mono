using System;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace System.IO
{
	public partial class DirectoryInfo
	{
		private DirectoryInfo(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public DirectorySecurity GetAccessControl()
		{
			return Directory.GetAccessControl(FullPath, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
		{
			return Directory.GetAccessControl(FullPath, includeSections);
		}

		public void SetAccessControl(DirectorySecurity directorySecurity)
		{
			Directory.SetAccessControl(FullPath, directorySecurity);
		}
	}
}
