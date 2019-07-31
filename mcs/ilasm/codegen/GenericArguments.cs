//
// Mono.ILASM.GenericArguments
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Text;

namespace Mono.ILASM {

	public class GenericArguments {
		ArrayList type_list;
		string type_str;
		BaseTypeRef [] type_arr;
		bool is_resolved;
		PEAPI.Type [] p_type_list;

		public GenericArguments ()
		{
			type_list = null;
			type_arr = null;
			type_str = null;
			is_resolved = false;
			p_type_list = null;
		}

		public int Count {
			get { return type_list.Count; }
		}

		public void Add (BaseTypeRef type)
		{
			if (type == null)
				throw new InternalErrorException ();

			if (type_list == null)
				type_list = new ArrayList ();
			var prim = PrimitiveTypeRef.GetPrimitiveType (type.FullName);
			if (prim != null)
				type_list.Add (prim);
			else
				type_list.Add (type);
			type_str = null;
			type_arr = null;
		}
		
		public BaseTypeRef [] ToArray ()
		{
			if (type_list == null)
				return null;
			if (type_arr == null)
				type_arr = (BaseTypeRef []) type_list.ToArray (typeof (BaseTypeRef));

			return type_arr;
		}

		public PEAPI.Type [] Resolve (CodeGen code_gen)
		{
			if (is_resolved)
				return p_type_list;

			int i = 0;
			p_type_list = new PEAPI.Type [type_list.Count];
			foreach (BaseTypeRef type in type_list) {
				type.Resolve (code_gen);
				p_type_list [i ++] = type.PeapiType;
			}
			is_resolved = true;
			type_str = null;
			return p_type_list;
		}

		public void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params)
		{
			foreach (BaseTypeRef type in type_list) {
				BaseGenericTypeRef gtr = type as BaseGenericTypeRef;
				if (gtr != null)
					gtr.Resolve (type_gen_params, method_gen_params);
			}
			/* Reset, might have changed (think GenericParamRef) */
			type_str = null;
		}

		private void MakeString ()
		{
			//Build full_name (foo < , >)
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<");
			foreach (BaseTypeRef tr in type_list)
				sb.AppendFormat ("{0}, ", tr.FullName);
			//Remove the extra ', ' at the end
			sb.Length -= 2;
			sb.Append (">");
			type_str = sb.ToString ();
		}

		public override string ToString ()
		{
			if (type_str == null)
				MakeString ();
			return type_str;
		}
	}

}

