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

                private MethodRef addon;
                private MethodRef fire;
                private MethodRef other;
                private MethodRef removeon;

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

                        if (addon != null) {
                                addon.Resolve (code_gen);
                                event_def.AddAddon ((PEAPI.MethodDef) addon.PeapiMethod);
                        }

                        if (fire != null) {
                                fire.Resolve (code_gen);
                                event_def.AddFire ((PEAPI.MethodDef) fire.PeapiMethod);
                        }

                        if (other != null) {
                                other.Resolve (code_gen);
                                event_def.AddOther ((PEAPI.MethodDef) other.PeapiMethod);
                        }

                        if (removeon != null) {
                                removeon.Resolve (code_gen);
                                event_def.AddRemoveOn ((PEAPI.MethodDef) removeon.PeapiMethod);
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
                        other = method_ref;
                }

                public void AddRemoveon (MethodRef method_ref)
                {
                        removeon = method_ref;
                }

        }

}

