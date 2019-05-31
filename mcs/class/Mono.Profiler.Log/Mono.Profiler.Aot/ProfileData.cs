namespace Mono.Profiler.Aot
{
	public sealed class ProfileData
	{
		public ProfileData (ModuleRecord[] modules, TypeRecord[] types, MethodRecord[] methods)
		{
			this.Modules = modules;
			this.Types = types;
			this.Methods = methods;
		}

		public ModuleRecord[] Modules { get; private set; }
		public TypeRecord[] Types { get; private set; }
		public MethodRecord[] Methods { get; private set; }
	}
}