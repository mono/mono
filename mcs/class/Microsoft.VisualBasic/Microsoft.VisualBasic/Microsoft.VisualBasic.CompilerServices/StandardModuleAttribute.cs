//
// StandardModuleAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualBasic.CompilerServices {
	[AttributeUsage(AttributeTargets.Class)] 
	[EditorBrowsable(EditorBrowsableState.Never)] 
	[StructLayout(LayoutKind.Auto)] 
	[MonoTODO("What should it do?")]
	sealed public class StandardModuleAttribute : Attribute {
	};
}
