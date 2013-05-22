//
// System.Configuration.ConnectionStringSettingsCollection.cs
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
//   Chris Toshok <toshok@ximian.com>
//
//
// Copyright (C) 2004,2005 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	[ConfigurationCollection (typeof (ConnectionStringSettings),
				  CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public sealed class ConnectionStringSettingsCollection : ConfigurationElementCollection
        {

                public ConnectionStringSettingsCollection () : base ()
                {
                }

                public new ConnectionStringSettings this [string Name]
                {
                        get {
                                foreach (ConfigurationElement c in this) {
                                        if (!(c is ConnectionStringSettings))
                                                continue;
                                        if (string.Compare(((ConnectionStringSettings) c).Name, Name, true, 
                                                System.Globalization.CultureInfo.InvariantCulture) == 0)
                                                return c as ConnectionStringSettings;

                                }
                                return null;
                        }
                }

                public ConnectionStringSettings this [int index]
                {
                        get { return (ConnectionStringSettings) BaseGet (index); }
                        set {
                                if (BaseGet (index) != null)
                                        BaseRemoveAt (index);
                                BaseAdd (index, value);
                        }
                }

		[MonoTODO]
		protected internal override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}


                protected override ConfigurationElement CreateNewElement ()
                {
                        return new ConnectionStringSettings ();
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

                protected override void BaseAdd (int index, ConfigurationElement element)
                {
                        if (!(element is ConnectionStringSettings))
                                base.BaseAdd (element);
                        if (IndexOf ((ConnectionStringSettings) element) >= 0)
                                throw new ConfigurationErrorsException (String.Format ("The element {0} already exist!",
                                                                                 ((ConnectionStringSettings) element).Name));
                        this [index] = (ConnectionStringSettings) element;
                }
        }

}

