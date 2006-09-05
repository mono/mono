//
// System.ComponentModel.Design.DesignerVerbCollection.cs
//
// Authors:
//   Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
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

using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class DesignerVerbCollection : CollectionBase
	{
		public DesignerVerbCollection()
		{
		}

		public DesignerVerbCollection (DesignerVerb[] value)
		{
			InnerList.AddRange (value);
		}

		public DesignerVerb this[int index] {
			get {
				return (DesignerVerb) InnerList[index];
			}
			
			set {
				InnerList[index] = value;		
			}
		}

		public int Add (DesignerVerb value)
		{
			return InnerList.Add ( value);
		}

		public void AddRange (DesignerVerb[] value)
		{
			InnerList.AddRange (value);
		}

		public void AddRange (DesignerVerbCollection value)
		{
			InnerList.AddRange (value);
		}

		public bool Contains (DesignerVerb value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (DesignerVerb[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (DesignerVerb value)
		{
			return InnerList.IndexOf (value);
		}

		public void Insert (int index, DesignerVerb value)
		{
			InnerList.Insert (index, value);
		}

		protected override void OnClear()
		{
			// Cannot think of anything we would need to do here - probably nothing
		}

		protected override void OnInsert (int index, object value)
		{
			// Cannot think of anything we would need to do here - probably nothing
		}

		protected override void OnRemove (int index, object value)
		{
			// Cannot think of anything we would need to do here - probably nothing
		}

		protected override void OnSet(int index, object oldValue, object newValue)
		{
			// Cannot think of anything we would need to do here - probably nothing
		}

		protected override void OnValidate(object value)
		{
			// Cannot think of anything we would need to do here - probably nothing
		}

		public void Remove (DesignerVerb value)
		{
			InnerList.Remove (value);
		}
	}	
}
