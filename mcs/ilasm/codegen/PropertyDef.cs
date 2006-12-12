//
// Mono.ILASM.PropertyDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All right reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class PropertyDef : ICustomAttrTarget {

                private FeatureAttr attr;
                private string name;
                private BaseTypeRef type;
                private ArrayList arg_list;
                private PEAPI.Property prop_def;
                private bool is_resolved;
                private ArrayList customattr_list;

                private MethodRef _get;
                private MethodRef _set;
                private ArrayList other_list;
                private PEAPI.Constant init_value;

                public PropertyDef (FeatureAttr attr, BaseTypeRef type, string name, ArrayList arg_list)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;
                        this.arg_list = arg_list;
                        is_resolved = false;
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public PEAPI.Property Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return prop_def;

                        PEAPI.Type[] type_list = new PEAPI.Type[arg_list.Count];

                        for (int i=0; i<type_list.Length; i++) {
                                BaseTypeRef arg_type = (BaseTypeRef) arg_list[i];
                                arg_type.Resolve (code_gen);
                                type_list[i] = arg_type.PeapiType;
                        }

                        type.Resolve (code_gen);
                        prop_def = classdef.AddProperty (name, type.PeapiType, type_list);

                        if ((attr & FeatureAttr.Rtspecialname) != 0)
                                prop_def.SetRTSpecialName ();

                        if ((attr & FeatureAttr.Specialname) != 0)
                                prop_def.SetSpecialName ();

                        prop_def.SetInstance ((attr & FeatureAttr.Instance) != 0);

                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list)
                                        customattr.AddTo (code_gen, prop_def);


                        is_resolved = true;

                        return prop_def;
                }
                
                private PEAPI.MethodDef AsMethodDef (PEAPI.Method method, string type)
                {
                        PEAPI.MethodDef methoddef = method as PEAPI.MethodDef;
                        if (methoddef == null)
                                Report.Error (type + " method of property " + name + " not found");
                        return methoddef;
                }

                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (!is_resolved)
                                Resolve (code_gen, classdef);

                        if (_get != null) {
                                _get.Resolve (code_gen);
                                prop_def.AddGetter (AsMethodDef (_get.PeapiMethod, "get"));
                        }

                        if (_set != null) {
                                _set.Resolve (code_gen);
                                prop_def.AddSetter (AsMethodDef (_set.PeapiMethod, "set"));
                        }

                        if (other_list != null) {
				foreach (MethodRef otherm in other_list) {
	                                otherm.Resolve (code_gen);
        	                        prop_def.AddOther (AsMethodDef (otherm.PeapiMethod, "other"));
				}
                        }

                        if (init_value != null)
                                prop_def.AddInitValue (init_value);
                }

                public void AddGet (MethodRef _get)
                {
                        this._get = _get;
                }

                public void AddSet (MethodRef _set)
                {
                        this._set = _set;
                }

                public void AddOther (MethodRef other)
                {
			if (other_list == null)
				other_list = new ArrayList ();
                        other_list.Add (other);
                }

                public void AddInitValue (PEAPI.Constant init_value)
                {
                        this.init_value = init_value;
                }
        }

}

