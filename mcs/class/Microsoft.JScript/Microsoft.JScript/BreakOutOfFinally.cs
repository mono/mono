//
// BreakOutOfFinally.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public sealed class BreakOutOfFinally : ApplicationException
	{
		public int target;

		public BreakOutOfFinally (int target)
		{
			this.target = target;
		}
	}
}