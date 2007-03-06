//
// System.Web.Configuration.BackendProviderCollection
//
// Authors:
//      Marek Habersack <grendello@gmail.com>
//
// (C) 2007 Marek Habersack
//

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
using System;
using System.Configuration;

namespace Mainsoft.Web.Configuration
{
	[ConfigurationCollection (typeof (BackendProviderInfo), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public sealed class BackendProviderCollection: ConfigurationElementCollection
        {	
		static ConfigurationPropertyCollection properties;

                static BackendProviderCollection ()
                {
                        properties = new ConfigurationPropertyCollection();
                }

                public void Add (BackendProviderInfo info)
                {
                        BaseAdd (info, false);
                }
                
                public void Clear ()
                {
                        BaseClear ();
                }
                
                protected override ConfigurationElement CreateNewElement ()
                {
                        return new BackendProviderInfo ();
                }
                
                protected override object GetElementKey (ConfigurationElement element)
                {
                        return ((BackendProviderInfo)element).Invariant;
                }
                
                public void Remove (string key)
                {
                        BaseRemove (key);
                }
                
                public void RemoveAt (int index)
                {
                        BaseRemoveAt (index);
                }

                public BackendProviderInfo this [int index] {
                        get { return (BackendProviderInfo) BaseGet (index); }
                        set {  if (BaseGet(index) != null)  BaseRemoveAt(index);  BaseAdd(index, value); }
                }

                public new BackendProviderInfo this [string providerName] {
                        get { return (BackendProviderInfo) BaseGet (providerName); }
                }

		protected override ConfigurationPropertyCollection Properties {
                        get { return properties; }
                }
	}
}

#endif