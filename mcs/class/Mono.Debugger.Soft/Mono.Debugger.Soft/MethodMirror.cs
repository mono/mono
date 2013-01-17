using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using C = Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	public class MethodMirror : Mirror
	{
		string name;
		MethodInfo info;
		TypeMirror declaring_type;
		DebugInfo debug_info;
		C.MethodDefinition meta;
		CustomAttributeDataMirror[] cattrs;
		ParameterInfoMirror[] param_info;
		ParameterInfoMirror ret_param;
		LocalVariable[] locals;
		IList<Location> locations;
		MethodBodyMirror body;
		MethodMirror gmd;
		TypeMirror[] type_args;

		internal MethodMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string Name {
			get {
				if (name == null)
					name = vm.conn.Method_GetName (id);
				return name;
			}
		}

		public TypeMirror DeclaringType {
			get {
				if (declaring_type == null)
					declaring_type = vm.GetType (vm.conn.Method_GetDeclaringType (id));
				return declaring_type;
			}
		}

		public TypeMirror ReturnType {
			get {
				return ReturnParameter.ParameterType;
			}
		}

		// FIXME:
		public string FullName {
			get {
				string type_namespace = DeclaringType.Namespace;
				string type_name = DeclaringType.Name;
				StringBuilder sb = new StringBuilder ();
				sb.Append (ReturnType.Name);
				sb.Append (' ');
				if (type_namespace != String.Empty)
					sb.Append (type_namespace + ".");
				sb.Append(type_name);
				sb.Append(":");
				sb.Append(Name);
				sb.Append(" ");
				sb.Append("(");
				for (var i = 0; i < param_info.Length; i++) {
					sb.Append(param_info[i].ParameterType.Name);
					if (i != param_info.Length - 1)
						sb.Append(", ");
				}
				sb.Append(")");
				return sb.ToString ();
			}
		}

		/*
		 * Creating the custom attributes themselves could modify the behavior of the
		 * debuggee, so we return objects similar to the CustomAttributeData objects
		 * used by the reflection-only functionality on .net.
		 * Since protocol version 2.21
		 */
		public CustomAttributeDataMirror[] GetCustomAttributes (bool inherit) {
			return GetCAttrs (null, inherit);
		}

		/* Since protocol version 2.21 */
		public CustomAttributeDataMirror[] GetCustomAttributes (TypeMirror attributeType, bool inherit) {
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");
			return GetCAttrs (attributeType, inherit);
		}

		CustomAttributeDataMirror[] GetCAttrs (TypeMirror type, bool inherit) {
			if (cattrs == null && meta != null && !Metadata.HasCustomAttributes)
				cattrs = new CustomAttributeDataMirror [0];

			// FIXME: Handle inherit
			if (cattrs == null) {
				CattrInfo[] info = vm.conn.Method_GetCustomAttributes (id, 0, false);
				cattrs = CustomAttributeDataMirror.Create (vm, info);
			}
			var res = new List<CustomAttributeDataMirror> ();
			foreach (var attr in cattrs)
				if (type == null || attr.Constructor.DeclaringType == type)
					res.Add (attr);
			return res.ToArray ();
		}

		MethodInfo GetInfo () {
			if (info == null)
				info = vm.conn.Method_GetInfo (id);
			
			return info;
		}

		public int MetadataToken {
			get {
				return GetInfo ().token;
			}
		}

		public MethodAttributes Attributes {
			get {
				return (MethodAttributes) GetInfo ().attributes;
			}
		}

		public bool IsPublic { 
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
			}
		}
		public bool IsPrivate {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
			}
		}
		public bool IsFamily {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
			}
		}
		public bool IsAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
			}
		}
		public bool IsFamilyAndAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
			}
		}
		public bool IsFamilyOrAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
			}
		}
		public bool IsStatic {
			get {
				return (Attributes & MethodAttributes.Static) != 0;
			}
		}
		public bool IsFinal {
			get {
				return (Attributes & MethodAttributes.Final) != 0;
			}
		}
		public bool IsVirtual {
			get {
				return (Attributes & MethodAttributes.Virtual) != 0;
			}
		}
		public bool IsHideBySig {
			get {
				return (Attributes & MethodAttributes.HideBySig) != 0;
			}
		}
		public bool IsAbstract {
			get {
				return (Attributes & MethodAttributes.Abstract) != 0;
			}
		}
		public bool IsSpecialName {
			get {
				return (Attributes & MethodAttributes.SpecialName) != 0;
			}
		}

		public bool IsConstructor {
			get {
				int attr = (int)Attributes;
				return ((attr & (int)MethodAttributes.RTSpecialName) != 0
					&& (Name == ".ctor"));
			}
		}

		// Since protocol version 2.12
		public bool IsGenericMethodDefinition {
			get {
				vm.CheckProtocolVersion (2, 12);
				return GetInfo ().is_gmd;
			}
		}

		// Since protocol version 2.12
		public bool IsGenericMethod {
			get {
				vm.CheckProtocolVersion (2, 12);
				return GetInfo ().is_generic_method;
			}
		}

		public MethodImplAttributes GetMethodImplementationFlags() {
			return (MethodImplAttributes)GetInfo ().iattributes;
		}

		public ParameterInfoMirror[] GetParameters () {
			if (param_info == null) {
				var pi = vm.conn.Method_GetParamInfo (id);
				param_info = new ParameterInfoMirror [pi.param_count];
				// Return
				ret_param = new ParameterInfoMirror (this, -1, vm.GetType (pi.ret_type), null, ParameterAttributes.Retval);
				// FIXME: this
				// FIXME: Attributes
				for (int i = 0; i < pi.param_count; ++i) {
					param_info [i] = new ParameterInfoMirror (this, i, vm.GetType (pi.param_types [i]), pi.param_names [i], 0);
				}
			}

			return param_info;
		}

		public ParameterInfoMirror ReturnParameter {
			get {
				if (ret_param == null)
					GetParameters ();
				return ret_param;
			}
		}

		public LocalVariable[] GetLocals () {
			if (locals == null) {

				LocalsInfo li = new LocalsInfo ();
				try {
					li = vm.conn.Method_GetLocalsInfo (id);
				} catch (CommandException) {
					throw new ArgumentException ("Method doesn't have a body.");
				}
				// Add the arguments as well
				var pi = vm.conn.Method_GetParamInfo (id);

				locals = new LocalVariable [pi.param_count + li.names.Length];

				for (int i = 0; i < pi.param_count; ++i)
					locals [i] = new LocalVariable (vm, this, i, pi.param_types [i], pi.param_names [i], -1, -1, true);

				for (int i = 0; i < li.names.Length; ++i)
					locals [i + pi.param_count] = new LocalVariable (vm, this, i, li.types [i], li.names [i], li.live_range_start [i], li.live_range_end [i], false);
			}
			return locals;
		}

		public LocalVariable GetLocal (string name) {
			if (name == null)
				throw new ArgumentNullException ("name");

			GetLocals ();

			LocalVariable res = null;
			for (int i = 0; i < locals.Length; ++i) {
				if (locals [i].Name == name) {
					if (res != null)
						throw new AmbiguousMatchException ("More that one local has the name '" + name + "'.");
					res = locals [i];
				}
			}

			return res;
		}

		public MethodBodyMirror GetMethodBody () {
			if (body == null) {
				MethodBodyInfo info = vm.conn.Method_GetBody (id);

				body = new MethodBodyMirror (vm, this, info);
			}
			return body;
		}

		public MethodMirror GetGenericMethodDefinition () {
			vm.CheckProtocolVersion (2, 12);
			if (gmd == null) {
				if (info.gmd == 0)
					throw new InvalidOperationException ();
				gmd = vm.GetMethod (info.gmd);
			}
			return gmd;
		}

		// Since protocol version 2.15
		public TypeMirror[] GetGenericArguments () {
			vm.CheckProtocolVersion (2, 15);
			if (type_args == null)
				type_args = vm.GetTypes (GetInfo ().type_args);
			return type_args;
		}

		public IList<int> ILOffsets {
			get {
				if (debug_info == null)
					debug_info = vm.conn.Method_GetDebugInfo (id);
				return Array.AsReadOnly (debug_info.il_offsets);
			}
		}

		public IList<int> LineNumbers {
			get {
				if (debug_info == null)
					debug_info = vm.conn.Method_GetDebugInfo (id);
				return Array.AsReadOnly (debug_info.line_numbers);
			}
		}

		public string SourceFile {
			get {
				if (debug_info == null)
					debug_info = vm.conn.Method_GetDebugInfo (id);
				return debug_info.source_files.Length > 0 ? debug_info.source_files [0].source_file : null;
			}
		}

		public IList<Location> Locations {
			get {
				if (locations == null) {
					var il_offsets = ILOffsets;
					var line_numbers = LineNumbers;
					IList<Location> res = new Location [ILOffsets.Count];
					for (int i = 0; i < il_offsets.Count; ++i)
						res [i] = new Location (vm, this, -1, il_offsets [i], debug_info.source_files [i].source_file, line_numbers [i], debug_info.column_numbers [i], debug_info.source_files [i].hash);
					locations = res;
				}
				return locations;
			}
		}				

		internal int il_offset_to_line_number (int il_offset, out string src_file, out byte[] src_hash, out int column_number) {
			if (debug_info == null)
				debug_info = vm.conn.Method_GetDebugInfo (id);

			// FIXME: Optimize this
			src_file = null;
			src_hash = null;
			column_number = 0;
			for (int i = debug_info.il_offsets.Length - 1; i >= 0; --i) {
				if (debug_info.il_offsets [i] <= il_offset) {
					src_file = debug_info.source_files [i].source_file;
					src_hash = debug_info.source_files [i].hash;
					column_number = debug_info.column_numbers [i];
					return debug_info.line_numbers [i];
				}
			}
			return -1;
		}

		public Location LocationAtILOffset (int il_offset) {
			IList<Location> locs = Locations;

			// FIXME: Optimize this
			for (int i = locs.Count - 1; i >= 0; --i) {
				if (locs [i].ILOffset <= il_offset)
					return locs [i];
			}

			return null;
		}

		public C.MethodDefinition Metadata {
			get {
				if (meta == null)
					meta = (C.MethodDefinition)DeclaringType.Assembly.Metadata.MainModule.LookupToken (MetadataToken);
				return meta;
			}
		}
	}
}
