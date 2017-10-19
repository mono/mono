//
// System.Web.UI.WebControls.AccessDataSourceView
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) Novell, Inc. (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Data.OleDb;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class AccessDataSourceView : SqlDataSourceView
	{
		OleDbConnection oleConnection;
		OleDbCommand oleCommand;
		AccessDataSource dataSource;
		public AccessDataSourceView (AccessDataSource owner, string name, HttpContext context)
			: base (owner, name, context)
		{
			dataSource = owner;
			oleConnection = new OleDbConnection (owner.ConnectionString);
		}

		[MonoTODO ("Handle arguments")]
		protected internal override IEnumerable ExecuteSelect (
						DataSourceSelectArguments arguments)
		{
			oleCommand = new OleDbCommand (this.SelectCommand, oleConnection);
			SqlDataSourceSelectingEventArgs cmdEventArgs = new SqlDataSourceSelectingEventArgs (oleCommand, arguments);
			OnSelecting (cmdEventArgs);
			IEnumerable enums = null; 
			Exception exception = null;
			OleDbDataReader reader = null;
			try {
				System.IO.File.OpenRead (dataSource.DataFile).Close ();
				oleConnection.Open ();
				reader = (OleDbDataReader)oleCommand.ExecuteReader ();
			
				//enums = reader.GetEnumerator ();
				throw new NotImplementedException("OleDbDataReader doesnt implements GetEnumerator method yet");
			} catch (Exception e) {
				exception = e;
			}
			SqlDataSourceStatusEventArgs statusEventArgs = 
				new SqlDataSourceStatusEventArgs (oleCommand, reader.RecordsAffected, exception);
			OnSelected (statusEventArgs);
			if (exception !=null)
				throw exception;
			return enums;			
		}						
	}	
}

