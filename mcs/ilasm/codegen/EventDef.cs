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

        public class EventDef {

                private FeatureAttr attr;
                private string name;
                private ITypeRef type;
                private PEAPI.Event event_def;
                private bool is_resolved;

                private ArrayList addon_list;
                private ArrayList fire_list;
                private ArrayList other_list;
                private ArrayList removeon_list;

                public EventDef (FeatureAttr attr, ITypeRef type, string name)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;
                        is_resolved = false;
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

                        is_resolved = true;

                        return event_def;
                }

                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (!is_resolved)
                                Resolve (code_gen, classdef);

                        if (addon_list != null) {
                                foreach (MethodRef methodref in addon_list) {
                                        methodref.Resolve (code_gen);
                                        event_def.AddAddon ((PEAPI.MethodDef) methodref.PeapiMethod);
                                }
                        }

                        if (fire_list != null) {
                                foreach (MethodRef methodref in fire_list) {
                                        methodref.Resolve (code_gen);
                                        event_def.AddFire ((PEAPI.MethodDef) methodref.PeapiMethod);
                                }
                        }

                        if (other_list != null) {
                                foreach (MethodRef methodref in other_list) {
                                        methodref.Resolve (code_gen);
                                        event_def.AddOther ((PEAPI.MethodDef) methodref.PeapiMethod);
                                }
                        }

                        if (removeon_list != null) {
                                foreach (MethodRef methodref in removeon_list) {
                                        methodref.Resolve (code_gen);
                                        event_def.AddRemoveOn ((PEAPI.MethodDef) methodref.PeapiMethod);
                                }
                        }
                }

                public void AddAddon (MethodRef method_ref)
                {
                        if (addon_list == null)
                                addon_list = new ArrayList ();

                        addon_list.Add (method_ref);
                }

                public void AddFire (MethodRef method_ref)
                {
                        if (fire_list == null)
                                fire_list = new ArrayList ();

                        fire_list.Add (method_ref);
                }

                public void AddOther (MethodRef method_ref)
                {
                        if (other_list == null)
                                other_list = new ArrayList ();

                        other_list.Add (method_ref);
                }

                public void AddRemoveon (MethodRef method_ref)
                {
                        if (removeon_list == null)
                                removeon_list = new ArrayList ();

                        removeon_list.Add (method_ref);
                }

        }

}

