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

                private bool is_resolved;

                public TypeRef (string full_name, Location location)
                {
                        this.full_name = full_name;
                        this.location = location;
                        is_pinned = false;
                        conversion_list = new ArrayList ();
                        is_resolved = false;
                }

                public string FullName {
                        get { return full_name; }
                }

                public bool IsPinned {
                        get { return is_pinned; }
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
                }

                public void MakeBoundArray (ArrayList bounds)
                {
                        conversion_list.Add (ConversionMethod.MakeBoundArray);
                        conversion_list.Add (bounds);
                }

                public void MakeManagedPointer ()
                {
                        conversion_list.Add (ConversionMethod.MakeManagedPointer);
                }

                public void MakeUnmanagedPointer ()
                {
                        conversion_list.Add (ConversionMethod.MakeUnmanagedPointer);
                }

                public void MakeCustomModified (PEAPI.CustomModifier modifier)
                {
                        conversion_list.Add (ConversionMethod.MakeCustomModified);
                        conversion_list.Add (modifier);
                }

                public void MakePinned ()
                {
                        is_pinned = true;
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
                                        peapi_type.MakeCustomModified ((PEAPI.CustomModifier) conversion_list[++i]);
                                        break;
                                }
                        }

                        resolved_type = peapi_type.PeapiType;

                        is_resolved = true;
                }

                public override string ToString ()
                {
                        return FullName;
                }

        }

}

