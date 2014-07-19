// NativeWrapper.cpp

#include "NativeWrapper.h"

#include <sqlite3.h>

using namespace Mono::Data::Sqlite::DllImport;
using namespace Platform;
using namespace std;

int32 NativeWrapper::sqlite3_open(const Platform::Array<uint8>^ filename, int64* db)
{
	sqlite3* sqlite3 = nullptr;

	int32 result = ::sqlite3_open(filename ? (const char*)filename->Data : nullptr, &sqlite3);

	if (db)
	{
		*db = (int64)sqlite3;
	}

	return result;
}

int32 NativeWrapper::sqlite3_open16(const Platform::Array<uint8>^ filename, int64* db)
{
	sqlite3* sqlite3 = nullptr;

	int32 result = ::sqlite3_open16(filename ? (const char*)filename->Data : nullptr, &sqlite3);

	if (db)
	{
		*db = (int64)sqlite3;
	}

	return result;
}

int32 NativeWrapper::sqlite3_open_v2(const Platform::Array<uint8>^ filename, int64* db, int32 flags, int64 zVfs)
{
	sqlite3* sqlite3 = nullptr;

	int32 result = ::sqlite3_open_v2(filename ? (const char*)filename->Data : nullptr, &sqlite3, flags, (const char*)zVfs);

	if (db)
	{
		*db = (int64)sqlite3;
	}

	return result;
}

int32 NativeWrapper::sqlite3_close(int64 db)
{
	return ::sqlite3_close((sqlite3*)db);
}

int32 NativeWrapper::sqlite3_close_v2(int64 db)
{
	return ::sqlite3_close_v2((sqlite3*)db);
}

void NativeWrapper::sqlite3_interrupt(int64 db)
{
	::sqlite3_interrupt((sqlite3*)db);
}

int32 NativeWrapper::sqlite3_prepare(int64 db, int64 zSql, int32 nByte, int64* ppStmpt, int64* pzTail)
{
	sqlite3_stmt* sqlite3_stmt = nullptr;
	const char* tail = nullptr;

	int32 result = ::sqlite3_prepare((sqlite3*)db, (const char*)zSql, nByte, &sqlite3_stmt, &tail);

	if (ppStmpt)
	{
		*ppStmpt = (int64)sqlite3_stmt;
	}
	if (pzTail)
	{
		*pzTail = (int64)tail;
	}

	return result;
}

int32 NativeWrapper::sqlite3_prepare_v2(int64 db, int64 zSql, int32 nByte, int64* ppStmpt, int64* pzTail)
{
	sqlite3_stmt* sqlite3_stmt = nullptr;
	const char* tail = nullptr;

	int32 result = ::sqlite3_prepare_v2((sqlite3*)db, (const char*)zSql, nByte, &sqlite3_stmt, &tail);

	if (ppStmpt)
	{
		*ppStmpt = (int64)sqlite3_stmt;
	}
	if (pzTail)
	{
		*pzTail = (int64)tail;
	}

	return result;
}

int64 NativeWrapper::sqlite3_errmsg(int64 db)
{
	return (int64)::sqlite3_errmsg((sqlite3*)db);
}

