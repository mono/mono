//
// System.ComponentModel.Design.IExtenderProviderService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public interface IExtenderProviderService
	{
		void AddExtenderProvider (IExtenderProvider provider);
		void RemoveExtenderProvider (IExtenderProvider provider);
	}
}
