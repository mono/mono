//
// System.Collections.IEnumerator.cs
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;

namespace System.Collections
{

	[Guid ("496B0ABF-CDEE-11D3-88E8-00902754C43A")]
	public interface IEnumerator
	{
		object Current { get; }

		bool MoveNext ();

		void Reset ();
	}

}
