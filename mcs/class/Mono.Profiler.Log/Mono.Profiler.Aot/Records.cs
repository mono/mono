namespace Mono.Profiler.Aot
{
	//
	// Represents the contents of an .aotprofile file created by the
	// AOT profiler
	//
	public class ProfileRecord
	{
		public ProfileRecord (int id)
		{
			Id = id;
		}

		public int Id {
			get; set;
		}
	}

	public class ModuleRecord : ProfileRecord
	{
		public ModuleRecord (int id, string name, string mvid) : base (id)
		{
			Name = name;
			Mvid = mvid;
		}

		public string Name {
			get; set;
		}

		public string Mvid {
			get; set;
		}
	}

	public class GenericInstRecord : ProfileRecord
	{
		public GenericInstRecord (int id, TypeRecord[] types) : base (id)
		{
			Types = types;
		}

		public TypeRecord[] Types {
			get; set;
		}
	}

	public class TypeRecord : ProfileRecord
	{
		public TypeRecord (int id, ModuleRecord module, string name, GenericInstRecord ginst) : base (id)
		{
			Module = module;
			Name = name;
			GenericInst = ginst;
		}

		public ModuleRecord Module {
			get; set;
		}

		public string Name {
			get; set;
		}

		public GenericInstRecord GenericInst {
			get; set;
		}
	}

	public class MethodRecord : ProfileRecord
	{
		public MethodRecord (int id, TypeRecord type, GenericInstRecord ginst, string name, string sig, int param_count) : base (id)
		{
			Type = type;
			GenericInst = ginst;
			Name = name;
			Signature = sig;
			ParamCount = param_count;
		}

		public TypeRecord Type {
			get; set;
		}

		public GenericInstRecord GenericInst {
			get; set;
		}

		public string Name {
			get; set;
		}

		public string Signature {
			get; set;
		}

		public int ParamCount {
			get; set;
		}
	}
}
