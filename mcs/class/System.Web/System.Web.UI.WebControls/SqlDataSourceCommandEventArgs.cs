//
// System.Web.UI.WebControls.SqlDataSourceCommandEventArgs
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
using System.Data;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceCommandEventArgs : CancelEventArgs {
		
		public SqlDataSourceCommandEventArgs (IDbCommand command)
		{
			this.command = command;
		}
		
		IDbCommand command;
		public IDbCommand Command {
			get { return command; }
		}
	}
}
#endif

