//
// DeclaredTypeElementCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

namespace System.Runtime.Serialization.Configuration
{
	[ConfigurationCollection (typeof (DeclaredTypeElement))]
	public sealed class DeclaredTypeElementCollection : ConfigurationElementCollection
	{
		public DeclaredTypeElementCollection ()
		{
		}

		public DeclaredTypeElement this [int index] {
			get { return (DeclaredTypeElement) BaseGet (index); }
			set {
				RemoveAt (index);
				Add (value);
			}
		}

		public new DeclaredTypeElement this [string typeName] {
			get { return (DeclaredTypeElement) BaseGet (typeName); }
			set {
				Remove (typeName);
				Add (value);
			}
		}

		public void Add (DeclaredTypeElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public bool Contains (string typeName)
		{
			return BaseGet (typeName) != null;
		}

		public int IndexOf (DeclaredTypeElement element)
		{
			return BaseIndexOf (element);
		}

		public void Remove (DeclaredTypeElement element)
		{
			Remove ((string) GetElementKey (element));
		}

		public void Remove (string typeName)
		{
			BaseRemove (typeName);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new DeclaredTypeElement ();
		}

		protected override Object GetElementKey (
			ConfigurationElement element)
		{
			return ((DeclaredTypeElement) element).Type;
		}
	}
}
#endif
