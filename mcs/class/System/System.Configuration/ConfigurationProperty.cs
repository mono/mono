//
// System.Configuration.ConfigurationProperty.cs
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

#if NET_2_0 && XML_DEP
#if XML_DEP
using System;
using System.ComponentModel;

namespace System.Configuration
{
        public class ConfigurationProperty : ConfigurationElement
        {
                string name;
                Type type;
                object default_value;
                TypeConverter converter;
                ConfigurationValidationAttribute validation;
                ConfigurationPropertyFlags flags;
                
                public ConfigurationProperty (string name, Type type, object default_value)
                        : this (name, type, default_value, ConfigurationPropertyFlags.None)
                {
                }

                public ConfigurationProperty (
                                string name, Type type, object default_value,
                                ConfigurationPropertyFlags flags)
                        :this (name, type, default_value, TypeDescriptor.GetConverter (type), null, flags)
                {
                }

                public ConfigurationProperty (
                                string name, Type type, object default_value,
                                TypeConverter converter,
                                ConfigurationValidationAttribute validation,
                                ConfigurationPropertyFlags flags)
                {
                        this.name = name;
                        this.converter = converter;
                        this.default_value = default_value;
                        this.flags = flags;
                        this.type = type;
                        this.validation = validation;
                }

                public TypeConverter Converter {
                        get { return converter; }
                }

                public object DefaultValue {                        
                        get { return default_value; }
                        
                }

                public bool IsKey {                        
                        get { return (flags & ConfigurationPropertyFlags.IsKey) != 0; }
                }

                public bool IsRequired {
                        get { return (flags & ConfigurationPropertyFlags.Required) != 0; }               
                }

                public string Name {
                        get { return name; }
                }

                public Type Type {
                        get { return type; }
                }

                public ConfigurationValidationAttribute ValidationAttribute {
                        get { return validation; }
                }

                [MonoTODO]
                protected internal virtual object ConvertFromString (string value)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                protected internal virtual string ConvertToString (object value)
                {
                        throw new NotImplementedException ();
                }
        }
}
#endif
#endif
