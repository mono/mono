//
// FormCollection.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
using System;
using System.Collections;
using System.Text;

namespace System.Windows.Forms
{
	public class FormCollection : ReadOnlyCollectionBase
	{
		public FormCollection () : base ()
		{
		}
		
		#region Public Properties
		public virtual Form this[int index] {
			get { 
				return (Form)base.InnerList[index];
			}
		}

		public virtual Form this[string name] {
			get {
				foreach (Form f in base.InnerList)
					if (f.Name == name)
						return f;
							
				return null;
			}
		}
		#endregion
		
		#region Internal Add/Remove Methods
		internal void Add (Form form)
		{
			if (base.InnerList.Contains (form))
				return;
			
			base.InnerList.Add (form);
		}
		
		internal void Remove (Form form)
		{
			base.InnerList.Remove (form);
		}
		#endregion
	}
}
