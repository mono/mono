//
// System.Web.UI.WebControls.AccessDataSource.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.IO;

namespace System.Web.UI.WebControls {
	public class AccessDataSource : SqlDataSource 
	{
		string dataFile;
		
		public AccessDataSource () : base ()
		{
			this.ProviderName = "System.Data.OleDb";
		}

		public AccessDataSource (string dataFile, string selectCommand) : 
			base (String.Empty, selectCommand)
		{
			this.dataFile = dataFile;
			//After setting dataFile, connectionString gets recreated
			//On accessing ConnectionString, MS.Net throws NullReferenceException
			//Need to dig more on this.
			this.ProviderName = "System.Data.OleDb";							
		}

		/*[MonoTODO]
		protected override SqlDataSourceView CreateDataSourceView (string view)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void SaveDataToCache (int startingRowIndex,
							int maxRows, object data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void SaveTotalRowCountToCache(int totalRows)
		{
			throw new NotImplementedException ();
		}

		public override string SqlCacheDependency {
			get { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
			set { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
		}*/

		//Above commented out portion will come into place after implementing 
		// stuff in SqlDataSource class.
		//Overrid implementation will depend on how .Net stores data in 
		//Cache property of HttpContext object. 

		public override string ConnectionString {
			get { return this.ConnectionString; }
			set { throw new InvalidOperationException 
				("The ConnectionString is automatically generated for AccessDataSource and hence cannot be set."); 
			}
		}

		public string DataFile {
			get { return dataFile; }
			set { dataFile = value; }
			//After setting dataFile, connectionString gets recreated
			//On accessing ConnectionString, MS.Net throws NullReferenceException
			//Need to dig more on this.
		}

		public override string ProviderName {
			get { return this.ProviderName; }
			set { throw new InvalidOperationException
				("Setting ProviderName on an AccessDataSource is not allowed");
			}
		}		
	}
}
#endif
