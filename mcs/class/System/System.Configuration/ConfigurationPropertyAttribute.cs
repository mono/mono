//
// System.Configuration.ConfigurationPropertyAttribute.cs
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

namespace System.Configuration
{
        public sealed class ConfigurationPropertyAttribute : Attribute
        {
                string name;
                bool collection_key, default_collection_property, required_value;
                object default_value;
                ConfigurationPropertyFlags flags;
                
                public ConfigurationPropertyAttribute (string name)
                {
                        this.name = name;
                }

                public bool CollectionKey {
                        get { return collection_key; }
                        set { collection_key = value; }
                }

                public bool DefaultCollectionProperty {
                        get { return default_collection_property; }
                        set { default_collection_property = value; }
                }

                public object DefaultValue {
                        get { return default_value; }
                        set { default_value = value; }
                }
                
                public ConfigurationPropertyFlags Flags {
                        get { return flags; }
                        set { flags = value; }
                }

                public string Name {
                        get { return name; }
                        set { name = value; }
                }

                public bool RequiredValue {
                        get { return required_value; }
                        set { required_value = value; }
                }                        
        }
}
#endif
