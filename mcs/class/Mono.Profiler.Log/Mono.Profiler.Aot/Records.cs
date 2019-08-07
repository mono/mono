using System.Text;

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

		public override string ToString ()
		{
			return Name;
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

		public override string ToString ()
		{
			if (Types == null || Types.Length <= 0)
				return "";

			var sb = new StringBuilder ("<");
			var first = true;
			foreach (var type in Types) {
				if (!first)
					sb.Append (", ");
				else
					first = false;

				sb.Append (type.ToString ());
			}

			sb.Append (">");

			return sb.ToString ();
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

		public string FullName {
			get {
				string prefix;

				if (Name.Length > 0 && Name [0] == '.')
					prefix = Module.ToString ();
				else
					prefix = "";

				return $"{prefix}{Name}{GenericInst}";
			}
		}

		public GenericInstRecord GenericInst {
			get; set;
		}

		public override string ToString ()
		{
			return FullName;
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

		public override string ToString ()
		{
			return $"{Signature.Replace ("(", $" {Type}:{Name} (")}";
		}
	}
}
