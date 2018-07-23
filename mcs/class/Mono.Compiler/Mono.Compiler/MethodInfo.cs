using SimpleJit.Metadata;

namespace Mono.Compiler
{
	public class MethodInfo
	{
		public ClassInfo ClassInfo { get; }
		public string Name { get; }
		public MethodBody Body { get; }

		public MethodInfo (ClassInfo ci, string name, MethodBody body) {
			ClassInfo = ci;
			Name = name;
			Body = body;
		}


	}
}
