//
// System.ComponentModel.InheritanceLevel.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[Serializable]
	public enum InheritanceLevel
	{
		Inherited = 1,
		InheritedReadOnly = 2,
		NotInherited = 3
	}
}

