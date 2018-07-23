using System.Reflection.Emit;

namespace Mono.Compiler
{
	public class MethodInfo
	{
		public ClassInfo ClassInfo { get; }
		public string Name { get; }
		public OpCode [] Stream { get; }

		public MethodInfo (ClassInfo ci, string name, OpCode [] stream) {
			ClassInfo = ci;
			Name = name;
			Stream = stream;
		}
	}
}
