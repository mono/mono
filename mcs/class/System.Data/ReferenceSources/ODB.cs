using System.Data.Common;

namespace System.Data.OleDb {

	internal static class ODB {

		// used by OleDbConnection to create and verify OLE DB Services
		internal const string DataLinks_CLSID = "CLSID\\{2206CDB2-19C1-11D1-89E0-00C04FD7A829}\\InprocServer32";

		static internal InvalidOperationException MDACNotAvailable(Exception inner) {
			return ADP.DataAdapter(Res.GetString(Res.OleDb_MDACNotAvailable), inner);
		}        
	}
}