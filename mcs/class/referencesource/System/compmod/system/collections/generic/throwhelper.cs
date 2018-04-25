namespace System {
    // This file defines an internal class used to throw exceptions in BCL code.
    // The main purpose is to reduce code size. 
    // 
    // The old way to throw an exception generates quite a lot IL code and assembly code.
    // Following is an example:
    //     C# source
    //          throw new ArgumentNullException("key", SR.GetString("ArgumentNull_Key"));
    //     IL code:
    //          IL_0003:  ldstr      "key"
    //          IL_0008:  ldstr      "ArgumentNull_Key"
    //          IL_000d:  call       string System.Environment::GetResourceString(string)
    //          IL_0012:  newobj     instance void System.ArgumentNullException::.ctor(string,string)
    //          IL_0017:  throw
    //    which is 21bytes in IL.
    // 
    // So we want to get rid of the ldstr and call to Environment.GetResource in IL.
    // In order to do that, I created two enums: ExceptionResource, ExceptionArgument to represent the
    // argument name and resource name in a small integer. The source code will be changed to 
    //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key, ExceptionResource.ArgumentNull_Key);
    //
    // The IL code will be 7 bytes.
    //    IL_0008:  ldc.i4.4
    //    IL_0009:  ldc.i4.4
    //    IL_000a:  call       void System.ThrowHelper::ThrowArgumentNullException(valuetype System.ExceptionArgument)
    //    IL_000f:  ldarg.0
    //
    // This will also reduce the Jitted code size a lot. 
    //
    // It is very important we do this for generic classes because we can easily generate the same code 
    // multiple times for different instantiation. 
    // 
    // <










#if !SILVERLIGHT
    using System.Runtime.Serialization;
#endif

    using System.Diagnostics;
    internal static class ThrowHelper {    
        internal static void ThrowWrongKeyTypeArgumentException(object key, Type targetType) {
            throw new ArgumentException(SR.GetString(SR.Arg_WrongType, key, targetType), "key");
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type targetType) {
            throw new ArgumentException(SR.GetString(SR.Arg_WrongType, value, targetType), "value");
        }

        internal static void ThrowKeyNotFoundException() {
            throw new System.Collections.Generic.KeyNotFoundException();
        }
        
        internal static void ThrowArgumentException(ExceptionResource resource) {
            throw new ArgumentException(SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowArgumentNullException(ExceptionArgument argument) {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument) {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource) {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument), SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource) {
            throw new InvalidOperationException(SR.GetString(GetResourceName(resource)));
        }

#if !SILVERLIGHT
        internal static void ThrowSerializationException(ExceptionResource resource) {
            throw new SerializationException(SR.GetString(GetResourceName(resource)));
        }
#endif
        
        internal static void ThrowNotSupportedException(ExceptionResource resource) {
            throw new NotSupportedException(SR.GetString(GetResourceName(resource)));
        }

        // Allow nulls for reference types and Nullable<U>, but not for value types.
        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argName) {
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            if (value == null && !(default(T) == null))
                ThrowHelper.ThrowArgumentNullException(argName);
        }

        //
        // This function will convert an ExceptionArgument enum value to the argument name string.
        //
        internal static string GetArgumentName(ExceptionArgument argument) {
            string argumentName = null;

            switch (argument) {
                case ExceptionArgument.array:
                    argumentName = "array";
                    break;

                case ExceptionArgument.arrayIndex:
                    argumentName = "arrayIndex";
                    break;

                case ExceptionArgument.capacity:
                    argumentName = "capacity";
                    break;

                case ExceptionArgument.collection:
                    argumentName = "collection";
                    break;

                case ExceptionArgument.converter:
                    argumentName = "converter";
                    break;

                case ExceptionArgument.count:
                    argumentName = "count";
                    break;

                case ExceptionArgument.dictionary:
                    argumentName = "dictionary";
                    break;

                case ExceptionArgument.index:
                    argumentName = "index";
                    break;

                case ExceptionArgument.info:
                    argumentName = "info";
                    break;

                case ExceptionArgument.key:
                    argumentName = "key";
                    break;

                case ExceptionArgument.match:
                    argumentName = "match";
                    break;

                case ExceptionArgument.obj:
                    argumentName = "obj";
                    break;

                case ExceptionArgument.queue:
                    argumentName = "queue";
                    break;

                case ExceptionArgument.stack:
                    argumentName = "stack";
                    break;

                case ExceptionArgument.startIndex:
                    argumentName = "startIndex";
                    break;

                case ExceptionArgument.value:
                    argumentName = "value";
                    break;

                case ExceptionArgument.item:
                    argumentName = "item";
                    break;

                default:
                    Debug.Assert(false, "The enum value is not defined, please checked ExceptionArgumentName Enum.");
                    return string.Empty;
            }

            return argumentName;
        }

        //
        // This function will convert an ExceptionResource enum value to the resource string.
        //
        internal static string GetResourceName(ExceptionResource resource) {
            string resourceName = null;

            switch (resource) {
                case ExceptionResource.Argument_ImplementIComparable:
                    resourceName = SR.Argument_ImplementIComparable;
                    break;

                case ExceptionResource.Argument_AddingDuplicate:
                    resourceName = SR.Argument_AddingDuplicate;
                    break;

                case ExceptionResource.ArgumentOutOfRange_Index:
                    resourceName = SR.ArgumentOutOfRange_Index;
                    break;

                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    resourceName = SR.ArgumentOutOfRange_NeedNonNegNum;
                    break;

                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired:
                    resourceName = SR.ArgumentOutOfRange_NeedNonNegNumRequired;
                    break;

                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    resourceName = SR.ArgumentOutOfRange_SmallCapacity;
                    break;

                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    resourceName = SR.Arg_ArrayPlusOffTooSmall;
                    break;

                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    resourceName = SR.Arg_MultiRank;
                    break;

                case ExceptionResource.Arg_NonZeroLowerBound:
                    resourceName = SR.Arg_NonZeroLowerBound;
                    break;

                case ExceptionResource.Argument_InvalidArrayType:
                    resourceName = SR.Invalid_Array_Type;
                    break;

                case ExceptionResource.Argument_InvalidOffLen:
                    resourceName = SR.Argument_InvalidOffLen;
                    break;

                case ExceptionResource.InvalidOperation_CannotRemoveFromStackOrQueue:
                    resourceName = SR.InvalidOperation_CannotRemoveFromStackOrQueue;
                    break;

                case ExceptionResource.InvalidOperation_EmptyCollection:
                    resourceName = SR.InvalidOperation_EmptyCollection;
                    break;

                case ExceptionResource.InvalidOperation_EmptyQueue:
                    resourceName = SR.InvalidOperation_EmptyQueue;
                    break;

                case ExceptionResource.InvalidOperation_EnumOpCantHappen:
                    resourceName = SR.InvalidOperation_EnumOpCantHappen;
                    break;

                case ExceptionResource.InvalidOperation_EnumFailedVersion:
                    resourceName = SR.InvalidOperation_EnumFailedVersion;
                    break;

                case ExceptionResource.InvalidOperation_EmptyStack:
                    resourceName = SR.InvalidOperation_EmptyStack;
                    break;

                case ExceptionResource.InvalidOperation_EnumNotStarted:
                    resourceName = SR.InvalidOperation_EnumNotStarted;
                    break;

                case ExceptionResource.InvalidOperation_EnumEnded:
                    resourceName = SR.InvalidOperation_EnumEnded;
                    break;

                case ExceptionResource.NotSupported_KeyCollectionSet:
                    resourceName = SR.NotSupported_KeyCollectionSet;
                    break;

                case ExceptionResource.NotSupported_SortedListNestedWrite:
                    resourceName = SR.NotSupported_SortedListNestedWrite;
                    break;

#if !SILVERLIGHT
                case ExceptionResource.Serialization_InvalidOnDeser:
                    resourceName = SR.Serialization_InvalidOnDeser;
                    break;

                case ExceptionResource.Serialization_MissingValues:
                    resourceName = SR.Serialization_MissingValues;
                    break;

                case ExceptionResource.Serialization_MismatchedCount:
                    resourceName = SR.Serialization_MismatchedCount;
                    break;
#endif

                case ExceptionResource.NotSupported_ValueCollectionSet:
                    resourceName = SR.NotSupported_ValueCollectionSet;
                    break;

                default:
                    Debug.Assert(false, "The enum value is not defined, please checked ExceptionArgumentName Enum.");
                    return string.Empty;
            }

            return resourceName;
        }

    }

    //
    // The convention for this enum is using the argument name as the enum name
    // 
    internal enum ExceptionArgument {
        obj,
        dictionary,
        array,
        info,
        key,
        collection,
        match,
        converter,
        queue,
        stack,
        capacity,
        index,
        startIndex,
        value,
        count,
        arrayIndex,
        item,
    }

    //
    // The convention for this enum is using the resource name as the enum name
    // 
    internal enum ExceptionResource {
        Argument_ImplementIComparable,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_NeedNonNegNumRequired,
        Arg_ArrayPlusOffTooSmall,
        Argument_AddingDuplicate,
        Serialization_InvalidOnDeser,
        Serialization_MismatchedCount,
        Serialization_MissingValues,
        Arg_RankMultiDimNotSupported,
        Arg_NonZeroLowerBound,
        Argument_InvalidArrayType,
        NotSupported_KeyCollectionSet,
        ArgumentOutOfRange_SmallCapacity,
        ArgumentOutOfRange_Index,
        Argument_InvalidOffLen,
        NotSupported_ReadOnlyCollection,
        InvalidOperation_CannotRemoveFromStackOrQueue,
        InvalidOperation_EmptyCollection,
        InvalidOperation_EmptyQueue,
        InvalidOperation_EnumOpCantHappen,
        InvalidOperation_EnumFailedVersion,
        InvalidOperation_EmptyStack,
        InvalidOperation_EnumNotStarted,
        InvalidOperation_EnumEnded,
        NotSupported_SortedListNestedWrite,
        NotSupported_ValueCollectionSet,
    }
}


