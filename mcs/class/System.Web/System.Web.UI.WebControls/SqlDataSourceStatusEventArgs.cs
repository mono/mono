//
// System.Web.UI.WebControls.SqlDataSourceStatusEventArgs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceStatusEventArgs : EventArgs {
		public SqlDataSourceStatusEventArgs (IOrderedDictionary outputParameters, object returnValue, int rowsAffected)
		{
			this.outputParameters = outputParameters;
			this.returnValue = returnValue;
			this.rowsAffected = rowsAffected;
		}
		
		IOrderedDictionary outputParameters;
		object returnValue;
		int rowsAffected;
		
		public IOrderedDictionary OutputParameters {
			get { return outputParameters; }
		}
		
		public object ReturnValue {
			get { return returnValue; }
		}
		
		public int RowsAffected {
			get { return rowsAffected; }
		}
	}
}
#endif

