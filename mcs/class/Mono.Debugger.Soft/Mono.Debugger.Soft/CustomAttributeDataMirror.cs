using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft {

	public sealed class CustomAttributeDataMirror {
		MethodMirror ctorInfo;
		IList<CustomAttributeTypedArgumentMirror> ctorArgs;
		IList<CustomAttributeNamedArgumentMirror> namedArgs;

		internal CustomAttributeDataMirror (MethodMirror ctorInfo, object [] ctorArgs, object [] namedArgs)
		{
			this.ctorInfo = ctorInfo;
			
			this.ctorArgs = Array.AsReadOnly<CustomAttributeTypedArgumentMirror> 
				(ctorArgs != null ? UnboxValues<CustomAttributeTypedArgumentMirror> (ctorArgs) : new CustomAttributeTypedArgumentMirror [0]);
			
			this.namedArgs = Array.AsReadOnly<CustomAttributeNamedArgumentMirror> 
				(namedArgs != null ? UnboxValues<CustomAttributeNamedArgumentMirror> (namedArgs) : new CustomAttributeNamedArgumentMirror [0]);
		}

		[ComVisible (true)]
		public MethodMirror Constructor {
			get {
				return ctorInfo;
			}
		}

		[ComVisible (true)]
		public IList<CustomAttributeTypedArgumentMirror> ConstructorArguments {
			get {
				return ctorArgs;
			}
		}

		public IList<CustomAttributeNamedArgumentMirror> NamedArguments {
			get {
				return namedArgs;
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("[" + ctorInfo.DeclaringType.FullName + "(");
			if (ctorArgs != null) {
				for (int i = 0; i < ctorArgs.Count; i++) {
					sb.Append (ctorArgs [i].ToString ());
					if (i + 1 < ctorArgs.Count)
						sb.Append (", ");
				}
			}

			if (namedArgs != null) {
				if (namedArgs.Count > 0)
					sb.Append (", ");
			
				for (int j = 0; j < namedArgs.Count; j++) {
					sb.Append (namedArgs [j].ToString ());
					if (j + 1 < namedArgs.Count)
						sb.Append (", ");
				}
			}
			sb.AppendFormat (")]");

			return sb.ToString ();
		}

		static T [] UnboxValues<T> (object [] values)
		{
			T [] retval = new T [values.Length];
			for (int i = 0; i < values.Length; i++)
				retval [i] = (T) values [i];

			return retval;
		}

		/* 
		 * Construct a normal object from the value, so accessing the cattr doesn't 
		 * require remoting calls.
		 */
		static CustomAttributeTypedArgumentMirror CreateArg (VirtualMachine vm, ValueImpl vi) {
			object val;

			/* Instead of receiving a mirror of the Type object, we receive the id of the type */
			if (vi.Type == (ElementType)ValueTypeId.VALUE_TYPE_ID_TYPE)
				val = vm.GetType (vi.Id);
			else {
				Value v = vm.DecodeValue (vi);
				if (v is PrimitiveValue)
					val = (v as PrimitiveValue).Value;
				else if (v is StringMirror)
					val = (v as StringMirror).Value;
				else
					// FIXME:
					val = v;
			}
			return new CustomAttributeTypedArgumentMirror (null, val);
		}

		internal static CustomAttributeDataMirror[] Create (VirtualMachine vm, CattrInfo[] info) {
			var res = new CustomAttributeDataMirror [info.Length];
			for (int i = 0; i < info.Length; ++i) {
				CattrInfo attr = info [i];
				MethodMirror ctor = vm.GetMethod (attr.ctor_id);
				var ctor_args = new object [attr.ctor_args.Length];
				for (int j = 0; j < ctor_args.Length; ++j)
					ctor_args [j] = CreateArg (vm, attr.ctor_args [j]);
				var named_args = new object [attr.named_args.Length];
				for (int j = 0; j < named_args.Length; ++j) {
					CattrNamedArgInfo arg = attr.named_args [j];
					CustomAttributeTypedArgumentMirror val;

					val = CreateArg (vm, arg.value);

					if (arg.is_property) {
						foreach (var prop in ctor.DeclaringType.GetProperties ()) {
							if (prop.Id == arg.id)
								named_args [j] = new CustomAttributeNamedArgumentMirror (prop, null, val);
						}
					} else {
						foreach (var field in ctor.DeclaringType.GetFields ()) {
							if (field.Id == arg.id)
								named_args [j] = new CustomAttributeNamedArgumentMirror (null, field, val);
						}
					}
					if (named_args [j] == null)
						throw new NotImplementedException ();
				}
				res [i] = new CustomAttributeDataMirror (ctor, ctor_args, named_args);
			}

			return res;
		}
	}

}
