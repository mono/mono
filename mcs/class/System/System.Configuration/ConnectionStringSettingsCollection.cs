//
// System.Configuration.ConnectionStringSettingsCollection.cs
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
#if XML_DEP

#region Using directives
using System;
#endregion

namespace System.Configuration
{
        public sealed class ConnectionStringSettingsCollection : ConfigurationElementCollection
        {

                #region Constructors
                public ConnectionStringSettingsCollection () : base ()
                {
                }
                #endregion // Constructors

                #region Properties
                public new ConnectionStringSettings this [string Name]
                {
                        get
                        {
                                foreach (ConfigurationElement c in this) {
                                        if (!(c is ConnectionStringSettings))
                                                continue;
                                        if (((ConnectionStringSettings) c).Name.Equals (Name))
                                                return c as ConnectionStringSettings;

                                }
                                return null;
                        }
                }

                public new ConnectionStringSettings this [int index]
                {
                        get
                        {
                                return (ConnectionStringSettings) BaseGet (index);
                        }
                        set
                        {
                                if (BaseGet (index) != null)
                                        BaseRemoveAt (index);
                                BaseAdd (index, value);
                        }
                }
                #endregion // Properties

                #region Methods
                protected override ConfigurationElement CreateNewElement ()
                {
                        return new ConnectionStringSettings ("", "", "");
                }
                protected override object GetElementKey (ConfigurationElement element)
                {
                        return ((ConnectionStringSettings) element).Name;
                }

                public void Add (ConnectionStringSettings settings)
                {
                        BaseAdd ((ConfigurationElement) settings);
                }
                public void Clear ()
                {
                        BaseClear ();
                }
                public int IndexOf (ConnectionStringSettings settings)
                {
                        return BaseIndexOf (settings);
                }
                public void Remove (ConnectionStringSettings settings)
                {
                        BaseRemove (settings.Name);
                }
                public void Remove (string name)
                {
                        BaseRemove (name);
                }
                public void RemoveAt (int index)
                {
                        BaseRemoveAt (index);
                }

                protected override void BaseAdd (ConfigurationElement element)
                {
                        if ( ! (element is ConnectionStringSettings))
                                base.BaseAdd (element);
                        if (IndexOf ((ConnectionStringSettings) element) >= 0)
                                throw new ConfigurationException (String.Format ("The element {0} already exist!",
                                                                                 ((ConnectionStringSettings) element).Name));
                        base.BaseAdd (element);
                }
                protected override void BaseAdd (int index, ConfigurationElement element)
                {
                        if (!(element is ConnectionStringSettings))
                                base.BaseAdd (element);
                        if (IndexOf ((ConnectionStringSettings) element) >= 0)
                                throw new ConfigurationException (String.Format ("The element {0} already exist!",
                                                                                 ((ConnectionStringSettings) element).Name));
                        this [index] = (ConnectionStringSettings) element;
                }
                protected override bool CompareKeys (object key1, object key2)
                {
                        if (!(key1 is ConnectionStringSettings) || !(key2 is ConnectionStringSettings))
                                return false;
                        return (((ConnectionStringSettings) key1).Name == ((ConnectionStringSettings) key2).Name);
                }
                #endregion // Methods
        
        }

}
#endif // XML_DEP
#endif // NET_2_0
