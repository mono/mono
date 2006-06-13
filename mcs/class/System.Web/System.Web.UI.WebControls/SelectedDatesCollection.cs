//
// System.Web.UI.WebControls.SelectedDatesCollection
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS - no inheritance demand required because the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class SelectedDatesCollection : ICollection {

		ArrayList l;
		
		public SelectedDatesCollection (ArrayList dateList)
		{
			l = dateList;
		}
		
	
		public void Add (DateTime date)
		{
			date = date.Date;
			if (!l.Contains (date))
				l.Add (date);
		}
		
		public void Clear ()
		{
			l.Clear ();
		}
		
		public bool Contains (DateTime date)
		{
			return l.Contains (date.Date);
		}
		
		public void CopyTo (Array array, int index)
		{
			l.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return l.GetEnumerator ();
		}
		
		public void Remove (DateTime date)
		{
			l.Remove (date.Date);
		}
		
		public void SelectRange (DateTime fromDate, DateTime toDate)
		{
			fromDate = fromDate.Date;
			toDate = toDate.Date;
			
			l.Clear ();
			for (DateTime dt = fromDate; dt <= toDate; dt = dt.AddDays (1))
				Add (dt);
		}
			
		public int Count {
			get {
				return l.Count;
			}
		}
		
		public bool IsReadOnly {
			get {
				return l.IsReadOnly;
			}
		}
		
		public bool IsSynchronized {
			get {
				return l.IsSynchronized;
			}
		}
		
		public DateTime this [int index] {
			get {
				return (DateTime) l [index];
			}
		}
		
		public object SyncRoot {
			get {
				return this;
			}
		}

	}
}
