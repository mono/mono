//
// Mono.ILASM.EventDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All right reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class EventDef : ICustomAttrTarget {

                private FeatureAttr attr;
                private string name;
                private BaseTypeRef type;
                private PEAPI.Event event_def;
                private bool is_resolved;
                private ArrayList customattr_list;

                private MethodRef addon;
                private MethodRef fire;
                private ArrayList other_list;
                private MethodRef removeon;

                public EventDef (FeatureAttr attr, BaseTypeRef type, string name)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;
                        is_resolved = false;
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public PEAPI.Event Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return event_def;

                        type.Resolve (code_gen);
                        event_def = classdef.AddEvent (name, type.PeapiType);

                        if ((attr & FeatureAttr.Rtspecialname) != 0)
                                event_def.SetRTSpecialName ();

                        if ((attr & FeatureAttr.Specialname) != 0)
                                event_def.SetSpecialName ();
                        
                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list)
                                        customattr.AddTo (code_gen, event_def);

                        is_resolved = true;

                        return event_def;
                }

                private PEAPI.MethodDef AsMethodDef (PEAPI.Method method, string type)
                {
                        PEAPI.MethodDef methoddef = method as PEAPI.MethodDef;
                        if (methoddef == null)
                                Report.Error (type + " method of event " + name + " not found");
                        return methoddef;
                }

                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (!is_resolved)
                                Resolve (code_gen, classdef);

                        if (addon != null) {
                                addon.Resolve (code_gen);
                                event_def.AddAddon (AsMethodDef (addon.PeapiMethod, "addon"));
                        }

                        if (fire != null) {
                                fire.Resolve (code_gen);
                                event_def.AddFire (AsMethodDef (fire.PeapiMethod, "fire"));
                        }

                        if (other_list != null) {
				foreach (MethodRef otherm in other_list) {
	                                otherm.Resolve (code_gen);
        	                        event_def.AddOther (AsMethodDef (otherm.PeapiMethod, "other"));
				}
                        }

                        if (removeon != null) {
                                removeon.Resolve (code_gen);
                                event_def.AddRemoveOn (AsMethodDef (removeon.PeapiMethod, "removeon"));
                        }
                }

                public void AddAddon (MethodRef method_ref)
                {
                        addon = method_ref;
                }

                public void AddFire (MethodRef method_ref)
                {
                        fire = method_ref;
                }

                public void AddOther (MethodRef method_ref)
                {
			if (other_list == null)
				other_list = new ArrayList ();
                        other_list.Add (method_ref);
                }

                public void AddRemoveon (MethodRef method_ref)
                {
                        removeon = method_ref;
                }

        }

}

