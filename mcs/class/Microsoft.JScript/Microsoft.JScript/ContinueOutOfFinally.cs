//
// ContinueOutOfFinally.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public sealed class ContinueOutOfFinally : ApplicationException
	{
		public int target;

		public ContinueOutOfFinally (int target)
		{
			this.target = target;
		}
	}
}