//
// System.Data.Common.ExceptionHelper
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;

using java.util;

namespace System.Data.Common
{
	internal sealed class ExceptionHelper
	{
		sealed class ResourceManager
		{
			private static readonly ResourceBundle _resourceBundle = ResourceBundle.getBundle("SystemData");
			
			internal ResourceManager()
			{				
			}
			
			internal string GetString(string key)
			{
				return _resourceBundle.getString(key);
			}
		}

		static ResourceManager _resourceManager = new ResourceManager();
		
		internal static ArgumentException InvalidSizeValue(int value)
		{
			string[] args = new string[] {value.ToString()};
			return new ArgumentException(GetExceptionMessage("ADP_InvalidSizeValue",args));
		}

		internal static ArgumentOutOfRangeException InvalidDataRowVersion(DataRowVersion value)
		{
			return InvalidEnumerationValue(typeof(DataRowVersion), (int) value);
		}
		 
		internal static ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value)
		{
			object[] args = new object[] { type.Name, value.ToString() } ;
			return new ArgumentOutOfRangeException(GetExceptionMessage("ADP_InvalidEnumerationValue",args));
		}
 
		internal static ArgumentException InvalidOffsetValue(int value)
		{
			string[] args = new string[] {value.ToString()};
			return new ArgumentException(GetExceptionMessage("ADP_InvalidOffsetValue",args));
		}

		internal static ArgumentOutOfRangeException InvalidParameterDirection(ParameterDirection value)
		{
			return InvalidEnumerationValue(typeof(ParameterDirection), (int) value);
		}

		internal static InvalidOperationException NoStoredProcedureExists(string procedureName) {
			object[] args = new object[1] { procedureName } ;
			return new InvalidOperationException(GetExceptionMessage("ADP_NoStoredProcedureExists", args));
		}

		internal static ArgumentNullException ArgumentNull(string parameter)
		{
			return new ArgumentNullException(parameter);
		}

		internal static InvalidOperationException TransactionRequired()
		{
			return new InvalidOperationException(GetExceptionMessage("ADP_TransactionRequired_Execute"));
		}

		internal static ArgumentOutOfRangeException InvalidOleDbType(int value)
		{
			string[] args = new string[] {value.ToString()};
			return new ArgumentOutOfRangeException(GetExceptionMessage("OleDb_InvalidOleDbType",args));
		}
 
		internal static ArgumentException InvalidDbType(int value)
		{
			string[] args = new string[] {value.ToString()};
			return new ArgumentException(GetExceptionMessage("ADP_UnknownDataType",args));
		}

		internal static InvalidOperationException DeriveParametersNotSupported(Type type,CommandType commandType)
		{
			string[] args = new string[] {type.ToString(),commandType.ToString()};
			return new InvalidOperationException(GetExceptionMessage("ADP_DeriveParametersNotSupported",args));
		}

		internal static InvalidOperationException ReaderClosed(string mehodName)
		{
			string[] args = new string[] {mehodName};
			return new InvalidOperationException(GetExceptionMessage("ADP_DataReaderClosed",args));
		}

		internal static ArgumentOutOfRangeException InvalidSqlDbType(int value)
		{
			string[] args = new string[] {value.ToString()};
			return new ArgumentOutOfRangeException(GetExceptionMessage("SQL_InvalidSqlDbType",args));
		}

		internal static ArgumentException UnknownDataType(string type1, string type2)
		{
			string[] args = new string[] {type1, type2};
			return new ArgumentException(GetExceptionMessage("ADP_UnknownDataType",args));
		}

		internal static InvalidOperationException TransactionNotInitialized()
		{
			return new InvalidOperationException(GetExceptionMessage("ADP_TransactionRequired_Execute"));
		}

		internal static InvalidOperationException ParametersNotInitialized(int parameterPosition,string parameterName,string parameterType)
		{
			object[] args = new object[] {parameterPosition,parameterName,parameterType};
			return new InvalidOperationException(GetExceptionMessage("OleDb_UninitializedParameters",args));
		}

