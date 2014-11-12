// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>[....]</OWNER>
// <OWNER>[....]</OWNER>
// <OWNER>[....]</OWNER>

using System;

namespace System.Runtime.InteropServices.WindowsRuntime
{
    [ComImport]
    [Guid("629bdbc8-d932-4ff4-96b9-8d96c5c1e858")]
    [WindowsRuntimeImport]
    internal interface IPropertyValueFactory
    {
        IPropertyValue CreateEmpty();
        IPropertyValue CreateUInt8(byte value);
        IPropertyValue CreateInt16(short value);
        IPropertyValue CreateUInt16(ushort value);
        IPropertyValue CreateInt32(int value);
        IPropertyValue CreateUInt32(uint value);
        IPropertyValue CreateInt64(long value);
        IPropertyValue CreateUInt64(ulong value);
        IPropertyValue CreateSingle(float value);
        IPropertyValue CreateDouble(double value);
        IPropertyValue CreateChar16(char value);
        IPropertyValue CreateBoolean(bool value);
        IPropertyValue CreateString(string value);
        IPropertyValue CreateInspectable(object value);
        IPropertyValue CreateGuid(Guid value);
        IPropertyValue CreateDateTime(DateTimeOffset value);
        IPropertyValue CreateTimeSpan(TimeSpan value);
        IPropertyValue CreatePoint(Point value);
        IPropertyValue CreateSize(Size value);
        IPropertyValue CreateRect(Rect value);
        IPropertyValue CreateUInt8Array(byte[] value);
        IPropertyValue CreateInt16Array(short[] value);
        IPropertyValue CreateUInt16Array(ushort[] value);
        IPropertyValue CreateInt32Array(Int32[] value);
        IPropertyValue CreateUInt32Array(UInt32[] value);
        IPropertyValue CreateInt64Array(Int64[] value);
        IPropertyValue CreateUInt64Array(UInt64[] value);
        IPropertyValue CreateSingleArray(Single[] value);
        IPropertyValue CreateDoubleArray(Double[] value);
        IPropertyValue CreateChar16Array(Char[] value);
        IPropertyValue CreateBooleanArray(Boolean[] value);
        IPropertyValue CreateStringArray(String[] value);
        IPropertyValue CreateInspectableArray(Object[] value);
        IPropertyValue CreateGuidArray(Guid[] value);
        IPropertyValue CreateDateTimeArray(DateTimeOffset[] value);
        IPropertyValue CreateTimeSpanArray(TimeSpan[] value);
        IPropertyValue CreatePointArray(Point[] value);
        IPropertyValue CreateSizeArray(Size[] value);
        IPropertyValue CreateRectArray(Rect[] value);        
    }
}
