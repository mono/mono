//
// System.ComponentModel.Design.IDesignerOptionService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public interface IDesignerOptionService
	{
		object GetOptionValue (string pageName, string valueName);
		void SetOptionValue (string pageName, string valueName, 
				     object value);
	}
}
