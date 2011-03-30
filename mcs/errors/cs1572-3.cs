// CS1572: XML comment on `Testing.Test.D' has a param tag for `mismatch', but there is no parameter by that name
// Line: 10
// Compiler options: -doc:dummy.xml -warn:2 -warnaserror

namespace Testing
{
	class Test
	{
		/// <param name='mismatch'>mismatch</param>
		public delegate void D (int i);
	}
}
