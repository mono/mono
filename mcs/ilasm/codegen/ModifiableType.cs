//
// Mono.ILASM.ModifiableType
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper (Jackson@LatitudeGeo.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class ModifiableType {

                private ArrayList conversion_list;
                private bool is_pinned;
                private bool is_ref;
                private bool is_array;
                private bool use_type_spec;

                private enum ConversionMethod {
                        MakeArray,
                        MakeBoundArray,
                        MakeManagedPointer,
                        MakeUnmanagedPointer,
                        MakeCustomModified
                }

                public ModifiableType ()
                {
                        conversion_list = new ArrayList (5);
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

                public void MakeArray ()
                {
                        use_type_spec = true;
                        conversion_list.Add (ConversionMethod.MakeArray);
                        is_array = true;
                }

                public void MakeBoundArray (ArrayList bounds)
                {
                        use_type_spec = true;
                        conversion_list.Add (ConversionMethod.MakeBoundArray);
                        conversion_list.Add (bounds);
                        is_array = true;
                }

                public void MakeManagedPointer ()
                {
                        use_type_spec = true;
                        conversion_list.Add (ConversionMethod.MakeManagedPointer);
                        is_ref = true;
                }

                public void MakeUnmanagedPointer ()
                {
                        use_type_spec = true;
                        conversion_list.Add (ConversionMethod.MakeUnmanagedPointer);
                }

                public void MakeCustomModified (CodeGen code_gen, PEAPI.CustomModifier modifier,
                                IClassRef klass)
                {
                        use_type_spec = true;
                        conversion_list.Add (ConversionMethod.MakeCustomModified);
                        conversion_list.Add (klass);
                        conversion_list.Add (modifier);
                }

                public void MakePinned ()
                {
                        use_type_spec = true;
                        is_pinned = true;
                }


                protected PEAPI.Type Modify (CodeGen code_gen, PEAPI.Type type, ref string full_name)
                {
                        PeapiTypeRef peapi_type = new PeapiTypeRef (type, full_name);
                        int count = conversion_list.Count;
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

                        full_name = peapi_type.FullName;
                        return peapi_type.PeapiType;
                }

        }

}


