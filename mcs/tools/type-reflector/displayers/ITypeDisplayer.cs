//
// ITypeDisplayer.cs: 
//   Framework interface for the displaying of types to a display device (e.g.
//   console or GUI program).
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;

namespace Mono.TypeReflector
{
	public interface ITypeDisplayer
	{
		INodeFormatter Formatter {set;}
		INodeFinder Finder {set;}
		TypeReflectorOptions Options {set;}
		int MaxDepth {set;}
		bool RequireTypes {get;}

		void AddType (Type n);

		void Run ();

		void ShowError (string message);
	}
}

