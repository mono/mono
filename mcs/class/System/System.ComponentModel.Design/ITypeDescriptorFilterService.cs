//
// System.ComponentModel.Design.ITypeDescriptorFilterService.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.ComponentModel.Design
{
	public interface ITypeDescriptorFilterService
	{
		bool FilterAttributes (IComponent component,
				       IDictionary attributes);

		bool FilterEvents (IComponent component, 
				   IDictionary events);

		bool FilterProperties (IComponent component, 
				       IDictionary properties);
	}
}
