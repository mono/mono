//
// System.ComponentModel.Design.IDictionaryService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public interface IDictionaryService
	{
		object GetKey (object value);
		object GetValue (object key);
		void SetValue (object key, object value);
	}
}
