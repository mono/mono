using System;

namespace Mono.Doc.Core
{
	[Flags]
	public enum NamingFlags
	{
		None              = 1,
		FullName          = 2,
		TypeSpecifier     = 4,
		ForceMethodParams = 8
	}
}