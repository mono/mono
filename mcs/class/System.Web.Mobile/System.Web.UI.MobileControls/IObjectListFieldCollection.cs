/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : IObjectListFieldCollection
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;

namespace System.Web.UI.MobileControls
{
	public interface IObjectListFieldCollection : ICollection
	{
		ObjectListField this[int index] { get; }
		
		ObjectListField[] GetAll();
		int               IndexOf(ObjectListField field);
		int               ObjectListField(string fieldID);
	}
}
