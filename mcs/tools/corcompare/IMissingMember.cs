// Mono.Util.CorCompare.IMissingMember
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

namespace Mono.Util.CorCompare {

	interface IMissingMember 
	{
		string Name { get ; }

		string Status { get; }

		string Type { get; }
	}
}