int64 NativeWrapper::sqlite3_next_stmt(int64 db, int64 stmHandle)
{
	return (int64)::sqlite3_next_stmt((sqlite3*)db, (sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_changes(int64 db)
{
	return ::sqlite3_changes((sqlite3*)db);
}

int32 NativeWrapper::sqlite3_create_function(int64 db, const Platform::Array<uint8>^ zFunctionName, int32 nArg, int32 eTextRep, int64 pApp, int64 xFunc, int64 xStep, int64 xFinal)
{
	return ::sqlite3_create_function((sqlite3*)db, zFunctionName ? (const char*)zFunctionName->Data : nullptr, nArg, eTextRep, (void*)pApp, (void(*)(sqlite3_context*, int, sqlite3_value**))xFunc, (void(*)(sqlite3_context*, int, sqlite3_value**))xStep, (void(*)(sqlite3_context*))xFinal);
}

int32 NativeWrapper::sqlite3_key(int64 db, const Platform::Array<uint8>^ key, int32 keylen)
{
	throw ref new NotImplementedException();
}

int32 NativeWrapper::sqlite3_rekey(int64 db, const Platform::Array<uint8>^ key, int32 keylen)
{
	throw ref new NotImplementedException();
}

int64 NativeWrapper::sqlite3_commit_hook(int64 db, int64 xCallback, int64 pArg)
{
	return (int64)::sqlite3_commit_hook((sqlite3*)db, (int(*)(void*))xCallback, (void*)pArg);
}

int64 NativeWrapper::sqlite3_rollback_hook(int64 db, int64 xCallback, int64 pArg)
{
	return (int64)::sqlite3_rollback_hook((sqlite3*)db, (void(*)(void*))xCallback, (void*)pArg);
}

int64 NativeWrapper::sqlite3_update_hook(int64 db, int64 xCallback, int64 pArg)
{
	return (int64)::sqlite3_update_hook((sqlite3*)db, (void(*)(void*, int, const char*, const char*, sqlite3_int64))xCallback, (void*)pArg);
}

int32 NativeWrapper::sqlite3_busy_timeout(int64 db, int32 ms)
{
	return ::sqlite3_busy_timeout((sqlite3*)db, ms);
}

int32 NativeWrapper::sqlite3_exec(int64 db, const Platform::Array<uint8>^ zSql, int64 xCallback, int64 pArg, int64* pzErrMsg)
{
	char *errMsg = nullptr;

	int32 result = ::sqlite3_exec((sqlite3*)db, zSql ? (const char*)zSql->Data : nullptr, (int(*)(void*, int, char**, char**))xCallback, (void*)pArg, &errMsg);

	if (pzErrMsg)
	{
		*pzErrMsg = (int64)errMsg;
	}

	return result;
}

int32 NativeWrapper::sqlite3_table_column_metadata(int64 db, const Platform::Array<uint8>^ zDbName, const Platform::Array<uint8>^ zTableName, const Platform::Array<uint8>^ zColumnName, int64* pzDataType, int64* pzCollSeq, int32* pNotNull, int32* pPrimaryKey, int32* pAutoinc)
{
	char const *dataType = nullptr;
	char const *collSeq = nullptr;
	int notNull = 0;
	int primaryKey = 0;
	int autoinc = 0;

	int32 result = ::sqlite3_table_column_metadata((sqlite3*)db, zDbName ? (const char*)zDbName->Data : nullptr, zTableName ? (const char*)zTableName->Data : nullptr, zColumnName ? (const char*)zColumnName->Data : nullptr, &dataType, &collSeq, pNotNull, pPrimaryKey, pAutoinc);

	if (pzDataType)
	{
		*pzDataType = (int64)dataType;
	}
	if (pzCollSeq)
	{
		*pzCollSeq = (int64)collSeq;
	}

	return result;
}

int32 NativeWrapper::sqlite3_bind_int(int64 stmHandle, int32 iParam, int32 value)
{
	return ::sqlite3_bind_int((sqlite3_stmt*)stmHandle, iParam, value);
}

int32 NativeWrapper::sqlite3_bind_int64(int64 stmHandle, int32 iParam, int64 value)
{
	return ::sqlite3_bind_int64((sqlite3_stmt*)stmHandle, iParam, (sqlite3_int64)value);
}

int32 NativeWrapper::sqlite3_bind_text(int64 stmHandle, int32 iParam, const Platform::Array<uint8>^ value, int32 length, int64 destructor)
{
	return ::sqlite3_bind_text((sqlite3_stmt*)stmHandle, iParam, value ? (const char*)value->Data : nullptr, length, (void(*)(void*))destructor);
}

int32 NativeWrapper::sqlite3_bind_text16(int64 stmHandle, int32 iParam, const Platform::Array<uint8>^ value, int32 length, int64 destructor)
{
	return ::sqlite3_bind_text16((sqlite3_stmt*)stmHandle, iParam, value ? (const char*)value->Data : nullptr, length, (void(*)(void*))destructor);
}

int32 NativeWrapper::sqlite3_bind_double(int64 stmHandle, int32 iParam, float64 value)
{
	return ::sqlite3_bind_double((sqlite3_stmt*)stmHandle, iParam, value);
}

int32 NativeWrapper::sqlite3_bind_blob(int64 stmHandle, int32 iParam, const Array<uint8>^ value, int32 length, int64 destructor)
{
	return ::sqlite3_bind_blob((sqlite3_stmt*)stmHandle, iParam, value ? value->Data : nullptr, length, (void(*)(void*))destructor);
}

int32 NativeWrapper::sqlite3_bind_null(int64 stmHandle, int32 iParam)
{
	return ::sqlite3_bind_null((sqlite3_stmt*)stmHandle, iParam);
}

int32 NativeWrapper::sqlite3_bind_parameter_count(int64 stmHandle)
{
	return ::sqlite3_bind_parameter_count((sqlite3_stmt*)stmHandle);
}

int64 NativeWrapper::sqlite3_bind_parameter_name(int64 stmHandle, int32 iParam)
{
	return (int64)::sqlite3_bind_parameter_name((sqlite3_stmt*)stmHandle, iParam);
}

int32 NativeWrapper::sqlite3_bind_parameter_index(int64 stmHandle, const Platform::Array<uint8>^ paramName)
{
	return ::sqlite3_bind_parameter_index((sqlite3_stmt*)stmHandle, paramName ? (const char*)paramName->Data : nullptr);
}

int32 NativeWrapper::sqlite3_step(int64 stmHandle)
{
	return ::sqlite3_step((sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_column_int(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_int((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_int64(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_int64((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_text(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_text((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_text16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_text16((sqlite3_stmt*)stmHandle, iCol);
}

float64 NativeWrapper::sqlite3_column_double(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_double((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_blob(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_blob((sqlite3_stmt*)stmHandle, iCol);
}

int32 NativeWrapper::sqlite3_column_type(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_type((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_decltype(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_decltype((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_decltype16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_decltype16((sqlite3_stmt*)stmHandle, iCol);
}

int32 NativeWrapper::sqlite3_column_bytes(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_bytes((sqlite3_stmt*)stmHandle, iCol);
}

int32 NativeWrapper::sqlite3_column_count(int64 stmHandle)
{
	return ::sqlite3_column_count((sqlite3_stmt*)stmHandle);
}

int64 NativeWrapper::sqlite3_column_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_name16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_name16((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_origin_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_origin_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_origin_name16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_origin_name16((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_table_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_table_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_table_name16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_table_name16((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_database_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_database_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 NativeWrapper::sqlite3_column_database_name16(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_database_name16((sqlite3_stmt*)stmHandle, iCol);
}

int32 NativeWrapper::sqlite3_data_count(int64 stmHandle)
{
	return ::sqlite3_data_count((sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_reset(int64 stmHandle)
{
	return ::sqlite3_reset((sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_clear_bindings(int64 stmHandle)
{
	return ::sqlite3_clear_bindings((sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_finalize(int64 stmHandle)
{
	return ::sqlite3_finalize((sqlite3_stmt*)stmHandle);
}

int32 NativeWrapper::sqlite3_value_int(int64 value)
{
	return ::sqlite3_value_int((sqlite3_value*)value);
}

int64 NativeWrapper::sqlite3_value_int64(int64 value)
{
	return ::sqlite3_value_int64((sqlite3_value*)value);
}

int64 NativeWrapper::sqlite3_value_text(int64 value)
{
	return (int64)::sqlite3_value_text((sqlite3_value*)value);
}

int64 NativeWrapper::sqlite3_value_text16(int64 value)
{
	return (int64)::sqlite3_value_text16((sqlite3_value*)value);
}

float64 NativeWrapper::sqlite3_value_double(int64 value)
{
	return ::sqlite3_value_double((sqlite3_value*)value);
}

int64 NativeWrapper::sqlite3_value_blob(int64 value)
{
	return (int64)::sqlite3_value_blob((sqlite3_value*)value);
}

int32 NativeWrapper::sqlite3_value_type(int64 value)
{
	return ::sqlite3_value_type((sqlite3_value*)value);
}

int32 NativeWrapper::sqlite3_value_bytes(int64 value)
{
	return ::sqlite3_value_bytes((sqlite3_value*)value);
}

void NativeWrapper::sqlite3_result_int(int64 context, int32 result)
{
	::sqlite3_result_int((sqlite3_context*)context, result);
}

void NativeWrapper::sqlite3_result_int64(int64 context, int64 result)
{
	::sqlite3_result_int64((sqlite3_context*)context, (sqlite3_int64)result);
}

void NativeWrapper::sqlite3_result_text(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor)
{
	::sqlite3_result_text((sqlite3_context*)context, result ? (const char*)result->Data : nullptr, length, (void(*)(void*))destructor);
}

void NativeWrapper::sqlite3_result_text16(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor)
{
	::sqlite3_result_text16((sqlite3_context*)context, result ? (const char*)result->Data : nullptr, length, (void(*)(void*))destructor);
}

void NativeWrapper::sqlite3_result_double(int64 context, float64 result)
{
	::sqlite3_result_double((sqlite3_context*)context, result);
}

void NativeWrapper::sqlite3_result_blob(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor)
{
	::sqlite3_result_blob((sqlite3_context*)context, result ? result->Data : nullptr, length, (void(*)(void*))destructor);
}

void NativeWrapper::sqlite3_result_null(int64 context)
{
	::sqlite3_result_null((sqlite3_context*)context);
}

void NativeWrapper::sqlite3_result_error(int64 context, const Platform::Array<uint8>^ result, int32 length)
{
	::sqlite3_result_error((sqlite3_context*)context, result ? (const char*)result->Data : nullptr, length);
}

void NativeWrapper::sqlite3_result_error16(int64 context, const Platform::Array<uint8>^ result, int32 length)
{
	::sqlite3_result_error16((sqlite3_context*)context, result ? (const char*)result->Data : nullptr, length);
}

int64 NativeWrapper::sqlite3_aggregate_context(int64 context, int32 length)
{
	return (int64)::sqlite3_aggregate_context((sqlite3_context*)context, length);
}

int32 NativeWrapper::sqlite3_aggregate_count(int64 context)
{
	return ::sqlite3_aggregate_count((sqlite3_context*)context);
}

int32 NativeWrapper::sqlite3_create_collation(int64 db, const Platform::Array<uint8>^ zName, int32 eTextRep, int64 pArg, int64 xCompare)
{
	return ::sqlite3_create_collation((sqlite3*)db, zName ? (const char*)zName->Data : nullptr, eTextRep, (void*)pArg, (int(*)(void*, int, const void*, int, const void*))xCompare);
}

int64 NativeWrapper::sqlite3_libversion()
{
	return (int64)::sqlite3_libversion();
}

int32 NativeWrapper::sqlite3_config(int32 op)
{
	return ::sqlite3_config(op);
}

void NativeWrapper::sqlite3_free(int64 ptr)
{
	::sqlite3_free((void*)ptr);
}

int64 NativeWrapper::sqlite3_db_handle(int64 stmt)
{
	return (int64)::sqlite3_db_handle((sqlite3_stmt*)stmt);
}
