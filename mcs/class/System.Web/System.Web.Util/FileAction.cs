//
// System.Web.Util.FileAction.cs
//
// Authors:
//   Gaurav Vaish (my_scripts2001@yahoo.com, gvaish@iitk.ac.in)
//
// (c) Gaurav Vaish 2001
//

namespace System.Web.Util
{
	internal enum FileAction
	{
		Overwhleming,
		Added,
		Removed,
		Modifiled,
		RenamedOldName,
		RenamedNewName,
		Error = 0x7FFFFFFF
	}
}
