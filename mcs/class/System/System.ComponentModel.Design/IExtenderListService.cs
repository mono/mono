//
// System.ComponentModel.Design.IExtenderListService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public interface IExtenderListService
	{
		IExtenderProvider[] GetExtenderProviders();
	}
}
