//
// Mono.ILASM.Permission
//
// Author(s):
//  Ankit Jain  <JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public class Permission
        {
                BaseTypeRef type_ref;
                
                //PermissionMembers
                ArrayList members;
                PEAPI.Permission perm;

                public Permission (BaseTypeRef type_ref, ArrayList members)
                {
                        this.type_ref = type_ref;
                        this.members = members;
                }

                public PEAPI.Permission Resolve (CodeGen code_gen)
                {
                        string fname;

                        type_ref.Resolve (code_gen);

                        if (type_ref is ExternTypeRef) {
                                ExternAssembly ea = ((ExternTypeRef) type_ref).ExternRef as ExternAssembly;
                                if (ea == null)
                                        //FIXME: module.. ?
                                        throw new NotImplementedException ();

                                string name;
                                ExternTypeRef etr = type_ref as ExternTypeRef;
                                if (etr != null)
                                        name = etr.Name;
                                else
                                        name = type_ref.FullName;

                                fname = String.Format ("{0}, {1}", name, ea.AssemblyName.FullName);
                        } else {
                                fname = type_ref.FullName;
                        }

                        perm = new PEAPI.Permission (type_ref.PeapiType, fname);
                                        
                        foreach (PermissionMember member in members)
                                perm.AddMember (member.Resolve (code_gen));

                        return perm;
                }
        }

}
