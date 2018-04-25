//
// System.Web.UI.DataSourceSelectArguments.cs
//
// Author:
//   Sanjay Gupta <gsanjay@novell.com>
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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

using System;

namespace System.Web.UI
{
	public sealed class DataSourceSelectArguments
	{
		string sortExpression;
		int startingRowIndex;
		int maxRows;
		bool getTotalRowCount;
		int totalRowCount = -1;
		DataSourceCapabilities dsc = DataSourceCapabilities.None;

		// MSDN: Gets a DataSourceSelectArguments object with the sort expression set to Empty. 
		public static DataSourceSelectArguments Empty {
			get {
				return new DataSourceSelectArguments ();
			}
		}

		public DataSourceSelectArguments ()
		{
		}

		public DataSourceSelectArguments (string sortExpression)
		{
			this.sortExpression = sortExpression;
		}

		public DataSourceSelectArguments (int startRowIndex, int maximumRows)
		{
			this.startingRowIndex = startRowIndex;
			this.maxRows = maximumRows;
		}

		public DataSourceSelectArguments (string sortExpression, int startRowIndex, int maximumRows)
		{
			this.sortExpression = sortExpression; 
			this.startingRowIndex = startRowIndex;
			this.maxRows = maximumRows;
		}

		public void AddSupportedCapabilities (DataSourceCapabilities capabilities)
		{
			this.dsc = this.dsc | capabilities;
		}

		// MSDN: The DataSourceSelectArguments class overrides the Object.Equals method to test 
		// equality using the various properties of the objects. If the MaximumRows, 
		// RetrieveTotalRowCount, SortExpression, StartRowIndex, and TotalRowCount properties 
		// are all equal in value, the Equals(Object) method returns true.
		public override bool Equals (object obj)
		{
			DataSourceSelectArguments args = obj as DataSourceSelectArguments;
			if (args == null)
				return false;

			return (this.SortExpression == args.SortExpression &&
				this.StartRowIndex == args.StartRowIndex &&
				this.MaximumRows == args.MaximumRows &&
				this.RetrieveTotalRowCount == args.RetrieveTotalRowCount &&
				this.TotalRowCount == args.TotalRowCount);
		}

		public override int GetHashCode ()
		{
			int hash = SortExpression != null ? SortExpression.GetHashCode() : 0;
			return hash ^ StartRowIndex ^ MaximumRows ^ RetrieveTotalRowCount.GetHashCode() ^ TotalRowCount;
		}

		// The RaiseUnsupportedCapabilitiesError method is used by data-bound controls 
		// to compare additional requested capabilities represented by the properties 
		// of the DataSourceSelectArguments class, such as the ability to sort or page 
		// through a result set, with the capabilities supported by the data source view. 
		// The view calls its own RaiseUnsupportedCapabilityError method for each possible 
		// capability defined in the DataSourceCapabilities enumeration. 
		public void RaiseUnsupportedCapabilitiesError (DataSourceView view)
		{
			DataSourceCapabilities requestedCaps = RequestedCapabilities;
			DataSourceCapabilities notSupportedCaps = (requestedCaps ^ dsc) & requestedCaps;
			if (notSupportedCaps == DataSourceCapabilities.None)
				return;

			if ((notSupportedCaps & DataSourceCapabilities.RetrieveTotalRowCount) > 0)
				notSupportedCaps = DataSourceCapabilities.RetrieveTotalRowCount;
			else if ((notSupportedCaps & DataSourceCapabilities.Page) > 0)
				notSupportedCaps = DataSourceCapabilities.Page;

			view.RaiseUnsupportedCapabilityError (notSupportedCaps);
		}

		DataSourceCapabilities RequestedCapabilities {
			get {
				DataSourceCapabilities caps = DataSourceCapabilities.None;
				if (!String.IsNullOrEmpty (SortExpression))
					caps |= DataSourceCapabilities.Sort;
				if (RetrieveTotalRowCount)
					caps |= DataSourceCapabilities.RetrieveTotalRowCount;
				if (StartRowIndex > 0 || MaximumRows > 0)
					caps |= DataSourceCapabilities.Page;
				return caps;
			}
		}

		public int MaximumRows {
			get { return this.maxRows; }
			set { this.maxRows = value; }
		}

		public bool RetrieveTotalRowCount  {
			get { return this.getTotalRowCount; }
			set { this.getTotalRowCount = value; }
		}

		public string SortExpression {
			get {
				if (sortExpression == null)
					return String.Empty;
				return this.sortExpression;
			}
			set { this.sortExpression = value; }
		}

		public int StartRowIndex {
			get { return this.startingRowIndex; }
			set { this.startingRowIndex = value; }
		}

		public int TotalRowCount {
			get { return this.totalRowCount; }
			set { this.totalRowCount = value; }
		}
	}
}
