// CS0618: `Name.Feat.Feat(string, string, int, params object[])' is obsolete: `AaA'
// Line: 22
// Compiler options: -warnaserror

using System;

namespace Name
{
	public class A
	{
	}
	
	public class Feat
	{
		#region Constructors

		[Obsolete ("AaA")]
		public Feat(string name, string description, int arg, params object[] featReqs)
		{}

		public Feat(string name, string description)
			: this(name, description, 4)
		{}

		public Feat(string name)
			: this(name, string.Empty)
		{}

		#endregion
	}
}