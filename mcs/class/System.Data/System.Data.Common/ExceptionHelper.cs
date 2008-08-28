//
// System.Data.Common.ExceptionHelper
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;
using System.Globalization;

namespace System.Data.Common
{
	internal sealed class ExceptionHelper
	{
		internal static ArgumentException InvalidSizeValue (int value)
		{
			string [] args = new string [] {value.ToString ()};
			return new ArgumentException  (GetExceptionMessage ("Invalid parameter Size value '{0}'. The value must be greater than or equal to 0.",args));
		}

		internal static void CheckEnumValue (Type enumType, object value)
		{
			if (!Enum.IsDefined (enumType, value))
				throw InvalidEnumValueException (enumType.Name, value);
		}

		internal static ArgumentException InvalidEnumValueException (String enumeration, object value)
		{
			string msg = string.Format (CultureInfo.InvariantCulture,
				"The {0} enumeration value, {1}, is invalid.",
				enumeration, value);
#if NET_2_0
			return new ArgumentOutOfRangeException (enumeration, msg);
#else
			return new ArgumentException (msg);
#endif
		}

		internal static ArgumentOutOfRangeException InvalidDataRowVersion (DataRowVersion value)
		{
			object [] args = new object [] { "DataRowVersion", value.ToString () } ;
			return new ArgumentOutOfRangeException  (GetExceptionMessage ("{0}: Invalid DataRow Version enumeration value: {1}",args));
		}

		internal static ArgumentOutOfRangeException InvalidParameterDirection (ParameterDirection value)
		{
			object [] args = new object [] { "ParameterDirection", value.ToString () } ;
			return new ArgumentOutOfRangeException  (GetExceptionMessage ("Invalid direction '{0}' for '{1}' parameter.",args));
		}

		internal static InvalidOperationException NoStoredProcedureExists (string procedureName) {
			object [] args = new object [1] { procedureName } ;
			return new InvalidOperationException  (GetExceptionMessage ("The stored procedure '{0}' doesn't exist.", args));
		}

		internal static ArgumentNullException ArgumentNull (string parameter)
		{
			return new ArgumentNullException (parameter);
		}

		internal static InvalidOperationException TransactionRequired ()
		{
			return new InvalidOperationException  (GetExceptionMessage ("Execute requires the command to have a transaction object when the connection assigned to the command is in a pending local transaction.  The Transaction property of the command has not been initialized."));
		}

		internal static ArgumentOutOfRangeException InvalidOleDbType (int value)
		{
			string [] args = new string [] { value.ToString() };
			return new ArgumentOutOfRangeException  (GetExceptionMessage ("Invalid OleDbType enumeration value: {0}",args));
		}
 
		internal static ArgumentException InvalidDbType(int value)
		{
			string [] args = new string [] { value.ToString () };
			return new ArgumentException  (GetExceptionMessage ("No mapping exists from DbType {0} to a known {1}.",args));
		}

		internal static InvalidOperationException DeriveParametersNotSupported(Type type,CommandType commandType)
		{
			string [] args = new string [] { type.ToString(),commandType.ToString() };
			return new InvalidOperationException  (GetExceptionMessage ("{0} DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{1}.",args));
		}

		internal static InvalidOperationException ReaderClosed (string mehodName)
		{
			string [] args = new string [] { mehodName };
			return new InvalidOperationException  (GetExceptionMessage ("Invalid attempt to {0} when reader is closed.",args));
		}

		internal static ArgumentOutOfRangeException InvalidSqlDbType (int value)
		{
			string [] args = new string [] { value.ToString () };
			return new ArgumentOutOfRangeException  (GetExceptionMessage ("{0}: Invalid SqlDbType enumeration value: {1}.",args));
		}

		internal static ArgumentException UnknownDataType (string type1, string type2)
		{
			string [] args = new string [] { type1, type2 };
			return new ArgumentException  (GetExceptionMessage ("No mapping exists from DbType {0} to a known {1}.",args));
		}

		internal static InvalidOperationException TransactionNotInitialized ()
		{
			return new InvalidOperationException  (GetExceptionMessage ("Execute requires the command to have a transaction object when the connection assigned to the command is in a pending local transaction.  The Transaction property of the command has not been initialized."));
		}

		internal static InvalidOperationException TransactionNotUsable (Type type)
		{
			return new InvalidOperationException (string.Format (
				CultureInfo.InvariantCulture,
				"This {0} has completed; it is no longer usable.",
				type.Name));
		}

		internal static InvalidOperationException ParametersNotInitialized (int parameterPosition,string parameterName,string parameterType)
		{
			object [] args = new object [] { parameterPosition, parameterName, parameterType };
			return new InvalidOperationException  (GetExceptionMessage ("Parameter {0}: '{1}', the property DbType is uninitialized: OleDbType.{2}.",args));
		}

