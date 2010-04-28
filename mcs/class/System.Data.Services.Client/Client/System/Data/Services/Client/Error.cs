//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    using System;

    internal static partial class Error
    {
        internal static ArgumentException Argument(string message, string parameterName)
        {
            return Trace(new ArgumentException(message, parameterName));
        }

        internal static InvalidOperationException InvalidOperation(string message)
        {
            return Trace(new InvalidOperationException(message));
        }

        internal static InvalidOperationException InvalidOperation(string message, Exception innerException)
        {
            return Trace(new InvalidOperationException(message, innerException));
        }

        internal static NotSupportedException NotSupported(string message)
        {
            return Trace(new NotSupportedException(message));
        }

        internal static void ThrowObjectDisposed(Type type)
        {
            throw Trace(new ObjectDisposedException(type.ToString()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "errorCode ignored for code sharing")]
        internal static InvalidOperationException HttpHeaderFailure(int errorCode, string message)
        {
            return Trace(new InvalidOperationException(message));
        }

        internal static InvalidOperationException BatchStreamMissingBoundary()
        {
            return InvalidOperation(Strings.BatchStream_MissingBoundary);
        }

        internal static InvalidOperationException BatchStreamContentExpected(BatchStreamState state)
        {
            return InvalidOperation(Strings.BatchStream_ContentExpected(state.ToString()));
        }

        internal static InvalidOperationException BatchStreamContentUnexpected(BatchStreamState state)
        {
            return InvalidOperation(Strings.BatchStream_ContentUnexpected(state.ToString()));
        }

        internal static InvalidOperationException BatchStreamGetMethodNotSupportInChangeset()
        {
            return InvalidOperation(Strings.BatchStream_GetMethodNotSupportedInChangeset);
        }

        internal static InvalidOperationException BatchStreamInvalidBatchFormat()
        {
            return InvalidOperation(Strings.BatchStream_InvalidBatchFormat);
        }

        internal static InvalidOperationException BatchStreamInvalidDelimiter(string delimiter)
        {
            return InvalidOperation(Strings.BatchStream_InvalidDelimiter(delimiter));
        }

        internal static InvalidOperationException BatchStreamMissingEndChangesetDelimiter()
        {
            return InvalidOperation(Strings.BatchStream_MissingEndChangesetDelimiter);
        }

        internal static InvalidOperationException BatchStreamInvalidHeaderValueSpecified(string headerValue)
        {
            return InvalidOperation(Strings.BatchStream_InvalidHeaderValueSpecified(headerValue));
        }

        internal static InvalidOperationException BatchStreamInvalidContentLengthSpecified(string contentLength)
        {
            return InvalidOperation(Strings.BatchStream_InvalidContentLengthSpecified(contentLength));
        }

        internal static InvalidOperationException BatchStreamOnlyGETOperationsCanBeSpecifiedInBatch()
        {
            return InvalidOperation(Strings.BatchStream_OnlyGETOperationsCanBeSpecifiedInBatch);
        }

        internal static InvalidOperationException BatchStreamInvalidOperationHeaderSpecified()
        {
            return InvalidOperation(Strings.BatchStream_InvalidOperationHeaderSpecified);
        }

        internal static InvalidOperationException BatchStreamInvalidHttpMethodName(string methodName)
        {
            return InvalidOperation(Strings.BatchStream_InvalidHttpMethodName(methodName));
        }

        internal static InvalidOperationException BatchStreamMoreDataAfterEndOfBatch()
        {
            return InvalidOperation(Strings.BatchStream_MoreDataAfterEndOfBatch);
        }

        internal static InvalidOperationException BatchStreamInternalBufferRequestTooSmall()
        {
            return InvalidOperation(Strings.BatchStream_InternalBufferRequestTooSmall);
        }

        internal static NotSupportedException MethodNotSupported(System.Linq.Expressions.MethodCallExpression m)
        {
            return Error.NotSupported(Strings.ALinq_MethodNotSupported(m.Method.Name));
        }

        internal static void ThrowBatchUnexpectedContent(InternalError value)
        {
            throw InvalidOperation(Strings.Batch_UnexpectedContent((int)value));
        }

        internal static void ThrowBatchExpectedResponse(InternalError value)
        {
            throw InvalidOperation(Strings.Batch_ExpectedResponse((int)value));
        }

        internal static InvalidOperationException BatchStreamInvalidMethodHeaderSpecified(string header)
        {
            return InvalidOperation(Strings.BatchStream_InvalidMethodHeaderSpecified(header));
        }

        internal static InvalidOperationException BatchStreamInvalidHttpVersionSpecified(string actualVersion, string expectedVersion)
        {
            return InvalidOperation(Strings.BatchStream_InvalidHttpVersionSpecified(actualVersion, expectedVersion));
        }

        internal static InvalidOperationException BatchStreamInvalidNumberOfHeadersAtOperationStart(string header1, string header2)
        {
            return InvalidOperation(Strings.BatchStream_InvalidNumberOfHeadersAtOperationStart(header1, header2));
        }

        internal static InvalidOperationException BatchStreamMissingOrInvalidContentEncodingHeader(string headerName, string headerValue)
        {
            return InvalidOperation(Strings.BatchStream_MissingOrInvalidContentEncodingHeader(headerName, headerValue));
        }

        internal static InvalidOperationException BatchStreamInvalidNumberOfHeadersAtChangeSetStart(string header1, string header2)
        {
            return InvalidOperation(Strings.BatchStream_InvalidNumberOfHeadersAtChangeSetStart(header1, header2));
        }

        internal static InvalidOperationException BatchStreamMissingContentTypeHeader(string headerName)
        {
            return InvalidOperation(Strings.BatchStream_MissingContentTypeHeader(headerName));
        }

        internal static InvalidOperationException BatchStreamInvalidContentTypeSpecified(string headerName, string headerValue, string mime1, string mime2)
        {
            return InvalidOperation(Strings.BatchStream_InvalidContentTypeSpecified(headerName, headerValue, mime1, mime2));
        }

        internal static InvalidOperationException InternalError(InternalError value)
        {
            return InvalidOperation(Strings.Context_InternalError((int)value));
        }

        internal static void ThrowInternalError(InternalError value)
        {
            throw InternalError(value);
        }

        private static T Trace<T>(T exception) where T : Exception
        {
            return exception;
        }
    }

    internal enum InternalError
    {
        UnexpectedXmlNodeTypeWhenReading = 1,
        UnexpectedXmlNodeTypeWhenSkipping = 2,
        UnexpectedEndWhenSkipping = 3,
        UnexpectedReadState = 4,
        UnexpectedRequestBufferSizeTooSmall = 5,
        UnvalidatedEntityState = 6,
        NullResponseStream = 7,
        EntityNotDeleted = 8,
        EntityNotAddedState = 9,
        LinkNotAddedState = 10,
        EntryNotModified = 11,
        LinkBadState = 12,
        UnexpectedBeginChangeSet = 13,
        UnexpectedBatchState = 14,
        ChangeResponseMissingContentID = 15,
        ChangeResponseUnknownContentID = 16,
        TooManyBatchResponse = 17,

        InvalidEndGetRequestStream = 20,
        InvalidEndGetRequestCompleted = 21,
        InvalidEndGetRequestStreamRequest = 22,
        InvalidEndGetRequestStreamStream = 23,
        InvalidEndGetRequestStreamContent = 24,
        InvalidEndGetRequestStreamContentLength = 25,

        InvalidEndWrite = 30,
        InvalidEndWriteCompleted = 31,
        InvalidEndWriteRequest = 32,
        InvalidEndWriteStream = 33,

        InvalidEndGetResponse = 40,
        InvalidEndGetResponseCompleted = 41,
        InvalidEndGetResponseRequest = 42,
        InvalidEndGetResponseResponse = 43,
        InvalidAsyncResponseStreamCopy = 44,
        InvalidAsyncResponseStreamCopyBuffer = 45,

        InvalidEndRead = 50,
        InvalidEndReadCompleted = 51,
        InvalidEndReadStream = 52,
        InvalidEndReadCopy = 53,
        InvalidEndReadBuffer = 54,

        InvalidSaveNextChange = 60,
        InvalidBeginNextChange = 61,
        SaveNextChangeIncomplete = 62,

        InvalidGetRequestStream = 70,
        InvalidGetResponse = 71,
    }
}
