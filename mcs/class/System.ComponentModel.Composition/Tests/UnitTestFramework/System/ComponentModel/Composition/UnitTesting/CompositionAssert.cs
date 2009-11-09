// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.UnitTesting
{
    public static class CompositionAssert
    {
        internal static void AreEqual(CompositionResult expected, CompositionResult actual)
        {
            Assert.AreEqual(expected.Succeeded, actual.Succeeded);

            EnumerableAssert.AreSequenceEqual(expected.Errors, actual.Errors, (index, expectedError, actualError) =>
            {
                AreEqual(expectedError, actualError);
            });
        }

        internal static void AreEqual(CompositionError expected, CompositionError actual)
        {
            Assert.AreEqual(((ICompositionError)expected).Id, ((ICompositionError)actual).Id);
            Assert.AreEqual(expected.Description, actual.Description);
            ExtendedAssert.IsInstanceOfSameType(expected.Exception, actual.Exception);
        }

        public static void ThrowsPart(ErrorId id, Action action)
        {
            ThrowsPart(id, RetryMode.Retry, action);
        }

        public static void ThrowsPart(ErrorId id, RetryMode retry, Action action)
        {
            ThrowsPart(new CompositionErrorExpectation { Id = id }, retry, action);
        }

        public static void ThrowsPart(ErrorId id, ICompositionElement element, Action action)
        {
            ThrowsPart(id, element, RetryMode.Retry, action);
        }

        public static void ThrowsPart(ErrorId id, ICompositionElement element, RetryMode retry, Action action)
        {
            ThrowsPart(new CompositionErrorExpectation { Id = id, Element = element }, retry, action);
        }

        public static void ThrowsPart<TInner>(ErrorId id, Action action)
            where TInner : Exception
        {
            ThrowsPart<TInner>(id, RetryMode.Retry, action);
        }

        public static void ThrowsPart<TInner>(ErrorId id, RetryMode retry, Action action)
            where TInner : Exception
        {
            ThrowsPart(new CompositionErrorExpectation { Id = id, InnerExceptionType = typeof(TInner) }, retry, action);
        }

        private static void ThrowsPart(CompositionErrorExpectation expectation, RetryMode retry, Action action)
        {
            ExceptionAssert.Throws<ComposablePartException>(retry, action, (thrownException, retryCount) =>
            {
                AssertCore(retryCount, "ComposablePartException", thrownException, expectation);
            });
        }

        public static void ThrowsRootError(ErrorId rootId, RetryMode retry, Action action)
        {
            var exception = ExceptionAssert.Throws<CompositionException>(retry, action, (thrownException, retryCount) =>
            {
                ErrorId actualId = GetRootErrorId(thrownException);

                Assert.AreEqual(rootId, actualId, "Retry Count {0}: Expected '{1}' to be the root ErrorId, however, '{2}' is.", retryCount, rootId, actualId);
            });
        }

        public static void ThrowsError<TInner>(ErrorId id, RetryMode retry, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, InnerExceptionType = typeof(TInner) }, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id}, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, ErrorId innerId, Action action)
        {
            ThrowsError(id, innerId, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, ErrorId innerId, RetryMode retry, Action action)
        {
            ThrowsError(GetExpectation(id, innerId), retry, action);
        }

        public static void ThrowsError(ErrorId id, ErrorId innerId, ErrorId innerInnerId, Action action)
        {
            ThrowsError(id, innerId, innerInnerId, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, ErrorId innerId, ErrorId innerInnerId, RetryMode retry, Action action)
        {
            ThrowsError(GetExpectation(id, innerId, innerInnerId), retry, action);
        }

        public static void ThrowsError(ErrorId id, RetryMode retry, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, }, retry, action);
        }

        public static void ThrowsError(ErrorId id, ICompositionElement element, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, Element = element}, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, ICompositionElement element, RetryMode retry, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, Element = element }, retry, action);
        }

        public static void ThrowsError(ErrorId id, Exception exception, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, InnerException = exception }, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, Exception exception, RetryMode retry, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, InnerException = exception }, retry, action);
        }

        public static void ThrowsError(ErrorId id, ICompositionElement element, Exception exception, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, Element = element, InnerException = exception }, RetryMode.Retry, action);
        }

        public static void ThrowsError(ErrorId id, ICompositionElement element, Exception exception, RetryMode retry, Action action)
        {
            ThrowsError(new CompositionErrorExpectation { Id = id, Element = element, InnerException = exception }, retry, action);
        }

        private static void ThrowsError(CompositionErrorExpectation expectation, RetryMode retry, Action action)
        {
            ThrowsErrors(new CompositionErrorExpectation[] { expectation }, retry, action);
        }

        public static void ThrowsErrors(ErrorId id1, ErrorId id2, Action action)
        {
            ThrowsErrors(id1, id2, RetryMode.Retry, action);
        }

        public static void ThrowsErrors(ErrorId id1, ErrorId id2, RetryMode retry, Action action)
        {
            ThrowsErrors(new ErrorId[] { id1, id2 }, retry, action);
        }

        public static void ThrowsErrors(ErrorId[] ids, RetryMode retry, Action action)
        {
            CompositionErrorExpectation[] expectations = new CompositionErrorExpectation[ids.Length]; 
            for (int i = 0; i < expectations.Length; i++)
            {
                expectations[i] = new CompositionErrorExpectation { Id = ids[i] };
            }

            ThrowsErrors(expectations, retry, action);
        }

        private static void ThrowsErrors(CompositionErrorExpectation[] expectations, RetryMode retry, Action action)
        {
            ExceptionAssert.Throws<CompositionException>(retry, action, (thrownException, retryCount) =>
            {
                AssertCore(retryCount, "CompositionException", thrownException, expectations);
            });
        }

        public static void ThrowsChangeRejectedRootError(ErrorId rootId, RetryMode retry, Action action)
        {
            var exception = ExceptionAssert.Throws<ChangeRejectedException>(retry, action, (thrownException, retryCount) =>
            {
                ErrorId actualId = GetRootErrorId(thrownException);

                Assert.AreEqual(rootId, actualId, "Retry Count {0}: Expected '{1}' to be the root ErrorId, however, '{2}' is.", retryCount, rootId, actualId);
            });
        }

        public static void ThrowsChangeRejectedError(ErrorId id, Action action)
        {
            ThrowsChangeRejectedError(new CompositionErrorExpectation { Id = id }, RetryMode.Retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, RetryMode retry, Action action)
        {
            ThrowsChangeRejectedError(new CompositionErrorExpectation { Id = id, }, retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, ICompositionElement element, Action action)
        {
            ThrowsChangeRejectedError(new CompositionErrorExpectation { Id = id, Element = element }, RetryMode.Retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, ErrorId innerId, RetryMode retry, Action action)
        {
            ThrowsChangeRejectedError(GetExpectation(id, innerId), retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, ErrorId innerId, ErrorId innerInnerId, Action action)
        {
            ThrowsChangeRejectedError(id, innerId, innerInnerId, RetryMode.Retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, ErrorId innerId, ErrorId innerInnerId, RetryMode retry, Action action)
        {
            ThrowsChangeRejectedError(GetExpectation(id, innerId, innerInnerId), retry, action);
        }

        private static void ThrowsChangeRejectedError(CompositionErrorExpectation expectation, RetryMode retry, Action action)
        {
            ThrowsChangeRejectedErrors(new CompositionErrorExpectation[] { expectation }, retry, action);
        }

        public static void ThrowsChangeRejectedError(ErrorId id, ICompositionElement element, Exception exception, Action action)
        {
            ThrowsChangeRejectedError(new CompositionErrorExpectation { Id = id, Element = element, InnerException = exception }, RetryMode.Retry, action);
        }

        public static void ThrowsChangeRejectedErrors(ErrorId id1, ErrorId id2, RetryMode retry, Action action)
        {
            ThrowsChangeRejectedErrors(new ErrorId[] { id1, id2 }, retry, action);
        }

        public static void ThrowsChangeRejectedErrors(ErrorId[] ids, RetryMode retry, Action action)
        {
            CompositionErrorExpectation[] expectations = new CompositionErrorExpectation[ids.Length];
            for (int i = 0; i < expectations.Length; i++)
            {
                expectations[i] = new CompositionErrorExpectation { Id = ids[i] };
            }

            ThrowsChangeRejectedErrors(expectations, retry, action);
        }

        private static void ThrowsChangeRejectedErrors(CompositionErrorExpectation[] expectations, RetryMode retry, Action action)
        {
            ExceptionAssert.Throws<ChangeRejectedException>(retry, action, (thrownException, retryCount) =>
            {
                AssertCore(retryCount, "CompositionException", thrownException, expectations);
            });
        }

        private static void AssertCore(int retryCount, string prefix, CompositionException exception, CompositionErrorExpectation[] expectations)
        {
            Assert.AreEqual(exception.Errors.Count, expectations.Length);

            for (int i = 0; i < exception.Errors.Count; i++)
            {
                AssertCore(retryCount, prefix + ".Errors[" + i + "]", exception.Errors[i], expectations[i]);
            }
        }

        private static void AssertCore(int retryCount, string prefix, ICompositionError error, CompositionErrorExpectation expectation)
        {
            if (expectation.IdSpecified)
            {
                AssertCore(retryCount, prefix, "Id", expectation.Id, (ErrorId)error.Id);
            }

            if (expectation.ElementSpecified)
            {
                AssertCore(retryCount, prefix, "Element", expectation.Element, error.Element);
            }

            if (expectation.InnerExceptionSpecified)
            {
                AssertCore(retryCount, prefix, "InnerException", expectation.InnerException, error.InnerException);
            }

            if (expectation.InnerExceptionTypeSpecified)
            {
                AssertCore(retryCount, prefix, "InnerException.GetType()", expectation.InnerExceptionType, error.InnerException == null ? null : error.InnerException.GetType());
            }

            if (expectation.InnerExpectationsSpecified)
            {
                ICompositionError innerError = error.InnerException as ICompositionError;
                if (innerError != null)
                {
                    Assert.AreEqual(1, expectation.InnerExpectations.Length);
                    AssertCore(retryCount, prefix + ".InnerException", innerError, expectation.InnerExpectations[0]);
                }
                else
                {
                    AssertCore(retryCount, prefix + ".InnerException", (CompositionException)error.InnerException, expectation.InnerExpectations);
                }
            }
        }
               
        private static void AssertCore<T>(int retryCount, string prefix, string propertyName, T expected, T actual)
        {
            Assert.AreEqual(expected, actual, "Retry Count {0}: Expected '{1}' to be {3}.{4}, however, '{2}' is.", retryCount, expected, actual, prefix, propertyName);
        }

        private static CompositionErrorExpectation GetExpectation(params ErrorId[] ids)
        {
            var parent = new CompositionErrorExpectation() { Id = ids[0] };
            var expectation = parent;

            for (int i = 1; i < ids.Length; i++)
            {
                expectation.InnerExpectations = new CompositionErrorExpectation[] { new CompositionErrorExpectation() { Id = ids[i] } };
                expectation = expectation.InnerExpectations[0];
            }

            return parent;
        }

        private static ErrorId GetRootErrorId(CompositionException exception)
        {
            Assert.IsTrue(exception.Errors.Count == 1);

            return GetRootErrorId(exception.Errors[0]);
        }

        private static ErrorId GetRootErrorId(ICompositionError error)
        {
            Exception exception = error.InnerException;

            var childError = exception as ICompositionError;
            if (childError != null)
            {
                return GetRootErrorId(childError);
            }

            CompositionException composition = exception as CompositionException;
            if (composition != null)
            {
                return GetRootErrorId(composition);
            }

            return (ErrorId)error.Id;
        }

        private class CompositionErrorExpectation
        {
            private ErrorId _id;
            private Exception _innerException;
            private Type _innerExceptionType;
            private ICompositionElement _element;
            private CompositionErrorExpectation[] _innerExpectations;

            public ErrorId Id
            {
                get { return _id; }
                set
                {
                    _id = value;
                    IdSpecified = true;
                }
            }

            public Exception InnerException
            {
                get { return _innerException; }
                set
                {
                    _innerException = value;
                    InnerExceptionSpecified = true;
                }
            }

            public Type InnerExceptionType
            {
                get { return _innerExceptionType; }
                set
                {
                    _innerExceptionType = value;
                    InnerExceptionTypeSpecified = true;
                }
            }

            public ICompositionElement Element
            {
                get { return _element; }
                set
                {
                    _element = value;
                    ElementSpecified = true;
                }
            }

            public CompositionErrorExpectation[] InnerExpectations
            {
                get { return _innerExpectations; }
                set
                {
                    _innerExpectations = value;
                    InnerExpectationsSpecified = true;
                }
            }

            public bool IdSpecified
            {
                get;
                private set;
            }

            public bool InnerExceptionSpecified
            {
                get;
                private set;
            }

            public bool InnerExceptionTypeSpecified
            {
                get;
                private set;
            }

            public bool ElementSpecified
            {
                get;
                private set;
            }

            public bool InnerExpectationsSpecified
            {
                get;
                private set;
            }
        }
    }
}
