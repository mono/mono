// System.Configuration.Install.Installer.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
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

namespace System.Configuration.Install
{
	public class InstallerCollection : CollectionBase
	{
		#region Constructors

		internal InstallerCollection(Installer owner)
		{
			this.owner = owner;
		}

		#endregion Constructors
		

		public Installer this[int index] {
			get 
			{
				return (Installer) base.List[index];
			}
			set 
			{
				base.List[index] = value;
			}
		}

		public int Add (Installer value) {
			return base.List.Add (value);
		}

		public void AddRange (Installer[] value) {
			if (value == null) 
			{
				throw new ArgumentNullException ("value");
			}

			for (int counter = 0; counter < value.Length; counter++)
			{
				Add (value[counter]);
			}
		}
			
		public void AddRange (InstallerCollection value) {
			if (value == null)
			{
				throw new ArgumentNullException ("value");
			}

			int itemCount = value.Count;
			for (int counter = 0; counter < itemCount; counter++)
			{
				Add (value[counter]);
			}
		}

		public bool Contains (Installer value) {
			return base.List.Contains (value);
		}		

		public void CopyTo (Installer[] array, int index) {
			base.List.CopyTo (array, index);
		}
		
		public int IndexOf (Installer value) {
			return base.List.IndexOf (value);
		}

		public void Insert (int index, Installer value) {
			base.List.Insert (index, value);
		}

		protected override void OnInsert (int index, object value) {
			((Installer) value).parent = owner;
		}

		protected override void OnRemove (int index, object value) {
			((Installer) value).parent = null;
		}

		protected override void OnSet (int index, object oldValue, object newValue) {
			((Installer) oldValue).parent = null;
			((Installer) newValue).parent = owner;
		}

		public void Remove (Installer value) {
			base.List.Remove(value);
		}

		#region Private Instance Fields

		private Installer owner;

		#endregion Private Instance Fields
	}
}
