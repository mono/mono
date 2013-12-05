/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.Collections;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySchemaClassCollection : CollectionBase
	{
		public ActiveDirectorySchemaClass this [int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int Add (ActiveDirectorySchemaClass schemaClass)
		{
			throw new NotImplementedException ();
		}

		public void AddRange (ActiveDirectorySchemaClass[] schemaClasses)
		{
			throw new NotImplementedException ();
		}

		public void AddRange (ActiveDirectorySchemaClassCollection schemaClasses)
		{
			throw new NotImplementedException ();
		}

		public void AddRange (ReadOnlyActiveDirectorySchemaClassCollection schemaClasses)
		{
			throw new NotImplementedException ();
		}

		public void Remove (ActiveDirectorySchemaClass schemaClass)
		{
			throw new NotImplementedException ();
		}

		public void Insert (int index, ActiveDirectorySchemaClass schemaClass)
		{
			throw new NotImplementedException ();
		}

		public bool Contains (ActiveDirectorySchemaClass schemaClass)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (ActiveDirectorySchemaClass[] schemaClasses, int index)
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (ActiveDirectorySchemaClass schemaClass)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClearComplete ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnInsertComplete (int index, object value)
		{
			throw new NotImplementedException ();
		}

		protected override void OnRemoveComplete (int index, object value)
		{
			throw new NotImplementedException ();
		}

		protected override void OnSetComplete (int index, object oldValue, object newValue)
		{
			throw new NotImplementedException ();
		}

		protected override void OnValidate (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
