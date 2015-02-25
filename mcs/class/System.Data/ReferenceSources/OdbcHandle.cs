using System.Text;
using System.Runtime.InteropServices;

namespace System.Data.Odbc {

    internal abstract class OdbcHandle : SafeHandle {

    	protected OdbcHandle(ODBC32.SQL_HANDLE handleType, OdbcHandle parentHandle) : base(IntPtr.Zero, true) {
			throw new NotImplementedException ();
    	}

		internal ODBC32.RetCode GetDiagnosticRecord(short record, out string sqlState, StringBuilder message, out int nativeError, out short cchActual) {
			throw new NotImplementedException ();
		}
    }
}