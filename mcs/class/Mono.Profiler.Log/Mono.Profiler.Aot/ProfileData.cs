using System.Collections.Generic;

namespace Mono.Profiler.Aot
{
	public class ProfileData
	{
		internal ProfileData ()
		{
			Modules = new List<ModuleRecord> ();
			Types = new List<TypeRecord> ();
			Methods = new List<MethodRecord> ();
		}

		public List<ModuleRecord> Modules { get; private set; }

		public List<TypeRecord> Types { get; private set; }

		public List<MethodRecord> Methods { get; private set; }
	}
}