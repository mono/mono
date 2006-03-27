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

namespace Mono.ILASM {

        public interface IDeclSecurityTarget {
                void AddPermission (PEAPI.SecurityAction sec_action, IPermission iper);
                void AddPermissionSet (PEAPI.SecurityAction sec_action, PermissionSet perm_set);
        }

        public class DeclSecurity {

                private Hashtable permissionset_table;

                public DeclSecurity ()
                {
                        permissionset_table = new Hashtable ();        
                }

                public void AddPermission (PEAPI.SecurityAction sec_action, IPermission perm)
                {
                        PermissionSet ps = (PermissionSet) permissionset_table [sec_action];
                        if (ps == null) {
                                ps = new PermissionSet (PermissionState.None);        
                                permissionset_table [sec_action] = ps;
                        }

                        ps.AddPermission (perm);
                }

                public void AddPermissionSet (PEAPI.SecurityAction sec_action, PermissionSet perm_set)
                {
                        PermissionSet ps = (PermissionSet) permissionset_table [sec_action];
                        if (ps == null) {
                                permissionset_table [sec_action] = perm_set;
                                return;
                        }

                        foreach (IPermission iper in perm_set)
                                ps.AddPermission (iper);
                }
		
                public void AddTo (CodeGen code_gen, PEAPI.MetaDataElement elem)
                {
                        System.Text.UnicodeEncoding ue = new System.Text.UnicodeEncoding ();
                        foreach (PEAPI.SecurityAction sec_action in permissionset_table.Keys)
                                code_gen.PEFile.AddDeclSecurity (sec_action, 
                                        ue.GetBytes (((PermissionSet) permissionset_table [sec_action]).ToXml ().ToString ()), 
                                        elem);
                }

        }

}
