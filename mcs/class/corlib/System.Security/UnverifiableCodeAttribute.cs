//
// System.Security.UnverifiableCodeAttribute.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

namespace System.Security {

	[AttributeUsage (AttributeTargets.Module)]
	public sealed class UnverifiableCodeAttribute : Attribute {}
}
