// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class CompositionErrorTests
    {
        [TestMethod]
        public void Constructor1_NullAsMessageArgument_ShouldSetMessagePropertyToEmptyString()
        {
            var error = new CompositionError((string)null);

            Assert.AreEqual("", error.Description);
        }

        [TestMethod]
        public void Constructor2_NullAsMessageArgument_ShouldSetMessagePropertyToEmptyString()
        {
            var error = new CompositionError((string)null, ElementFactory.Create());

            Assert.AreEqual("", error.Description);
        }

        [TestMethod]
        public void Constructor3_NullAsMessageArgument_ShouldSetMessagePropertyToEmptyString()
        {
            var error = new CompositionError((string)null, new Exception());

            Assert.AreEqual("", error.Description);
        }

        [TestMethod]
        public void Constructor4_NullAsMessageArgument_ShouldSetMessagePropertyToEmptyString()
        {
            var error = new CompositionError((string)null, ElementFactory.Create(), new Exception());

            Assert.AreEqual("", error.Description);
        }

        [TestMethod]
        public void Constructor5_NullAsMessageArgument_ShouldSetMessagePropertyToEmptyString()
        {
            var error = new CompositionError(CompositionErrorId.Unknown, (string)null, ElementFactory.Create(), new Exception());

            Assert.AreEqual("", error.Description);
        }

        [TestMethod]
        public void Constructor1_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(e);

                Assert.AreEqual(e, exception.Description);
            }
        }

        [TestMethod]
        public void Constructor2_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(e, ElementFactory.Create());

                Assert.AreEqual(e, exception.Description);
            }
        }

        [TestMethod]
        public void Constructor3_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(e, new Exception());

                Assert.AreEqual(e, exception.Description);
            }
        }

        [TestMethod]
        public void Constructor4_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(e, ElementFactory.Create(), new Exception());

                Assert.AreEqual(e, exception.Description);
            }
        }

        [TestMethod]
        public void Constructor5_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(CompositionErrorId.Unknown, e, ElementFactory.Create(), new Exception());

                Assert.AreEqual(e, exception.Description);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetExceptionPropertyToNull()
        {
            var error = new CompositionError("Description");

            Assert.IsNull(error.Exception);
        }

        [TestMethod]
        public void Constructor2_ShouldSetExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", ElementFactory.Create());

            Assert.IsNull(error.Exception);
        }

        [TestMethod]
        public void Constructor3_NullAsExceptionArgument_ShouldSetExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", (Exception)null);

            Assert.IsNull(error.Exception);
        }

        [TestMethod]
        public void Constructor4_NullAsExceptionArgument_ShouldSetExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", ElementFactory.Create(), (Exception)null);

            Assert.IsNull(error.Exception);
        }

        [TestMethod]
        public void Constructor5_NullAsExceptionArgument_ShouldSetExceptionPropertyToNull()
        {
            var error = new CompositionError(CompositionErrorId.Unknown, "Description", ElementFactory.Create(), (Exception)null);

            Assert.IsNull(error.Exception);
        }

        [TestMethod]
        public void Constructor3_ValueAsExceptionArgument_ShouldSetExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var error = new CompositionError("Description", e);

                Assert.AreSame(e, error.Exception);
            }
        }

        [TestMethod]
        public void Constructor4_ValueAsExceptionArgument_ShouldSetExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var error = new CompositionError("Description", ElementFactory.Create(), e);

                Assert.AreSame(e, error.Exception);
            }
        }

        [TestMethod]
        public void Constructor5_ValueAsExceptionArgument_ShouldSetExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var error = new CompositionError(CompositionErrorId.Unknown, "Description", ElementFactory.Create(), e);

                Assert.AreSame(e, error.Exception);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetInnerExceptionPropertyToNull()
        {
            var error = new CompositionError("Description");

            Assert.IsNull(((ICompositionError)error).InnerException);
        }

        [TestMethod]
        public void Constructor2_ShouldSetInnerExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", ElementFactory.Create());

            Assert.IsNull(((ICompositionError)error).InnerException);
        }

        [TestMethod]
        public void Constructor3_NullAsExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", (Exception)null);

            Assert.IsNull(((ICompositionError)error).InnerException);
        }

        [TestMethod]
        public void Constructor4_NullAsExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var error = new CompositionError("Description", ElementFactory.Create(), (Exception)null);

            Assert.IsNull(((ICompositionError)error).InnerException);
        }

        [TestMethod]
        public void Constructor5_NullAsExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var error = new CompositionError(CompositionErrorId.Unknown, "Description", ElementFactory.Create(), (Exception)null);

            Assert.IsNull(((ICompositionError)error).InnerException);
        }

        [TestMethod]
        public void Constructor3_ValueAsExceptionArgument_ShouldSetInnerExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var error = new CompositionError("Description", e);

                Assert.AreSame(e, ((ICompositionError)error).InnerException);
            }
        }

        [TestMethod]
        public void Constructor4_ValueAsExceptionArgument_ShouldSetInnerExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var error = new CompositionError("Description", ElementFactory.Create(), e);

                Assert.AreSame(e, ((ICompositionError)error).InnerException);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetICompositionErrorIdPropertyToCompositionErrorIdUnknown()
        {
            var error = new CompositionError("Description");

            Assert.AreEqual(CompositionErrorId.Unknown, ((ICompositionError)error).Id);
        }

        [TestMethod]
        public void Constructor2_ShouldSetICompositionErrorIdPropertyToCompositionErrorIdUnknown()
        {
            var error = new CompositionError("Description", ElementFactory.Create());

            Assert.AreEqual(CompositionErrorId.Unknown, ((ICompositionError)error).Id);
        }

        [TestMethod]
        public void Constructor3_ShouldSetICompositionErrorIdPropertyToCompositionErrorIdUnknown()
        {
            var error = new CompositionError("Description", new Exception());

            Assert.AreEqual(CompositionErrorId.Unknown, ((ICompositionError)error).Id);
        }

        [TestMethod]
        public void Constructor4_ShouldSetICompositionErrorIdPropertyToCompositionErrorIdUnknown()
        {
            var error = new CompositionError("Description", ElementFactory.Create(), new Exception());

            Assert.AreEqual(CompositionErrorId.Unknown, ((ICompositionError)error).Id);
        }

        [TestMethod]
        public void Constructor5_ValueAsIdArgument_ShouldSetICompositionErrorIdProperty()
        {
            var expectations = Expectations.GetEnumValues<CompositionErrorId>();

            foreach (var e in expectations)
            {
                var error = new CompositionError(e, "Description", ElementFactory.Create(), new Exception());

                Assert.AreEqual(e, ((ICompositionError)error).Id);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetElementPropertyToNull()
        {
            var exception = new CompositionError("Description");

            Assert.IsNull(exception.Element);
        }

        [TestMethod]
        public void Constructor2_NullAsElementArgument_ShouldSetElementPropertyToNull()
        {
            var exception = new CompositionError("Description", (ICompositionElement)null);

            Assert.IsNull(exception.Element);
        }

        [TestMethod]
        public void Constructor3_ShouldSetElementPropertyToNull()
        {
            var exception = new CompositionError("Description", new Exception());

            Assert.IsNull(exception.Element);
        }

        [TestMethod]
        public void Constructor4_NullAsElementArgument_ShouldSetElementPropertyToNull()
        {
            var exception = new CompositionError("Description", (ICompositionElement)null, new Exception());

            Assert.IsNull(exception.Element);
        }

        [TestMethod]
        public void Constructor5_NullAsElementArgument_ShouldSetElementPropertyToNull()
        {
            var exception = new CompositionError(CompositionErrorId.Unknown, "Description", (ICompositionElement)null, new Exception());

            Assert.IsNull(exception.Element);
        }

        [TestMethod]
        public void Constructor2_ValueAsElementArgument_ShouldSetElementProperty()
        {
            var expectations = Expectations.GetCompositionElements();

            foreach (var e in expectations)
            {
                var exception = new CompositionError("Description", (ICompositionElement)e);

                Assert.AreSame(e, exception.Element);
            }
        }

        [TestMethod]
        public void Constructor4_ValueAsElementArgument_ShouldSetElementProperty()
        {
            var expectations = Expectations.GetCompositionElements();

            foreach (var e in expectations)
            {
                var exception = new CompositionError("Description", (ICompositionElement)e, new Exception());

                Assert.AreSame(e, exception.Element);
            }
        }

        [TestMethod]
        public void Constructor5_ValueAsElementArgument_ShouldSetElementProperty()
        {
            var expectations = Expectations.GetCompositionElements();

            foreach (var e in expectations)
            {
                var exception = new CompositionError(CompositionErrorId.Unknown, "Description", (ICompositionElement)e, new Exception());

                Assert.AreSame(e, exception.Element);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var error = CreateCompositionError(e);

                Assert.AreEqual(error.Description, error.ToString());
            }
        }        

#if !SILVERLIGHT

        [TestMethod]
        public void ICompositionErrorId_CanBeSerialized()
        {
            var expectations = Expectations.GetEnumValues<CompositionErrorId>();

            foreach (var e in expectations)
            {
                var error = (ICompositionError)CreateCompositionError(e);

                var result = SerializationTestServices.RoundTrip(error);

                Assert.AreEqual(error.Id, result.Id);
            }
        }

        [TestMethod]
        public void Exception_CanBeSerialized()
        {
            var expectations = Expectations.GetInnerExceptionsWithNull();

            foreach (var e in expectations)
            {
                var error = CreateCompositionError(e);

                var result = SerializationTestServices.RoundTrip(error);

                ExtendedAssert.IsInstanceOfSameType(error.Exception, result.Exception);
            }
        }

        [TestMethod]
        public void Message_CanBeSerialized()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var error = CreateCompositionError(e);

                var result = SerializationTestServices.RoundTrip(error);

                Assert.AreEqual(error.Description, result.Description);
            }
        }

