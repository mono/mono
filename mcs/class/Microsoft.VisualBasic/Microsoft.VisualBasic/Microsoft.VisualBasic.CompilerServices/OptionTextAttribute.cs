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

//complete. matches Mainsoft code.
namespace Microsoft.VisualBasic.CompilerServices
{
	[AttributeUsage(AttributeTargets.Class)] 
	[EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class OptionTextAttribute : Attribute {
	};
}

