/**
 * Namespace: System.Web.Util
 * Class:     FileAction
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.Util
{
	public enum FileAction
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
