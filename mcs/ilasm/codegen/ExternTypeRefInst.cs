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

	public class ExternTypeRefInst : ITypeRef {

		private ExternTypeRef type_ref;
		private PEAPI.Type type;
		private bool is_valuetypeinst;
		private bool is_resolved;

		public ExternTypeRefInst (ExternTypeRef type_ref, bool is_valuetypeinst)
		{
			this.type_ref = type_ref;
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
                        return new ExternTypeRefInst (type_ref.Clone (), is_valuetypeinst);
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
				PEAPI.CustomModifier modifier, IClassRef klass)
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
			type = new PEAPI.ClassRefInst (type_ref.PeapiType, is_valuetypeinst);

			is_resolved = true;
		}

		public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
				string name, ITypeRef[] param)
		{
			return new TypeSpecMethodRef (this, ret_type, call_conv, name, param);
		}

		public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
		{
			return new TypeSpecFieldRef (this, ret_type, name);
		}
	}
}

