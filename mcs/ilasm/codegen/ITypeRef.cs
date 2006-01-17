//
// Mono.ILASM.BaseTypeRef
//
// Interface that all Type references must implement
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public interface ITypeRef {

                /// <summary>
                ///  The full name of the type, <namespace>.<name>
                /// ie System.Collections.ArrayList
                /// </summary>
                string FullName { get; }

                bool IsPinned { get; }
                bool IsRef { get; }
                bool IsArray { get; }
                bool UseTypeSpec { get; }

                /// <summary>
                ///  The PEAPI.Type of this typeref, this is not guaranteed
                ///  to be correct untill after this is resolved.
                /// </summary>
                PEAPI.Type PeapiType { get; }

                /// <summary>
                ///  Convert this typeref into a zero based array
                /// </summary>
                void MakeArray ();

                /// <summary>
                ///  Convert this typeref into a bound array. The ArrayList
                ///  should be in the format Entry (lower_bound, upper_bound) with
                ///  null being used for unset bounds.
                /// </summary>
                void MakeBoundArray (ArrayList bounds);

                /// <summary>
                ///  Convert this typeref into a reference
                /// </summary>
                void MakeManagedPointer ();

                /// <summary>
                ///  Convert this typeref into an unmanaged pointer
                /// </summary>
                void MakeUnmanagedPointer ();

                /// <summary>
                ///  Convert this typeref into a CustomModifiedType
                /// </summary>
                void MakeCustomModified (CodeGen code_gen,
                                PEAPI.CustomModifier modifier, IClassRef klass);

                /// <summary>
                ///  Make this typeref pinned.
                /// </summary>
                void MakePinned ();

                void Resolve (CodeGen code_gen);

                BaseMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param, int gen_param_count);

                IFieldRef GetFieldRef (ITypeRef ret_type, string name);

                /// <summary>
                ///  Convert this typeref to a classref if not possible
                ///  throw an exception
                /// </summary>
                // IClassRef AsClassRef (CodeGen code_gen);
        }

}

