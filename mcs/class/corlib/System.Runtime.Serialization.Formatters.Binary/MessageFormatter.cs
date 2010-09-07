//
// System.Runtime.Remoting.MessageFormatter.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class MessageFormatter
	{
		public static void WriteMethodCall (BinaryWriter writer, object obj, Header[] headers, BinaryFormatter formatter)
		{
			IMethodCallMessage call = (IMethodCallMessage)obj;
			writer.Write ((byte) BinaryElement.MethodCall);

			MethodFlags methodFlags;
			int infoArraySize = 0;
			object info = null;
			object[] extraProperties = null;

			if (call.LogicalCallContext != null && call.LogicalCallContext.HasInfo)
			{
				methodFlags = MethodFlags.IncludesLogicalCallContext;
				infoArraySize++;
			}
			else
				methodFlags = MethodFlags.ExcludeLogicalCallContext;

			if (RemotingServices.IsMethodOverloaded (call))
			{
				infoArraySize++;
				methodFlags |= MethodFlags.IncludesSignature;
			}

			if (call.Properties.Count > MethodCallDictionary.InternalKeys.Length)
			{
				extraProperties = GetExtraProperties (call.Properties, MethodCallDictionary.InternalKeys);
				infoArraySize++;
			}

			if (call.MethodBase.IsGenericMethod) {
				infoArraySize++;
				methodFlags |= MethodFlags.GenericArguments;
			}
			if (call.ArgCount == 0)
				methodFlags |= MethodFlags.NoArguments;
			else {
				if (AllTypesArePrimitive (call.Args)) 
					methodFlags |= MethodFlags.PrimitiveArguments;
				else {
					if (infoArraySize == 0)
						methodFlags |= MethodFlags.ArgumentsInSimpleArray;
					else {
						methodFlags |= MethodFlags.ArgumentsInMultiArray;
						infoArraySize++;
					}
				}
			}

			writer.Write ((int) methodFlags);

			// Method name
			writer.Write ((byte) BinaryTypeCode.String);
			writer.Write (call.MethodName);

			// Class name
			writer.Write ((byte) BinaryTypeCode.String);
			writer.Write (call.TypeName);

			// Arguments

			if ((methodFlags & MethodFlags.PrimitiveArguments) > 0)
			{
				writer.Write ((uint)call.Args.Length);
				for (int n=0; n<call.ArgCount; n++)
				{
					object arg = call.GetArg(n);
					if (arg != null) {
						writer.Write (BinaryCommon.GetTypeCode (arg.GetType()));
						ObjectWriter.WritePrimitiveValue (writer, arg);
					}
					else
						writer.Write ((byte)BinaryTypeCode.Null);
				}
			}

			if ( infoArraySize > 0)
			{
				object[] ainfo = new object[infoArraySize];
				int n=0;
				if ((methodFlags & MethodFlags.ArgumentsInMultiArray) > 0) ainfo[n++] = call.Args;
				if ((methodFlags & MethodFlags.GenericArguments) > 0) ainfo[n++] = call.MethodBase.GetGenericArguments ();
				if ((methodFlags & MethodFlags.IncludesSignature) > 0) ainfo[n++] = call.MethodSignature;
				if ((methodFlags & MethodFlags.IncludesLogicalCallContext) > 0) ainfo[n++] = call.LogicalCallContext;
				if (extraProperties != null) ainfo[n++] = extraProperties;
				info = ainfo;
			}
			else if ((methodFlags & MethodFlags.ArgumentsInSimpleArray) > 0)
				info = call.Args;

			if (info != null)
			{
				ObjectWriter objectWriter = new ObjectWriter (formatter);
				objectWriter.WriteObjectGraph (writer, info, headers);
			}
			else
				writer.Write ((byte) BinaryElement.End);
		}

		public static void WriteMethodResponse (BinaryWriter writer, object obj, Header[] headers, BinaryFormatter formatter)
		{
			IMethodReturnMessage resp = (IMethodReturnMessage)obj;
			writer.Write ((byte) BinaryElement.MethodResponse);

			string[] internalProperties = MethodReturnDictionary.InternalReturnKeys;

			int infoArrayLength = 0;
			object info = null;
			object[] extraProperties = null;

			// Type of return value

			ReturnTypeTag returnTypeTag;
			MethodFlags contextFlag = MethodFlags.ExcludeLogicalCallContext;

			if (resp.Exception != null) {
				returnTypeTag = ReturnTypeTag.Exception | ReturnTypeTag.Null;
				internalProperties = MethodReturnDictionary.InternalExceptionKeys;
				infoArrayLength = 1;
			}
			else if (resp.ReturnValue == null) {
				returnTypeTag = ReturnTypeTag.Null;
			}
			else if (IsMethodPrimitive(resp.ReturnValue.GetType())) {
				returnTypeTag = ReturnTypeTag.PrimitiveType;
			}
			else {
				returnTypeTag = ReturnTypeTag.ObjectType;
				infoArrayLength++;
			}

			// Message flags

			MethodFlags formatFlag;

			if ((resp.LogicalCallContext != null) && resp.LogicalCallContext.HasInfo) 
			{
				contextFlag = MethodFlags.IncludesLogicalCallContext;
				infoArrayLength++;
			}

			if (resp.Properties.Count > internalProperties.Length && ((returnTypeTag & ReturnTypeTag.Exception) == 0))
			{
				extraProperties = GetExtraProperties (resp.Properties, internalProperties);
				infoArrayLength++;
			}

			if (resp.OutArgCount == 0)
				formatFlag = MethodFlags.NoArguments;
			else 
			{
				if (AllTypesArePrimitive (resp.Args)) 
					formatFlag = MethodFlags.PrimitiveArguments;
				else 
				{
					if (infoArrayLength == 0)
						formatFlag = MethodFlags.ArgumentsInSimpleArray; 
					else {
						formatFlag = MethodFlags.ArgumentsInMultiArray;
						infoArrayLength++;
					}
				}
			}

			writer.Write ((byte) (contextFlag | formatFlag));
			writer.Write ((byte) returnTypeTag);

			// FIXME: what are the following 2 bytes for?
			writer.Write ((byte) 0);
			writer.Write ((byte) 0);

			// Arguments

			if (returnTypeTag == ReturnTypeTag.PrimitiveType)
			{
				writer.Write (BinaryCommon.GetTypeCode (resp.ReturnValue.GetType()));
				ObjectWriter.WritePrimitiveValue (writer, resp.ReturnValue);
			}

			if (formatFlag == MethodFlags.PrimitiveArguments)
			{
				writer.Write ((uint)resp.ArgCount);
				for (int n=0; n<resp.ArgCount; n++)
				{
					object val = resp.GetArg(n);
					if (val != null) {
						writer.Write (BinaryCommon.GetTypeCode (val.GetType()));
						ObjectWriter.WritePrimitiveValue (writer, val);
					}
					else
						writer.Write ((byte)BinaryTypeCode.Null);
				}
			}

			if (infoArrayLength > 0)
			{
				object[] infoArray = new object[infoArrayLength];
				int n = 0;

				if ((returnTypeTag & ReturnTypeTag.Exception) != 0)
					infoArray[n++] = resp.Exception;
				
				if (formatFlag == MethodFlags.ArgumentsInMultiArray)
					infoArray[n++] = resp.Args;

				if (returnTypeTag == ReturnTypeTag.ObjectType)
					infoArray[n++] = resp.ReturnValue;

				if (contextFlag == MethodFlags.IncludesLogicalCallContext)
					infoArray[n++] = resp.LogicalCallContext;

				if (extraProperties != null)
					infoArray[n++] = extraProperties;

				info = infoArray;
			}
			else if ((formatFlag & MethodFlags.ArgumentsInSimpleArray) > 0)
				info = resp.Args;

			if (info != null)
			{
				ObjectWriter objectWriter = new ObjectWriter (formatter);
				objectWriter.WriteObjectGraph (writer, info, headers);
			}
			else
				writer.Write ((byte) BinaryElement.End);
		}

		public static object ReadMethodCall (BinaryReader reader, bool hasHeaders, HeaderHandler headerHandler, BinaryFormatter formatter)
		{
			BinaryElement elem = (BinaryElement)reader.ReadByte();	// The element code
			return ReadMethodCall (elem, reader, hasHeaders, headerHandler, formatter);
		}

		public static object ReadMethodCall (BinaryElement elem, BinaryReader reader, bool hasHeaders, HeaderHandler headerHandler, BinaryFormatter formatter)
		{
			if (elem != BinaryElement.MethodCall) throw new SerializationException("Invalid format. Expected BinaryElement.MethodCall, found " +  elem);

			MethodFlags flags = (MethodFlags) reader.ReadInt32();

			if (((BinaryTypeCode)reader.ReadByte()) != BinaryTypeCode.String) throw new SerializationException ("Invalid format");
			string methodName = reader.ReadString();

			if (((BinaryTypeCode)reader.ReadByte()) != BinaryTypeCode.String) throw new SerializationException ("Invalid format");
			string className = reader.ReadString();

			//bool hasContextInfo = (flags & MethodFlags.IncludesLogicalCallContext) > 0;

			object[] arguments = null;
			object methodSignature = null;
			object callContext = null;
			object[] extraProperties = null;
			Header[] headers = null;
			Type[] genericArguments = null;

			if ((flags & MethodFlags.PrimitiveArguments) > 0)
			{
				uint count = reader.ReadUInt32();
				arguments = new object[count];
				for (int n=0; n<count; n++)
				{
					Type type = BinaryCommon.GetTypeFromCode (reader.ReadByte());
					arguments[n] = ObjectReader.ReadPrimitiveTypeValue (reader, type);
				}
			}

			if ((flags & MethodFlags.NeedsInfoArrayMask) > 0)
			{
				ObjectReader objectReader = new ObjectReader (formatter);

				object result;
				objectReader.ReadObjectGraph (reader, hasHeaders, out result, out headers);
				object[] msgInfo = (object[]) result;

				if ((flags & MethodFlags.ArgumentsInSimpleArray) > 0) {
					arguments = msgInfo;
				}
				else
				{
					int n = 0;
					if ((flags & MethodFlags.ArgumentsInMultiArray) > 0) {
						if (msgInfo.Length > 1) arguments = (object[]) msgInfo[n++];
						else arguments = new object[0];
					}

					if ((flags & MethodFlags.GenericArguments) > 0)
						genericArguments = (Type[]) msgInfo[n++];

					if ((flags & MethodFlags.IncludesSignature) > 0)
						methodSignature = msgInfo[n++];

					if ((flags & MethodFlags.IncludesLogicalCallContext) > 0) 
						callContext = msgInfo[n++];

					if (n < msgInfo.Length)
						extraProperties = (object[]) msgInfo[n];
				}
			}
			else {
				reader.ReadByte ();	// Reads the stream ender
			}

			if (arguments == null) arguments = new object[0];

			string uri = null;
			if (headerHandler != null)
				uri = headerHandler(headers) as string;

			Header[] methodInfo = new Header[7];
			methodInfo[0] = new Header("__MethodName", methodName);
			methodInfo[1] = new Header("__MethodSignature", methodSignature);
			methodInfo[2] = new Header("__TypeName", className);
			methodInfo[3] = new Header("__Args", arguments);
			methodInfo[4] = new Header("__CallContext", callContext);
			methodInfo[5] = new Header("__Uri", uri);
			methodInfo[6] = new Header("__GenericArguments", genericArguments);

			MethodCall call = new MethodCall (methodInfo);

			if (extraProperties != null) {
				foreach (DictionaryEntry entry in extraProperties)
					call.Properties [(string)entry.Key] = entry.Value;
			}

			return call;
		}

		public static object ReadMethodResponse (BinaryReader reader, bool hasHeaders, HeaderHandler headerHandler, IMethodCallMessage methodCallMessage, BinaryFormatter formatter)
		{
			BinaryElement elem = (BinaryElement) reader.ReadByte ();
			return ReadMethodResponse (elem, reader, hasHeaders, headerHandler, methodCallMessage, formatter);
		}

		public static object ReadMethodResponse (BinaryElement elem, BinaryReader reader, bool hasHeaders, HeaderHandler headerHandler, IMethodCallMessage methodCallMessage, BinaryFormatter formatter)
		{
			if (elem != BinaryElement.MethodResponse) throw new SerializationException("Invalid format. Expected BinaryElement.MethodResponse, found " +  elem);

			MethodFlags flags = (MethodFlags) reader.ReadByte ();
			ReturnTypeTag typeTag = (ReturnTypeTag) reader.ReadByte ();
			bool hasContextInfo = (flags & MethodFlags.IncludesLogicalCallContext) > 0;

			// FIXME: find a meaning for those 2 bytes
			reader.ReadByte();
			reader.ReadByte();

			object returnValue = null;
			object[] outArgs = null;
			LogicalCallContext callContext = null;
			Exception exception = null;
			object[] extraProperties = null;
			Header[] headers = null;

			if ((typeTag & ReturnTypeTag.PrimitiveType) > 0)
			{
				Type type = BinaryCommon.GetTypeFromCode (reader.ReadByte());
				returnValue = ObjectReader.ReadPrimitiveTypeValue (reader, type);
			}

			if ((flags & MethodFlags.PrimitiveArguments) > 0)
			{
				uint count = reader.ReadUInt32();
				outArgs = new object[count];
				for (int n=0; n<count; n++) {
					Type type = BinaryCommon.GetTypeFromCode (reader.ReadByte());
					outArgs[n] = ObjectReader.ReadPrimitiveTypeValue (reader, type);
				}
			}

			if (hasContextInfo || (typeTag & ReturnTypeTag.ObjectType) > 0 || 
				(typeTag & ReturnTypeTag.Exception) > 0 ||
				(flags & MethodFlags.ArgumentsInSimpleArray) > 0 || 
				(flags & MethodFlags.ArgumentsInMultiArray) > 0)
			{
				// There objects that need to be deserialized using an ObjectReader

				ObjectReader objectReader = new ObjectReader (formatter);
				object result;
				objectReader.ReadObjectGraph (reader, hasHeaders, out result, out headers);
				object[] msgInfo = (object[]) result;

				if ((typeTag & ReturnTypeTag.Exception) > 0) {
					exception = (Exception) msgInfo[0];
					if (hasContextInfo) callContext = (LogicalCallContext)msgInfo[1];
				}
				else if ((flags & MethodFlags.NoArguments) > 0 || (flags & MethodFlags.PrimitiveArguments) > 0) {
					int n = 0;
					if ((typeTag & ReturnTypeTag.ObjectType) > 0) returnValue = msgInfo [n++];
					if (hasContextInfo) callContext = (LogicalCallContext)msgInfo[n++];
					if (n < msgInfo.Length) extraProperties = (object[]) msgInfo[n];
				}
				else if ((flags & MethodFlags.ArgumentsInSimpleArray) > 0) {
					outArgs = msgInfo;
				}
				else {
					int n = 0;
					outArgs = (object[]) msgInfo[n++];
					if ((typeTag & ReturnTypeTag.ObjectType) > 0) returnValue = msgInfo[n++];
					if (hasContextInfo) callContext = (LogicalCallContext)msgInfo[n++];
					if (n < msgInfo.Length) extraProperties = (object[]) msgInfo[n];
				}
			}
			else {
				reader.ReadByte ();	// Reads the stream ender
			}

			if (headerHandler != null) 
				headerHandler(headers);

			if (exception != null)
				return new ReturnMessage (exception, methodCallMessage);
			else
			{
				int argCount = (outArgs!=null) ? outArgs.Length : 0;
				ReturnMessage result = new ReturnMessage (returnValue, outArgs, argCount, callContext, methodCallMessage);

				if (extraProperties != null) {
					foreach (DictionaryEntry entry in extraProperties)
						result.Properties [(string)entry.Key] = entry.Value;
				}

				return result;
			}
		}

		private static bool AllTypesArePrimitive(object[] objects)
		{
			foreach (object ob in objects) 
			{
				if (ob != null && !IsMethodPrimitive(ob.GetType())) 
					return false;
			}
			return true;
		}

		// When serializing methods, string are considered primitive types
		public static bool IsMethodPrimitive (Type type)
		{
			return type.IsPrimitive || type == typeof(string) || type == typeof (DateTime) || type == typeof (Decimal);
		}

		static object[] GetExtraProperties (IDictionary properties, string[] internalKeys)
		{
			object[] extraProperties = new object [properties.Count - internalKeys.Length];
			
			int n = 0;
			IDictionaryEnumerator e = properties.GetEnumerator();
			while (e.MoveNext())
				if (!IsInternalKey ((string) e.Entry.Key, internalKeys)) extraProperties [n++] = e.Entry;

			return extraProperties;
		}

		static bool IsInternalKey (string key, string[] internalKeys)
		{
			foreach (string ikey in internalKeys)
				if (key == ikey) return true;
			return false;
		}

	}
}
