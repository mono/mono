//
// System.ComponentModel.Design.InheritanceLevel
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	[Serializable]
        public enum InheritanceLevel
	{
		Inherited,
		InheritedReadOnly,
		NotInherited,
	}
}
