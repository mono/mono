#pragma once

using namespace Platform;

namespace Mono
{
	namespace Data
	{
		namespace Sqlite
		{
			namespace DllImport
			{
				public ref class NativeWrapper sealed
				{
				public:
					static int32 sqlite3_open(const Platform::Array<uint8>^ filename, int64* db);
					static int32 sqlite3_open16(const Platform::Array<uint8>^ filename, int64* db);
					static int32 sqlite3_open_v2(const Platform::Array<uint8>^ filename, int64* db, int32 flags, int64 zVfs);
					static int32 sqlite3_close(int64 db);
					static int32 sqlite3_close_v2(int64 db);
					static void sqlite3_interrupt(int64 db);
					static int32 sqlite3_prepare(int64 db, int64 zSql, int32 nByte, int64* ppStmpt, int64* pzTail);
					static int32 sqlite3_prepare_v2(int64 db, int64 zSql, int32 nByte, int64* ppStmpt, int64* pzTail);
					static int64 sqlite3_errmsg(int64 db);
					static int64 sqlite3_next_stmt(int64 db, int64 stmHandle);
					static int32 sqlite3_changes(int64 db);
					static int32 sqlite3_create_function(int64 db, const Platform::Array<uint8>^ zFunctionName, int32 nArg, int32 eTextRep, int64 pApp, int64 xFunc, int64 xStep, int64 xFinal);
					static int32 sqlite3_key(int64 db, const Platform::Array<uint8>^ key, int32 keylen);
					static int32 sqlite3_rekey(int64 db, const Platform::Array<uint8>^ key, int32 keylen);
					static int64 sqlite3_commit_hook(int64 db, int64 xCallback, int64 pArg);
					static int64 sqlite3_rollback_hook(int64 db, int64 xCallback, int64 pArg);
					static int64 sqlite3_update_hook(int64 db, int64 xCallback, int64 pArg);
					static int32 sqlite3_busy_timeout(int64 db, int32 ms);
					static int32 sqlite3_exec(int64 db, const Platform::Array<uint8>^ zSql, int64 xCallback, int64 pArg, int64* pzErrMsg);
					static int32 sqlite3_table_column_metadata(int64 db, const Platform::Array<uint8>^ zDbName, const Platform::Array<uint8>^ zTableName, const Platform::Array<uint8>^ zColumnName, int64* pzDataType, int64* pzCollSeq, int32* pNotNull, int32* pPrimaryKey, int32* pAutoinc);
					static int32 sqlite3_bind_int(int64 stmHandle, int32 iParam, int32 value);
					static int32 sqlite3_bind_int64(int64 stmHandle, int32 iParam, int64 value);
					static int32 sqlite3_bind_text(int64 stmHandle, int32 iParam, const Platform::Array<uint8>^ value, int32 length, int64 destructor);
					static int32 sqlite3_bind_text16(int64 stmHandle, int32 iParam, const Platform::Array<uint8>^ value, int32 length, int64 destructor);
					static int32 sqlite3_bind_double(int64 stmHandle, int32 iParam, float64 value);
					static int32 sqlite3_bind_blob(int64 stmHandle, int32 iParam, const Platform::Array<uint8>^ value, int32 length, int64 destructor);
					static int32 sqlite3_bind_null(int64 stmHandle, int32 iParam);
					static int32 sqlite3_bind_parameter_count(int64 stmHandle);
					static int64 sqlite3_bind_parameter_name(int64 stmHandle, int32 iParam);
					static int32 sqlite3_bind_parameter_index(int64 stmHandle, const Platform::Array<uint8>^ paramName);
					static int32 sqlite3_step(int64 stmHandle);
					static int32 sqlite3_column_int(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_int64(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_text(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_text16(int64 stmHandle, int32 iCol);
					static float64 sqlite3_column_double(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_blob(int64 stmHandle, int32 iCol);
					static int32 sqlite3_column_type(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_decltype(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_decltype16(int64 stmHandle, int32 iCol);
					static int32 sqlite3_column_bytes(int64 stmHandle, int32 iCol);
					static int32 sqlite3_column_count(int64 stmHandle);
					static int64 sqlite3_column_name(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_name16(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_origin_name(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_origin_name16(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_table_name(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_table_name16(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_database_name(int64 stmHandle, int32 iCol);
					static int64 sqlite3_column_database_name16(int64 stmHandle, int32 iCol);
					static int32 sqlite3_data_count(int64 stmHandle);
					static int32 sqlite3_reset(int64 stmHandle);
					static int32 sqlite3_clear_bindings(int64 stmHandle);
					static int32 sqlite3_finalize(int64 stmHandle);
					static int32 sqlite3_value_int(int64 value);
					static int64 sqlite3_value_int64(int64 value);
					static int64 sqlite3_value_text(int64 value);
					static int64 sqlite3_value_text16(int64 value);
					static float64 sqlite3_value_double(int64 value);
					static int64 sqlite3_value_blob(int64 value);
					static int32 sqlite3_value_type(int64 value);
					static int32 sqlite3_value_bytes(int64 value);
					static void sqlite3_result_int(int64 context, int32 result);
					static void sqlite3_result_int64(int64 context, int64 result);
					static void sqlite3_result_text(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor);
					static void sqlite3_result_text16(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor);
					static void sqlite3_result_double(int64 context, float64 result);
					static void sqlite3_result_blob(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor);
					static void sqlite3_result_null(int64 context);
					static void sqlite3_result_error(int64 context, const Platform::Array<uint8>^ result, int32 length);
					static void sqlite3_result_error16(int64 context, const Platform::Array<uint8>^ result, int32 length);
					static int64 sqlite3_aggregate_context(int64 context, int32 length);
					static int32 sqlite3_aggregate_count(int64 context);
					static int32 sqlite3_create_collation(int64 db, const Platform::Array<uint8>^ zName, int32 eTextRep, int64 pArg, int64 xCompare);
					static int64 sqlite3_libversion();
					static int32 sqlite3_config(int32 op);
					static void sqlite3_free(int64 ptr);
					static int64 sqlite3_db_handle(int64 stmt);
				};
			}
		}
	}
}
