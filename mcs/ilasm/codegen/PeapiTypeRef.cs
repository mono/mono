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
        public class Pair {
                protected PEAPI.Type type;
                protected string sig;

                public Pair (PEAPI.Type type, string sig)
                {
                        this.type = type;
                        this.sig = sig;
                }

                public override int GetHashCode ()
                {
                        return type.GetHashCode () ^ sig.GetHashCode (); 
                }

                public override bool Equals (Object o)
                {
                        Pair p = o as Pair;

                        if (p == null)
                                return false;
                        
                        return (p.type == this.type && p.sig == this.sig);
                }
        }

        class Triplet : Pair
        {
                string typeMod;

                public Triplet (PEAPI.Type type, string sig, string typeMod)
                        : base (type, sig)
                {
                        this.typeMod = typeMod;
                }

                public override int GetHashCode ()
                {
                        return base.GetHashCode () ^ typeMod.GetHashCode ();
                }

                public override bool Equals (object o)
                {
                        Triplet t = o as Triplet;
                        if (t == null)
                                return false;

                        return t.type == type && t.sig == sig && t.typeMod == typeMod;
                }
        }

        public class PeapiTypeRef  {

                private PEAPI.Type peapi_type;
                private bool is_pinned;
                private bool is_array;
                private bool is_ref;
                private bool use_type_spec;

                private static Hashtable type_table = new Hashtable ();

                public PeapiTypeRef (PEAPI.Type peapi_type)
                {
                        this.peapi_type = peapi_type;
                        is_pinned = false;
                        is_array = false;
                        is_ref = false;
                        use_type_spec = false;
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
                        PEAPI.Type type;

                        use_type_spec = true;
                        is_array = true;

                        Pair p = new Pair (peapi_type, "[]");
                        type = type_table [p] as PEAPI.Type;
                        if (type == null) {
                                type = new PEAPI.ZeroBasedArray (peapi_type);
                                type_table [p] = type;
                        }
                        peapi_type = type;
                }

                public void MakeBoundArray (ArrayList bound_list)
                {
                        use_type_spec = true;
                        is_array = true;

                        int dimen = bound_list.Count;
                        int[] lower_array = new int[dimen];
                        int[] size_array = new int[dimen];
                        int [] lobounds = null;
                        int [] sizes = null;
                        int num_lower, num_sizes;
                        string sigmod = "";
                        PEAPI.Type type;
                        Pair p;

                        sigmod += "[";
                        for (int i=0; i<bound_list.Count; i++) {
                                DictionaryEntry e = (DictionaryEntry) bound_list [i];
                                if (e.Key != TypeRef.Ellipsis)
                                        sigmod += e.Key;
                                sigmod += "...";
                                if (e.Value != TypeRef.Ellipsis)
                                        sigmod += e.Value;
                                if (i + 1 < bound_list.Count)
                                        sigmod += ", ";
                        }
                        sigmod += "]";

                        p = new Pair (peapi_type, sigmod);
                        type = type_table [p] as PEAPI.Type;
                        if (type != null) {
                                peapi_type = type;
                                return;
                        }

                        num_sizes = num_lower = 0;
                        // TODO: There should probably be an error reported if
                        // something like [3...,3...5] is done
                        for (int i=0; i<dimen; i++) {
                                if (bound_list [i] == null)
                                        continue;
                                        
                                DictionaryEntry bound = (DictionaryEntry) bound_list [i];
                                
                                if (bound.Key != TypeRef.Ellipsis) {
                                        /* Lower bound specified */
                                        lower_array [i] = (int) bound.Key;
                                        num_lower = i + 1;
                                }
                                if (bound.Value != TypeRef.Ellipsis) {
                                        size_array [i] = (int) bound.Value;
                                        if (bound.Key != TypeRef.Ellipsis)
                                                /* .Value is Upper bound eg [1...5] */
                                                size_array [i] -= lower_array [i] - 1;
                                        num_sizes = i + 1;
                                }
                        }

                        if (num_lower > 0) {
                                lobounds = new int [num_lower];
                                Array.Copy (lower_array, lobounds, num_lower);
                        }

                        if (num_sizes > 0) {
                                sizes = new int [num_sizes];
                                Array.Copy (size_array, sizes, num_sizes);
                        }

                        peapi_type = new PEAPI.BoundArray (peapi_type,
                                                (uint) dimen, lobounds, sizes);
                        type_table [p] = peapi_type;
                }

                public void MakeManagedPointer ()
                {
                        PEAPI.Type type;
                        use_type_spec = true;
                        is_ref = true;

                        Pair p = new Pair (peapi_type, "&");
                        type = type_table [p] as PEAPI.Type;
                        if (type == null) {
                                type = new PEAPI.ManagedPointer (peapi_type);
                                type_table [p] = type;
                        }
                        peapi_type = type;
                }

                public void MakeUnmanagedPointer ()
                {
                        PEAPI.Type type;
                        use_type_spec = true;

                        Pair p = new Pair (peapi_type, "*");
                        type = type_table [p] as PEAPI.Type;
                        if (type == null) {
                                type = new PEAPI.UnmanagedPointer (peapi_type);
                                type_table [p] = type;
                        }
                        peapi_type = type;
                }

                public void MakeCustomModified (CodeGen code_gen, PEAPI.CustomModifier modifier,
                                BaseTypeRef klass)
                {
			PEAPI.Type type;

                        use_type_spec = true;
                        
                        Pair p = new Triplet (peapi_type, modifier.ToString (), klass.FullName);
                        type = type_table [p] as PEAPI.Type;
                        if (type == null) {
                                type = GetType (code_gen, modifier, klass);
                                type_table [p] = type;
                        }
                        peapi_type = type;
                }

                PEAPI.Type GetType (CodeGen code_gen, PEAPI.CustomModifier modifier, BaseTypeRef klass)
                {
                        klass.Resolve (code_gen);
                        var bcr = klass as BaseClassRef;
                        if (bcr != null)
                                return new PEAPI.CustomModifiedType (peapi_type, modifier, bcr.PeapiClass);

                        var pt = klass as PrimitiveTypeRef;
                                return new PEAPI.CustomModifiedType (peapi_type, modifier, code_gen.PEFile.AddPrimitiveType ((PEAPI.PrimitiveType) pt.PeapiType));

                        throw new NotSupportedException (klass.GetType ().ToString ());
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

