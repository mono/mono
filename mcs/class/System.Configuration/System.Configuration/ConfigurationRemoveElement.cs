//
// System.Configuration.ConfigurationPropertyOptions.cs
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
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

using System;
using System.ComponentModel;
using System.Configuration;

namespace System.Configuration
{
	partial class ConfigurationElementCollection
	{
		sealed class ConfigurationRemoveElement : ConfigurationElement
		{
			readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection ();
			readonly ConfigurationElement _origElement;
			readonly ConfigurationElementCollection _origCollection;

			internal ConfigurationRemoveElement (ConfigurationElement origElement, ConfigurationElementCollection origCollection)
			{
				_origElement = origElement;
				_origCollection = origCollection;

				foreach (ConfigurationProperty p in origElement.Properties)
					if (p.IsKey) {
						properties.Add (p);
					}
			}

			internal object KeyValue
			{
				get
				{
					foreach (ConfigurationProperty p in Properties)
						_origElement [p] = this [p];

					return _origCollection.GetElementKey (_origElement);
				}
			}

			protected internal override ConfigurationPropertyCollection Properties
			{
				get { return properties; }
			}
		}
	}
}
