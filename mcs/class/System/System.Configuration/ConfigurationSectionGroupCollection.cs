//
// System.Configuration.ConfigurationSectionGroupCollection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System;
using System.Collections;
using System.Collections.Specialized;

namespace System.Configuration {

        public sealed class ConfigurationSectionGroupCollection : NameObjectCollectionBase
        {
                public ICollection AllKeys {
                        get { return BaseGetAllKeys (); }
                }

                public override int Count {
                        get { throw new NotImplementedException (); }
                }

                public ConfigurationSectionGroup this [int index] {
                        get { throw new NotImplementedException (); }
                }

                public ConfigurationSectionGroup this [string index] {
                        get { throw new NotImplementedException (); }                        
                }

                public override NameObjectCollectionBase.KeysCollection Keys {
                        get { throw new NotImplementedException (); }
                }

                public void Add (string name, ConfigurationSectionGroup section_group)
                {
                        BaseAdd (name, section_group);
                }

                public void Clear ()
                {
                        BaseClear ();
                }

                public void CopyTo (ConfigurationSectionGroup [] array, int index)
                {
                        throw new NotImplementedException ();
                }

                public ConfigurationSectionGroup Get (int index)
                {
                        return BaseGet (index) as ConfigurationSectionGroup;
                }

                public ConfigurationSectionGroup Get (string index)
                {
                        return BaseGet (index) as ConfigurationSectionGroup;
                }

                public override IEnumerator GetEnumerator ()
                {
                        throw new NotImplementedException ();
                }

                public string GetKey (string index)
                {
                        throw new NotImplementedException ();
                }

                public void Remove (string index)
                {
                        BaseRemove (index);
                }

                public void RemoveAt (int index)
                {
                        BaseRemoveAt (index);
                }
        }
}
#endif