		internal static InvalidOperationException WrongParameterSize(string provider)
		{
			string [] args = new string [] { provider };
			return new InvalidOperationException  (GetExceptionMessage ("{0}.Prepare method requires all variable length parameters to have an explicitly set non-zero Size.",args));
		}

		internal static InvalidOperationException ConnectionNotOpened (string operationName, string connectionState)
		{
			object [] args = new object [] { operationName, connectionState };
			return new InvalidOperationException  (GetExceptionMessage ("{0} requires an open and available Connection. The connection's current state is {1}.",args));
		}

		internal static InvalidOperationException ConnectionNotInitialized (string methodName)
		{
			object [] args = new object [] { methodName };
			return new InvalidOperationException (GetExceptionMessage ("{0}: Connection property has not been initialized.",args));
		}

		internal static InvalidOperationException OpenConnectionRequired (string methodName, object connectionState)
		{
			object [] args = new object [] { methodName, connectionState };
			return new InvalidOperationException (GetExceptionMessage ("{0} requires an open and available Connection. The connection's current state is {1}.",args));
		}

		internal static InvalidOperationException OpenedReaderExists ()
		{
			return new InvalidOperationException (GetExceptionMessage ("There is already an open DataReader associated with this Connection which must be closed first."));
		}

		internal static InvalidOperationException ConnectionAlreadyOpen (object connectionState)
		{
			object [] args = new object [] { connectionState };
			return new InvalidOperationException (GetExceptionMessage ("The connection is already Open (state={0}).",args));
		}

		internal static InvalidOperationException ConnectionClosed ()
		{
			return new InvalidOperationException ("Invalid operation. The Connection is closed.");
		}

		internal static InvalidOperationException ConnectionStringNotInitialized ()
		{
			return new InvalidOperationException (GetExceptionMessage ("The ConnectionString property has not been initialized."));
		}

		internal static InvalidOperationException ConnectionIsBusy (object commandType,object connectionState)
		{
			object [] args = new object [] { commandType.ToString (), connectionState.ToString () };
			return new InvalidOperationException (GetExceptionMessage ("The {0} is currently busy {1}.",args));
		}

		internal static InvalidOperationException NotAllowedWhileConnectionOpen (string propertyName, object connectionState)
		{
			object [] args = new object [] { propertyName, connectionState };
			return new InvalidOperationException (GetExceptionMessage ("Not allowed to change the '{0}' property while the connection (state={1}).",args));
		}

		internal static ArgumentException OleDbNoProviderSpecified ()
		{
			return new ArgumentException (GetExceptionMessage ("An OLE DB Provider was not specified in the ConnectionString.  An example would be, 'Provider=SQLOLEDB;'."));
		}

		internal static ArgumentException InvalidValueForKey (string key)
		{
			string [] args = new string [] { key };
			return new ArgumentException (String.Format ("Invalid value for key {0}",args));
		}

		internal static InvalidOperationException ParameterSizeNotInitialized( int parameterIndex, string parameterName,string parameterType,int parameterSize)
		{
			object [] args = new object [] { parameterIndex.ToString (), parameterName, parameterType, parameterSize.ToString () };
			return new InvalidOperationException (GetExceptionMessage ("Parameter {0}: '{1}' of type: {2}, the property Size has an invalid size: {3}",args));
		}

		internal static ArgumentException InvalidUpdateStatus (UpdateStatus status)
		{
			object [] args = new object [] { status };
			return new ArgumentException (GetExceptionMessage ("Invalid UpdateStatus: {0}",args));
		}

		internal static InvalidOperationException UpdateRequiresCommand (string command)
		{
			object [] args = new object [] { command };
			return new InvalidOperationException (GetExceptionMessage ("Auto SQL generation during {0} requires a valid SelectCommand.",args));
		}

		internal static DataException RowUpdatedError ()
		{
			return new DataException (GetExceptionMessage ("RowUpdatedEvent: Errors occurred; no additional is information available."));
		}

		internal static ArgumentNullException CollectionNoNullsAllowed (object collection, object objectsType)
		{
			object [] args = new object [] {collection.GetType ().ToString (), objectsType.ToString ()};
			return new ArgumentNullException (GetExceptionMessage ("The {0} only accepts non-null {1} type objects.", args));
		}

		internal static ArgumentException CollectionAlreadyContains(object objectType,string propertyName, object propertyValue, object collection)
		{
			object [] args = new object [] {objectType.ToString (), propertyName, propertyValue, collection.GetType ().ToString ()};
			return new ArgumentException (GetExceptionMessage ("The {0} with {1} '{2}' is already contained by this {3}.",args));
		}

		internal static string GetExceptionMessage (string exceptionMessage,object [] args)
		{
			if ((args == null) || (args.Length == 0)) {
				return exceptionMessage;
			} else {
				return String.Format (exceptionMessage,args);
			}
		}

		internal static string GetExceptionMessage (string exceptionMessage)
		{
			return GetExceptionMessage (exceptionMessage, null);
		}
	}
}
