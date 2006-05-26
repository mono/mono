//
// Mono.ILASM.DeclSecurity
//
// Author(s):
//  Ankit Jain  <JAnkit@novell.com>
//
// (C) 2005 Ankit Jain, All rights reserved
//


using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;

using SSPermissionSet = System.Security.PermissionSet;
using MIPermissionSet = Mono.ILASM.PermissionSet;

namespace Mono.ILASM {

        public interface IDeclSecurityTarget {
                DeclSecurity DeclSecurity { get; }
        }

        public class DeclSecurity {

                private Hashtable permissionset_table;
                private Hashtable permissionset20_table;

                public DeclSecurity ()
                {
                        permissionset_table = new Hashtable ();        
                }

                public void AddPermission (PEAPI.SecurityAction sec_action, IPermission perm)
                {
                        SSPermissionSet ps = (SSPermissionSet) permissionset_table [sec_action];
                        if (ps == null) {
                                ps = new SSPermissionSet (PermissionState.None);        
                                permissionset_table [sec_action] = ps;
                        }

                        ps.AddPermission (perm);
                }

                public void AddPermissionSet (PEAPI.SecurityAction sec_action, SSPermissionSet perm_set)
                {
                        SSPermissionSet ps = (SSPermissionSet) permissionset_table [sec_action];
                        if (ps == null) {
                                permissionset_table [sec_action] = perm_set;
                                return;
                        }

                        foreach (IPermission iper in perm_set)
                                ps.AddPermission (iper);
                }

                //Not called by parser for profile != NET_2_0
                public void AddPermissionSet (PEAPI.SecurityAction sec_action, MIPermissionSet perm_set)
                {
                        PermissionSet ps = null;

                        if (permissionset20_table == null)
                                permissionset20_table = new Hashtable ();
                        else
                                ps = (MIPermissionSet) permissionset20_table [sec_action];

                        if (ps == null) {
                                permissionset20_table [sec_action] = perm_set;
                                return;
                        }

                        foreach (Permission perm in perm_set.Permissions)
                                ps.AddPermission (perm);
                }

                public void AddTo (CodeGen code_gen, PEAPI.MetaDataElement elem)
                {
                        System.Text.UnicodeEncoding ue = new System.Text.UnicodeEncoding ();
                        foreach (DictionaryEntry entry in permissionset_table) {
                                PEAPI.SecurityAction sec_action = (PEAPI.SecurityAction) entry.Key;
                                SSPermissionSet ps = (SSPermissionSet) entry.Value;

                                code_gen.PEFile.AddDeclSecurity (sec_action,
                                        ue.GetBytes (ps.ToXml ().ToString ()), 
                                         elem);
                        }

                        if (permissionset20_table == null)
                                return;

                        foreach (DictionaryEntry entry in permissionset20_table) {
                                PEAPI.SecurityAction sec_action = (PEAPI.SecurityAction) entry.Key;
                                MIPermissionSet ps = (MIPermissionSet) entry.Value;

                                code_gen.PEFile.AddDeclSecurity (sec_action,
                                        ps.Resolve (code_gen), 
                                        elem);
                        }

                }

        }

}
