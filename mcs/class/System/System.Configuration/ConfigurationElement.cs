//
// System.Configuration.ConfigurationElement.cs
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
#if XML_DEP
using System.Collections;
using System.Xml;

namespace System.Configuration
{
        public abstract class ConfigurationElement
        {
                protected ConfigurationElement ()
                {
                }

                protected internal virtual ConfigurationPropertyCollection CollectionKeyProperties {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                protected internal object this [ConfigurationProperty property] {
                        get {
                                throw new NotImplementedException ();
                        }

                        set {
                                throw new NotImplementedException ();
                        }
                }

                protected internal object this [string property_name] {
                        get {
                                throw new NotImplementedException ();
                        }

                        set {
                                throw new NotImplementedException ();
                        }
                }

                protected internal virtual ConfigurationPropertyCollection Properties {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public override bool Equals (object compareTo)
                {
                        throw new NotImplementedException ();
                }

                public override int GetHashCode ()
                {
                        throw new NotImplementedException ();
                }

                public bool HasValue (string key)
                {
                        throw new NotImplementedException ();
                }

                public string PropertyFileName ()
                {
                        throw new NotImplementedException ();
                }

                public int PropertyLineNumber ()
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void Deserialize (
                        XmlReader reader, bool serialize_collection_key)
                {
                        throw new NotImplementedException ();
                }

                protected virtual bool HandleUnrecognizedAttribute (
                        string name, string value)
                {
                        throw new NotImplementedException ();
                }

                protected virtual bool HandleUnrecognizedElement (
                        string element, XmlReader reader)
                {
                        throw new NotImplementedException ();
                }

                protected internal void InitializeDefault ()
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual bool IsModified ()
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void ReadXml (XmlReader reader, object context)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void Reset (
                        ConfigurationElement parent_element, object context)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void ResetModified ()
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual bool Serialize (
                        XmlWriter writer, bool serialize_collection_key)
                {
                        throw new NotImplementedException ();
                }
                        
                protected internal virtual bool SerializeAttributeOnRemove (
                        ConfigurationProperty property)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual bool SerializeToXmlElement (
                        XmlWriter writer, string element_name)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void UnMerge (
                        ConfigurationElement source, ConfigurationElement parent,
                        bool serialize_collection_key, object context,
                        ConfigurationUpdateMode update_mode)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual void ValidateRequiresProperties (
                        ConfigurationPropertyCollection properties,
                        bool serialize_collection_key)
                {
                        throw new NotImplementedException ();
                }

                protected internal virtual string WriteXml (
                        ConfigurationElement parent,
                        object context, string name,
                        ConfigurationUpdateMode update_mode)
                {
                        throw new NotImplementedException ();
                }
        }
}
#endif
#endif
