//
// System.ComponentModel.Design.IRootDesigner
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
        public interface IRootDesigner : IDesigner, IDisposable
	{
		ViewTechnology[] SupportedTechnologies {get;}
		object GetView (ViewTechnology technology);
	}
}
