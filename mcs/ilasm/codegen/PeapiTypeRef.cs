//
// Mono.ILASM.PeapiTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class PeapiTypeRef  {

                private PEAPI.Type peapi_type;
                private string full_name;
                private bool is_pinned;
                private bool is_array;
                private bool is_ref;
                private bool use_type_spec;

                public PeapiTypeRef (PEAPI.Type peapi_type, string full_name)
                {
                        this.peapi_type = peapi_type;
                        this.full_name = full_name;
                        is_pinned = false;
                        is_array = false;
                        is_ref = false;
                        use_type_spec = false;
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

                public bool UseTypeSpec {
                        get { return use_type_spec; }
                }

                public PEAPI.Type PeapiType {
                        get { return peapi_type; }
                }

                public void MakeArray ()
                {
                        use_type_spec = true;

                        if (peapi_type is PEAPI.Class) {
                                PEAPI.Class klass = (PEAPI.Class) peapi_type;
                                peapi_type = klass.GetZeroBasedArray ();
                        } else {
                                peapi_type = new PEAPI.ZeroBasedArray (peapi_type);
                        }
                        full_name += "[]";
                        is_array = true;
                }

                public void MakeBoundArray (ArrayList bound_list)
                {
                        use_type_spec = true;

                        int dimen = bound_list.Count;
                        int[] lower_array = new int[dimen];
                        int[] size_array = new int[dimen];
                        bool lower_set = false;
                        bool size_set = false;
                        bool prev_lower_set = true;
                        bool prev_size_set = true;

                        // TODO: There should probably be an error reported if
                        // something like [3...,3...5] is done
                        for (int i=0; i<dimen; i++) {
                                DictionaryEntry bound = (DictionaryEntry) bound_list[i];

                                if (bound.Key != TypeRef.Ellipsis && prev_lower_set) {
                                        lower_array[i] = (int) bound.Key;
                                        lower_set = true;
                                } else {
                                        prev_lower_set = false;
                                }
                                if (bound.Value != TypeRef.Ellipsis && prev_size_set) {
                                        size_array[i] = (int) bound.Value;
                                        size_set = true;
                                } else {
                                        prev_size_set = false;
                                }
                        }
                        if (lower_set && size_set) {
                                peapi_type = new PEAPI.BoundArray (peapi_type,
                                                (uint) dimen, lower_array, size_array);
                        } else if (size_set) {
                                peapi_type = new PEAPI.BoundArray (peapi_type,
                                                (uint) dimen, size_array);
                        } else {
                                peapi_type = new PEAPI.BoundArray (peapi_type, (uint) dimen);
                        }
                        /// TODO: Proper full names
                        full_name += "[][]";
                        is_array = true;
                }

                public void MakeManagedPointer ()
                {
                        use_type_spec = true;

                        peapi_type = new PEAPI.ManagedPointer (peapi_type);
                        full_name += "&";
                        is_ref = true;
                }

                public void MakeUnmanagedPointer ()
                {
                        use_type_spec = true;

                        peapi_type = new PEAPI.UnmanagedPointer (peapi_type);
                        full_name += "*";
                }

                public void MakeCustomModified (CodeGen code_gen, PEAPI.CustomModifier modifier,
                                IClassRef klass)
                {
                        use_type_spec = true;

                        klass.Resolve (code_gen);
                        peapi_type = new PEAPI.CustomModifiedType (peapi_type,
                                        modifier, klass.PeapiClass);
                }

                public void MakePinned ()
                {
                        use_type_spec = true;
                        is_pinned = true;
                }

                public void Resolve (CodeGen code_gen)
                {

                }


        }

}

