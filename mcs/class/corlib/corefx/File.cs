using System;
using System.Security.AccessControl;

namespace System.IO
{
	public partial class File
	{
		public static FileSecurity GetAccessControl (string path)
		{
			// AccessControlSections.Audit requires special permissions.
			return GetAccessControl (path,
						 AccessControlSections.Owner |
						 AccessControlSections.Group |
						 AccessControlSections.Access);
		}

		public static FileSecurity GetAccessControl (string path, AccessControlSections includeSections)
		{
			return new FileSecurity (path, includeSections);
		}

		public static void SetAccessControl (string path,
						     FileSecurity fileSecurity)
		{
			if (null == fileSecurity)
				throw new ArgumentNullException ("fileSecurity");

			fileSecurity.PersistModifications (path);
		}
    }
}