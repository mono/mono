//
// OptionCompareAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Ximian Inc.
//

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
//complete as per Mainsoft code
namespace Microsoft.VisualBasic.CompilerServices {
	[EditorBrowsable(EditorBrowsableState.Never)] 
	[AttributeUsage(AttributeTargets.Parameter)] 
	[StructLayout(LayoutKind.Auto)] 
	sealed public class OptionCompareAttribute : Attribute {
	};

}

