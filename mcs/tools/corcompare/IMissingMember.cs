// Mono.Util.CorCompare.IMissingMember
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak
using System.Xml;

namespace Mono.Util.CorCompare {

	interface IMissingMember 
	{
		string Name { get ; }

		string Status { get; }

		CompletionTypes Completion { get; }

		string Type { get; }

		XmlElement CreateXML (XmlDocument doc);
	}
}
