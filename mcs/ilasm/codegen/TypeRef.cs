//
// Mono.ILASM.TypeRef
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
        /// Reference to a type in the module being compiled.
        /// </summary>
        public class TypeRef : IClassRef {

                private enum ConversionMethod {
                        MakeArray,
                        MakeBoundArray,
                        MakeManagedPointer,
                        MakeUnmanagedPointer,
                        MakeCustomModified
                }

                private Location location;
                private string full_name;
                private PEAPI.Type resolved_type;
                private ArrayList conversion_list;
                private bool is_pinned;
                private bool is_ref;
                private bool is_array;

                private bool is_resolved;

                public static readonly TypeRef Ellipsis = new TypeRef ("ELLIPSIS", null);
                public static readonly TypeRef Any = new TypeRef ("any", null);

                public TypeRef (string full_name, Location location)
                {
                        this.full_name = full_name;
                        this.location = location;
                        is_pinned = false;
                        is_ref = false;
                        is_array = false;
                        conversion_list = new ArrayList ();
                        is_resolved = false;
                }

                public string FullName {
                        get { return full_name; }
                }

                public bool IsPinned {
                        get { return is_pinned; }
                }

                public bool IsArray {
                        get { return is_array; }
                }

                public bool IsRef {
                        get { return is_ref; }
                }

                public PEAPI.Type PeapiType {
                        get { return resolved_type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return resolved_type as PEAPI.Class; }
                }

                public bool IsResolved {
                        get { return is_resolved; }
                }

                public void MakeArray ()
                {
                        conversion_list.Add (ConversionMethod.MakeArray);
                        is_array = true;
                }

                public void MakeBoundArray (ArrayList bounds)
                {
                        conversion_list.Add (ConversionMethod.MakeBoundArray);
                        conversion_list.Add (bounds);
                        is_array = true;
                }

                public void MakeManagedPointer ()
                {
                        conversion_list.Add (ConversionMethod.MakeManagedPointer);
                        is_ref = true;
                }

                public void MakeUnmanagedPointer ()
                {
                        conversion_list.Add (ConversionMethod.MakeUnmanagedPointer);
                }

                public void MakeCustomModified (CodeGen code_gen, PEAPI.CustomModifier modifier,
                                IClassRef klass)
                {
                        conversion_list.Add (ConversionMethod.MakeCustomModified);
                        conversion_list.Add (klass);
                        conversion_list.Add (modifier);
                }

                public void MakePinned ()
                {
                        is_pinned = true;
                }

                public  IMethodRef GetMethodRef (ITypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, ITypeRef[] param)
                {
                        return new MethodRef (this, call_conv, ret_type, name, param);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new FieldRef (this, ret_type, name);
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type base_type;
                        PeapiTypeRef peapi_type;
                        int count = conversion_list.Count;

                        base_type = code_gen.TypeManager.GetPeapiType (full_name);

                        /// TODO: Proper error message
                        if (base_type == null) {
                                Console.WriteLine ("Type not defined: {0} {1}", full_name, location);
                                return;
                        }

                        peapi_type = new PeapiTypeRef (base_type, full_name);

                        for (int i=0; i<count; i++) {
                                switch ((ConversionMethod) conversion_list[i]) {
                                case ConversionMethod.MakeArray:
                                        peapi_type.MakeArray ();
                                        break;
                                case ConversionMethod.MakeBoundArray:
                                        peapi_type.MakeBoundArray ((ArrayList) conversion_list[++i]);
                                        break;
                                case ConversionMethod.MakeManagedPointer:
                                        peapi_type.MakeManagedPointer ();
                                        break;
                                case ConversionMethod.MakeUnmanagedPointer:
                                        peapi_type.MakeUnmanagedPointer ();
                                        break;
                                case ConversionMethod.MakeCustomModified:
                                        peapi_type.MakeCustomModified (code_gen, (PEAPI.CustomModifier) conversion_list[++i],
                                                (IClassRef) conversion_list[++i]);
                                        break;
                                }
                        }

                        resolved_type = peapi_type.PeapiType;

                        is_resolved = true;
                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        return this;
                }

                public override string ToString ()
                {
                        return FullName;
                }

        }

}

