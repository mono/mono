//using System;
//
//using java.sql;
//
//namespace System.Data.OleDb
//{
//	public class OleDbOracleDataReader : OleDbDataReader
//	{
//		private int _nextParameterIndex = 0;
//		internal OleDbOracleDataReader(OleDbCommand command)
//		{
//			_command = command;
//			_statement = command.getStatement();
//			_schemaTable = null;
//        
//			try {
//				_results = null;
//				_lastColumnAccessed = -1;
//				_hasRows = Read();
//				_rowRead = _hasRows;
//			}
//			catch (SQLException e) {
//				throw new Exception(e.Message, e);
//			}
//		}
//
//		private ResultSet GetNextResultSet()
//		{
//			for (;_command.Parameters.Count > _nextParameterIndex;_nextParameterIndex++)
//			{
//				if (((OleDbParameter)_command.Parameters[_nextParameterIndex]).IsOracleRefCursor)
//					return (ResultSet)((CallableStatement)_statement).getObject(++_nextParameterIndex);
//			}
//
//			return null;
//		}
//
//		public override bool Read()
//		{
//			try {
//				if (_results == null) {
//					_results = GetNextResultSet();
//					_lastColumnAccessed = -1;
//					_rowRead = false;
//					_lastRowReached = false;
//				}
//		
//				if (_results == null) {
//					return false;
//				}
//				
//				// in the ctor we read one row so we have to check
//				// if a row has been read. if yes turn the flag of
//				// so next time we actualy read. and return the _hasRows param 
//				// we initialize in the ctor.
//				if (_rowRead) {
//					_rowRead = false;
//					return _hasRows;
//				}
//				else {
//					if (!_lastRowReached) {
//						_lastRowReached = !(_results.next());
//						return !(_lastRowReached);
//					}
//					else {
//						return false;
//					}
//				}
//			}
//			catch (SQLException exp) {
//				throw new Exception(exp.Message, exp);
//			}
//		}
//
//		public override bool NextResult()
//		{
//			try {
//				if (_results != null) {
//					_results.close();	
//				}
//				_results = GetNextResultSet();
//				_lastColumnAccessed = -1;
//				_rowRead = false;
//				_lastRowReached = false;
//				_schemaTable = ConstructSchemaTable();
//
//				return (_results != null);
//			}
//			catch (SQLException exp) {
//				throw new Exception(exp.Message, exp);
//			}
//		}
//	}
//}
