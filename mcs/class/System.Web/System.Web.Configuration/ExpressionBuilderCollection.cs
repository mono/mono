//
// System.Web.Configuration.ExpressionBuilderCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class ExpressionBuilderCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection props;

		static ExpressionBuilderCollection ()
		{
			props = new ConfigurationPropertyCollection ();
			ConfigurationPropertyFlags flags = ConfigurationPropertyFlags.DefaultCollection;
			ConfigurationProperty prop = new ConfigurationProperty (null, typeof (ExpressionBuilderCollection), null, flags);
			props.Add (prop);
		}

		public ExpressionBuilder this [int index] {
			get { return (ExpressionBuilder) BaseGet (index); }
			set {
				if (BaseGet (index) != null)
					BaseRemoveAt (index);

				BaseAdd (index, value);
			}
		}

		public new ExpressionBuilder this [string name] {
			get { return (ExpressionBuilder) BaseGet (name); }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return props; }
		}

		public void Add (ExpressionBuilder buildProvider)
		{
			BaseAdd (buildProvider);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		protected override bool CompareKeys (object key1, object key2)
		{
			return (0 == CaseInsensitiveComparer.DefaultInvariant.Compare ((string) key1, (string) key2));
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new ExpressionBuilder ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			ExpressionBuilder eb = (ExpressionBuilder) element;
			return eb.ExpressionPrefix;
		}
	}
}
#endif // NET_2_0

