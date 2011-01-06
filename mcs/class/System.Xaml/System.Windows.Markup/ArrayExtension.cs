//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Windows.Markup
{
	[MarkupExtensionReturnType (typeof (Array))]
	[ContentProperty ("Items")]
#if !NET_2_1
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyPresentationFramework_3_5)]
#endif
	public class ArrayExtension : MarkupExtension
	{
		public ArrayExtension ()
		{		
#if MOONLIGHT
			items = new List<object> ();
#else
			items = new ArrayList ();
#endif
		}

		public ArrayExtension (Array elements)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");
			Type = elements.GetType ().GetElementType ();
#if MOONLIGHT
			items = new List<object> ();
			foreach (var element in elements)
				items.Add (element);
#else
			items = new ArrayList (elements);
#endif
		}

		public ArrayExtension (Type arrayType)
		{
			if (arrayType == null)
				throw new ArgumentNullException ("arrayType");
			Type = arrayType;
#if MOONLIGHT
			items = new List<object> ();
#else
			items = new ArrayList ();
#endif
		}

		[ConstructorArgument ("arrayType")]
		public Type Type { get; set; }

		IList items;
#if !NET_2_1
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
#endif
		public IList Items {
			get { return items; }
		}

		public void AddChild (Object value)
		{
			// null is allowed.
			Items.Add (value);
		}
		
		public void AddText (string text)
		{
			// null is allowed.
			Items.Add (text);
		}
		
		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Type == null)
				throw new InvalidOperationException ("Type property must be set before calling ProvideValue method");

			bool invalid = false;
			foreach (var item in Items) {
				if (item == null) {
					if (Type.IsValueType)
						invalid = true;
				}
				else if (!Type.IsAssignableFrom (item.GetType ()))
					invalid = true;
				if (invalid)
					throw new InvalidOperationException (String.Format ("Item in the array must be an instance of '{0}'", Type));
			}
			Array a = Array.CreateInstance (Type, Items.Count);
			Items.CopyTo (a, 0);
			return a;
		}
	}
}
