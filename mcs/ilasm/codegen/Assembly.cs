//
// Mono.ILASM.Assembly
//
// Represents .assembly { }
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


using System.Collections;

namespace Mono.ILASM {

        public class Assembly : IDeclSecurityTarget, ICustomAttrTarget 
        {
                private DeclSecurity decl_sec;
                private ArrayList customattr_list;

                private string name;
                private byte [] public_key;
                private int major_version;
                private int minor_version;
                private int build_version;
                private int revision_version;
                private string locale;
                private int hash_algorithm;
                private PEAPI.AssemAttr attr;

                public Assembly (string name)
                {
                        this.name = name;
                }

                public string Name {
                        get { return name; }
                }

                public DeclSecurity DeclSecurity {
                        get { 
                                if (decl_sec == null)
                                        decl_sec = new DeclSecurity ();
                                return decl_sec; 
                        }
                }

                public void SetVersion (int major, int minor, int build, int revision)
                {
                        this.major_version = major;
                        this.minor_version = minor;
                        this.build_version = build;
                        this.revision_version = revision;
                }

                public void SetLocale (string locale)
                {
                        this.locale = locale;
                }

                public void SetHashAlgorithm (int algorithm)
                {
                        hash_algorithm = algorithm;
                }

                public void SetPublicKey (byte [] public_key)
                {
                        this.public_key = public_key;
                }

                public void SetAssemblyAttr (PEAPI.AssemAttr attr)
                {
                        this.attr = attr;
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public void Resolve (CodeGen code_gen, PEAPI.Assembly asm)
                {
                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list) {
                                        customattr.AddTo (code_gen, asm);
                                }
                        
                        if (decl_sec != null)
				decl_sec.AddTo (code_gen, asm);


                        asm.AddAssemblyInfo(major_version,
                                minor_version, build_version,
                                revision_version, public_key,
                                (uint) hash_algorithm, locale);

                        asm.AddAssemblyAttr (attr);
                }
        }
}
