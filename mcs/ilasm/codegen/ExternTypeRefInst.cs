//
// Mono.ILASM.ExternTypeRefInst
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {

	public class ExternTypeRefInst : BaseTypeRef {

		private ExternTypeRef type_ref;
		private PEAPI.Type type;
		private bool is_valuetypeinst;
		private bool is_resolved;
		private GenericArguments gen_args;
		private static Hashtable method_table = new Hashtable ();

		public ExternTypeRefInst (ExternTypeRef type_ref, GenericArguments gen_args, bool is_valuetypeinst)
		{
			this.type_ref = type_ref;
			this.gen_args = gen_args;
			this.is_valuetypeinst = is_valuetypeinst;

			is_resolved = false;
		}

		public PEAPI.Type PeapiType {
			get { return type; }
		}

		public string FullName {
			get { return type_ref.FullName; }
		}


		public string SigMod {
			get { return type_ref.SigMod; }
			set { type_ref.SigMod = value; }
		}

		
		public bool IsPinned {
			get { return type_ref.IsPinned; }
		}

		public bool IsRef {
			get { return type_ref.IsRef; }
		}

		public bool IsArray {
			get { return type_ref.IsArray; }
		}

		public bool UseTypeSpec {
			get { return type_ref.UseTypeSpec; }
		}

                public ExternTypeRefInst Clone ()
		{
			return new ExternTypeRefInst (type_ref.Clone (), gen_args, is_valuetypeinst);
		}

		public void MakeArray ()
		{
			is_valuetypeinst = false;
			type_ref.MakeArray ();
		}

		public void MakeBoundArray (ArrayList bounds)
		{
			is_valuetypeinst = false;
			type_ref.MakeBoundArray (bounds);
		}

		public void MakeManagedPointer ()
		{
			type_ref.MakeManagedPointer ();
		}

		public void MakeUnmanagedPointer ()
		{
			type_ref.MakeUnmanagedPointer ();
		}

		public void MakeCustomModified (CodeGen code_gen,
				PEAPI.CustomModifier modifier, BaseClassRef klass)
		{
			type_ref.MakeCustomModified (code_gen, modifier, klass);
		}

		public void MakePinned ()
		{
			type_ref.MakePinned ();
		}

		public void MakeValueClass ()
		{
			type_ref.MakeValueClass ();
		}

		public void Resolve (CodeGen code_gen)
		{
			if (is_resolved)
				return;

			type_ref.Resolve (code_gen);

			type = new PEAPI.GenericTypeInst (type_ref.PeapiType, gen_args.Resolve (code_gen));

			is_resolved = true;
		}

		public BaseMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
				string name, BaseTypeRef[] param, int gen_param_count)
		{
			string key = type_ref.FullName + MethodDef.CreateSignature (ret_type, name, param, gen_param_count) + type_ref.SigMod;
			TypeSpecMethodRef mr = method_table [key] as TypeSpecMethodRef;
			if (mr == null) {	 
				mr = new TypeSpecMethodRef (this, ret_type, call_conv, name, param, gen_param_count);
				method_table [key] = mr;
			}

			return mr;
		}

		public IFieldRef GetFieldRef (BaseTypeRef ret_type, string name)
		{
			return new TypeSpecFieldRef (this, ret_type, name);
		}
	}
}