		internal static InvalidOperationException WrongParameterSize(string provider)
		{
			string[] args = new string[] {provider};
			return new InvalidOperationException(GetExceptionMessage("ADP_PrepareParameterSize",args));
		}

		internal static InvalidOperationException ConnectionNotOpened(string operationName, string connectionState)
		{
			object[] args = new object[] {operationName,connectionState};
			return new InvalidOperationException(GetExceptionMessage("ADP_OpenConnectionRequired_PropertySet",args));
		}

		internal static InvalidOperationException ConnectionNotInitialized(string methodName)
		{
			object[] args = new object[] {methodName};
			return new InvalidOperationException(GetExceptionMessage("ADP_ConnectionRequired_ExecuteReader",args));
		}

		internal static InvalidOperationException OpenConnectionRequired(string methodName, object connectionState)
		{
			object[] args = new object[] {methodName, connectionState};
			return new InvalidOperationException(GetExceptionMessage("ADP_OpenConnectionRequired_Fill",args));
		}

		internal static InvalidOperationException OpenedReaderExists()
		{
			return new InvalidOperationException(GetExceptionMessage("ADP_OpenReaderExists"));
		}

		internal static InvalidOperationException ConnectionAlreadyOpen(object connectionState)
		{
			object[] args = new object[] {connectionState};
			return new InvalidOperationException(GetExceptionMessage("ADP_ConnectionAlreadyOpen",args));
		}

		internal static InvalidOperationException ConnectionStringNotInitialized()
		{
			return new InvalidOperationException(GetExceptionMessage("ADP_NoConnectionString"));
		}

		internal static InvalidOperationException ConnectionIsBusy(object commandType,object connectionState)
		{
			object[] args = new object[] {commandType.ToString(), connectionState.ToString()};
			return new InvalidOperationException(GetExceptionMessage("ADP_CommandIsActive",args));
		}

		internal static InvalidOperationException NotAllowedWhileConnectionOpen(string propertyName, object connectionState)
		{
			object[] args = new object[] {propertyName,connectionState};
			return new InvalidOperationException(GetExceptionMessage("ADP_OpenConnectionPropertySet",args));
		}

		internal static ArgumentException OleDbNoProviderSpecified()
		{
			return new ArgumentException(GetExceptionMessage("OleDb_NoProviderSpecified"));
		}

		internal static ArgumentException InvalidValueForKey(string key)
		{
			string[] args = new string[] { key };
			return new ArgumentException(String.Format("Invalid value for key {0}",args));
		}

		internal static InvalidOperationException ParameterSizeNotInitialized(int parameterIndex, string parameterName,string parameterType,int parameterSize)
		{
			object[] args = new object[] { parameterIndex.ToString(),parameterName,parameterType,parameterSize.ToString()};
			return new InvalidOperationException(GetExceptionMessage("ADP_UninitializedParameterSize",args));
		}

		internal static ArgumentException InvalidUpdateStatus(UpdateStatus status)
		{
			object[] args = new object[] { status };
			return new ArgumentException(GetExceptionMessage("ADP_InvalidUpdateStatus",args));
		}

		internal static InvalidOperationException UpdateRequiresCommand(string command)
		{
			return new InvalidOperationException(GetExceptionMessage("ADP_UpdateRequiresCommand" + command));
		}

		internal static DataException RowUpdatedError()
		{
			return new DataException(GetExceptionMessage("ADP_RowUpdatedErrors"));
		}

		internal static string GetExceptionMessage(string key,object[] args)
		{
			string exceptionMessage = _resourceManager.GetString(key);

			if ((args == null) || (args.Length == 0)) {
				return exceptionMessage;
			}
			else {
				return String.Format(exceptionMessage,args);
			}
		}

		internal static string GetExceptionMessage(string key)
		{
			return GetExceptionMessage(key,null);
		}
	}
}