#endif

        private static CompositionError CreateCompositionError()
        {
            return CreateCompositionError(CompositionErrorId.Unknown, (string)null, (ICompositionElement)null, (Exception)null);
        }

        private static CompositionError CreateCompositionError(string message)
        {
            return CreateCompositionError(CompositionErrorId.Unknown, message, (ICompositionElement)null, (Exception)null);
        }

        private static CompositionError CreateCompositionError(CompositionErrorId id)
        {
            return CreateCompositionError(id, (string)null, (ICompositionElement)null, (Exception)null);
        }

        private static CompositionError CreateCompositionError(Exception exception)
        {
            return CreateCompositionError(CompositionErrorId.Unknown, (string)null, (ICompositionElement)null, exception);
        }

        private static CompositionError CreateCompositionError(ICompositionElement element)
        {
            return CreateCompositionError(CompositionErrorId.Unknown, (string)null, element, (Exception)null);
        }

        private static CompositionError CreateCompositionError(CompositionErrorId id, string message, ICompositionElement element, Exception exception)
        {
            return new CompositionError(id, message, element, exception);
        }

        private static CompositionError CreateCompositionError(params string[] messages)
        {
            CompositionError error = null;
            foreach (string message in messages.Reverse())
            {
                CompositionException exception = null;
                if (error != null)
                {
                    exception = CreateCompositionException(error);
                }

                error = ErrorFactory.Create(message, exception);
            }

            return error;
        }

        private static CompositionError CreateCompositionErrorWithException(params string[] messages)
        {
            Exception innerException = null;
            foreach (string message in messages.Skip(1).Reverse())
            {
                innerException = new Exception(message, innerException);
            }

            return new CompositionError(messages[0], innerException);
        }

        private static CompositionError CreateCompostionErrorWithCompositionException(string message1, string message2)
        {
            var exception = CreateCompositionException(new Exception(message2));

            return new CompositionError(message1, exception);
        }

        private static CompositionException CreateCompositionException(CompositionError error)
        {
            return new CompositionException(error);
        }

        private static CompositionException CreateCompositionException(Exception innerException)
        {
            return new CompositionException("Description", innerException, null);
        }
    }
}