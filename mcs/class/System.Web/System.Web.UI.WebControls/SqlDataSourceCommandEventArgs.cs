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
		
		[MonoTODO ("this ctor is here to make cls happy")]
		public SqlDataSourceCommandEventArgs () {
			throw new Exception ("you shouldnt call this");
		}
		[CLSCompliant (false)]
		public SqlDataSourceCommandEventArgs (IDbCommand command)
		{
			this.command = command;
		}
		
		IDbCommand command;
		[MonoTODO ("cls not happy")]
		[CLSCompliant (false)]
		public IDbCommand Command {
			get { return command; }
		}
	}
}
#endif

