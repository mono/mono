//
// Mono.ILASM.PrimitiveTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        /// <summary>
        /// Reference to a primitive type, ie string, object, char
        /// </summary>
        public class PrimitiveTypeRef : BaseTypeRef {

                private static Hashtable s_method_table = new Hashtable ();

                public PrimitiveTypeRef (PEAPI.PrimitiveType type, string full_name)
                        : this (type, full_name, null, String.Empty)
                {
                }

                public PrimitiveTypeRef (PEAPI.PrimitiveType type, string full_name, ArrayList conv_list, string sig_mod)
                        : base (full_name, conv_list, sig_mod)
                {
                        this.type = type;
                        if (SigMod == null)
                                SigMod = String.Empty;
                }

                public override BaseTypeRef Clone ()
                {
                        return new PrimitiveTypeRef ((PEAPI.PrimitiveType) type, full_name,
                                (ArrayList) ConversionList.Clone (), sig_mod);
                }

		public string Name {
			get { return full_name; }
		}

                public override void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        // Perform all of the types modifications
                        type = Modify (code_gen, type);

                        is_resolved = true;
                }

                /// <summary>
                /// Primitive types can be created like this System.String instead
                /// of like a normal type that would be [mscorlib]System.String This
                /// method returns a proper primitive type if the supplied name is
                /// the name of a primitive type.
                /// </summary>
                public static PrimitiveTypeRef GetPrimitiveType (string full_name)
                {
                        switch (full_name) {
                        case "System.String":
                        case "[System.Runtime]System.String":
                                return new PrimitiveTypeRef (PEAPI.PrimitiveType.String, full_name);
                        case "[System.Runtime]System.Object":
                        case "System.Object":
                                return new PrimitiveTypeRef (PEAPI.PrimitiveType.Object, full_name);
                        default:
                                return null;
                        }
                }

                protected override BaseMethodRef CreateMethodRef (BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count)
                {
                        throw new InternalErrorException ("Should not be called");
                }

                public override BaseMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        /* Use FullName also here, as we are caching in a static hashtable */
                        string key = FullName + MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, true);
                        TypeSpecMethodRef mr = s_method_table [key] as TypeSpecMethodRef;
                        if (mr != null)
                                return mr;

			//FIXME: generic methodref for primitive type?
                        mr = new TypeSpecMethodRef (this, call_conv, ret_type, name, param, gen_param_count);
                        s_method_table [key] = mr;
                        return mr;
                }

                protected override IFieldRef CreateFieldRef (BaseTypeRef ret_type, string name)
                {
                        Report.Error ("PrimitiveType's can't have fields!");
                        return null;
                }

                public BaseClassRef AsClassRef (CodeGen code_gen)
                {
                        /*
                        PEAPI.ClassRef class_ref = code_gen.ExternTable.GetValueClass ("corlib", FullName);
                        ExternTypeRef type_ref = new ExternTypeRef (class_ref, FullName);

                        // TODO: Need to do the rest of the conversion (in order)
                        if (IsArray)
                                type_ref.MakeArray ();

                        return type_ref;
                        */
                        throw new NotImplementedException ("This method is getting depricated.");
                }

        }

}

