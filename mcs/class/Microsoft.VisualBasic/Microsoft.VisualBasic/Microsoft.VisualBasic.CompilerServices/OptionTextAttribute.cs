//
// OptionTextAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
using System;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices {
	[AttributeUsage(AttributeTargets.Class)] 
	[EditorBrowsable(EditorBrowsableState.Never)] 
	[MonoTODO("What should it do?")]
	sealed public class OptionTextAttribute : Attribute {
	};
}
