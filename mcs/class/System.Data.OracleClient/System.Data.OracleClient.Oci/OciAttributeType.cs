// 
// OciAttributeType.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;

namespace System.Data.OracleClient.Oci {
	internal enum OciAttributeType {
		FunctionCode = 0x01,
		ObjectMode = 0x02,
		NonBlockingMode = 0x03,
		SqlCode = 0x04,
		Environment = 0x05,
		Server = 0x06,
		Session = 0x07,
		Transaction = 0x08,
		RowCount = 0x09,
		SqlFunctionCode = 0x0a,
		PrefetchRows = 0x0b,
		NestedPrefetchRows = 0x0c,
		PrefetchMemory = 0x0d,
		NestedPrefetchMemory = 0x0e,
		CharacterCount = 0x0f,
		PackedDecimalScale = 0x10,
		PackedDecimalFormat = 0x11,
		ParameterCount = 0x12,
		RowId = 0x13,
		CharacterSet = 0x14,
		NChar = 0x15,
		// [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Not a secret.")]
		Username = 0x16,
		// [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Not a secret.")]
		Password = 0x17,
		StatementType = 0x18,
		InternalName = 0x19,
		ExternalName = 0x1a,
		TransactionId = 0x1b,
		TransactionLock = 0x1c,
		TransactionName = 0x1d,
		HeapAlloc = 0x1e,
		CharacterSetId = 0x1f,
		CharacterSetForm = 0x20,
		MaxDataSize = 0x21,
		CacheOptimalSize = 0x22,
		CacheMaxSize = 0x23,
		PinOption = 0x24,
		AllocDuration = 0x25,
		PinDuration = 0x26,
		FormatDescriptorObject = 0x27,
		PostProcessingCallback = 0x28,
		PostProcessingContext = 0x29,
		RowsReturned = 0x2a,
		FailoverCallback = 0x2b,
		InV8Mode = 0x2c,
		LobEmpty = 0x2d,
		SessionLanguage = 0x2e,
		DateFormat = 0x4b,

		/* Attributes common to columns and stored procedures */
		DataSize = 0x01,
		DataType = 0x02,
		DisplaySize = 0x03,
		Name = 0x04,
		Precision = 0x05,
		Scale = 0x06,
		IsNull = 0x07,
		TypeName = 0x08,
		SchemaName = 0x09,
		SubName = 0x0a,
		Position = 0x0b,

		/* Only columns */
		DisplayName = 0x64
	}
}
