//
// Mono.ILASM.PermissionMember
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

using System.Reflection;

namespace Mono.ILASM {

        public class PermissionMember {

                MemberTypes member_type;
                BaseTypeRef type_ref;
                string name;
                object value;

                PEAPI.PermissionMember member;
                
                public PermissionMember (MemberTypes member_type, BaseTypeRef type_ref, string name, object value)
                {
                        this.member_type = member_type;
                        this.type_ref = type_ref;
                        this.name = name;
                        this.value = value;
                }

                public PEAPI.PermissionMember Resolve (CodeGen code_gen)
                {
                        type_ref.Resolve (code_gen);

                        member = new PEAPI.PermissionMember (member_type, type_ref.PeapiType, name, value);

                        return member;
                }
        }

}
