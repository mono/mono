//
// System.ComponentModel.Design.IDesignerFilter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.ComponentModel.Design
{
	public interface IDesignerFilter
	{
		void PostFilterAttributes (IDictionary attributes);
		void PostFilterEvents (IDictionary events);
		void PostFilterProperties (IDictionary properties);
		void PreFilterAttributes (IDictionary attributes);
		void PreFilterEvents (IDictionary events);
		void PreFilterProperties (IDictionary properties);
	}
}
