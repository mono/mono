//
// System.Web.Util.WorkItem.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Web.Util
{
	public class WorkItem
	{
		public WorkItem ()
		{
		}

		public static void Post (WorkItemCallback callback)
		{
			throw new PlatformNotSupportedException ("Not supported on mono");
		}
	}
}
