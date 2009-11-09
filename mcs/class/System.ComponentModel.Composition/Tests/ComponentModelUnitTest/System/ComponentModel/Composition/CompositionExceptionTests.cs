// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.UnitTesting;
using System.ComponentModel.Composition.Factories;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Primitives;
using System.UnitTesting;
using Microsoft.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class CompositionExceptionTests
    {
        [TestMethod]
        public void Constructor1_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionException();

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [TestMethod]
        public void Constructor2_NullAsMessageArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionException((string)null);

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [TestMethod]
        public void Constructor3_EmptyEnumerableAsErrorsArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionException(Enumerable.Empty<CompositionError>());

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [TestMethod]
        public void Constructor4_NullAsMessageArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionException((string)null, new Exception());

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [TestMethod]
        public void Constructor5_NullAsMessageArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionException((string)null, new Exception(), Enumerable.Empty<CompositionError>());

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [TestMethod]
        public void Constructor2_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionException(e);

                Assert.AreEqual(e, exception.Message);
            }
        }

        [TestMethod]
        public void Constructor4_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionException(e, new Exception());

                Assert.AreEqual(e, exception.Message);
            }
        }

        [TestMethod]
        public void Constructor5_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionException(e, new Exception(), Enumerable.Empty<CompositionError>());

                Assert.AreEqual(e, exception.Message);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionException();

            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor2_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionException("Message");

            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor3_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionException(Enumerable.Empty<CompositionError>());

            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor4_NullAsInnerExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionException("Message", (Exception)null);

            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor5_NullAsInnerExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionException("Message", (Exception)null, Enumerable.Empty<CompositionError>());

            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor4_ValueAsInnerExceptionArgument_ShouldSetInnerExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var exception = new CompositionException("Message", e);

                Assert.AreSame(e, exception.InnerException);
            }
        }

        [TestMethod]
        public void Constructor5_ValueAsInnerExceptionArgument_ShouldSetInnerExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var exception = new CompositionException("Message", e, Enumerable.Empty<CompositionError>());

                Assert.AreSame(e, exception.InnerException);
            }
        }

        [TestMethod]
        public void Constructor2_ArrayWithNullAsErrorsArgument_ShouldThrowArgument()
        {
            var errors = new CompositionError[] { null };

            ExceptionAssert.ThrowsArgument<ArgumentException>("errors", () =>
            {
                new CompositionException(errors);
            });
        }

        [TestMethod]
        public void Constructor5_ArrayWithNullAsErrorsArgument_ShouldThrowArgument()
        {
            var errors = new CompositionError[] { null };

            ExceptionAssert.ThrowsArgument<ArgumentException>("errors", () =>
            {
                new CompositionException("Message", new Exception(), errors);
            });
        }

        [TestMethod]
        public void Constructor1_ShouldSetErrorsPropertyToEmpty()
        {
            var exception = new CompositionException();

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor2_NullAsErrorsArgument_ShouldSetErrorsPropertyToEmptyEnumerable()
        {
            var exception = new CompositionException((IEnumerable<CompositionError>)null);

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor2_EmptyEnumerableAsErrorsArgument_ShouldSetErrorsPropertyToEmptyEnumerable()
        {
            var exception = new CompositionException(Enumerable.Empty<CompositionError>());

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor2_ValueAsErrorsArgument_ShouldSetErrorsProperty()
        {
            var expectations = Expectations.GetCompositionErrors();

            foreach (var e in expectations)
            {
                var exception = new CompositionException(e);

                EnumerableAssert.AreSequenceSame(e, exception.Errors);
            }
        }

        [TestMethod]
        public void Constructor2_ArrayAsAsErrorsArgument_ShouldNotAllowModificationAfterConstruction()
        {
            var error = CreateCompositionError();
            var errors = new CompositionError[] { error };

            var exception = new CompositionException(errors);

            errors[0] = null;

            EnumerableAssert.AreEqual(exception.Errors, error);
        }

        [TestMethod]
        public void Constructor3_ShouldSetErrorsPropertyToEmpty()
        {
            var exception = new CompositionException();

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor4_ShouldSetErrorsPropertyToEmptyEnumerable()
        {
            var exception = new CompositionException("Message", new Exception());

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor5_NullAsErrorsArgument_ShouldSetErrorsPropertyToEmptyEnumerable()
        {
            var exception = new CompositionException("Message", new Exception(), (IEnumerable<CompositionError>)null);

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor5_EmptyEnumerableAsErrorsArgument_ShouldSetErrorsPropertyToEmptyEnumerable()
        {
            var exception = new CompositionException("Message", new Exception(), Enumerable.Empty<CompositionError>());

            EnumerableAssert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor5_ValueAsErrorsArgument_ShouldSetErrorsProperty()
        {
            var expectations = Expectations.GetCompositionErrors();

            foreach (var e in expectations)
            {
                var exception = new CompositionException("Message", new Exception(), e);

                EnumerableAssert.AreSequenceSame(e, exception.Errors);
            }
        }

        [TestMethod]
        public void Constructor5_ArrayAsAsErrorsArgument_ShouldNotAllowModificationAfterConstruction()
        {
            var error = CreateCompositionError();
            var errors = new CompositionError[] { error };

            var exception = new CompositionException("Message", new Exception(), errors);

            errors[0] = null;

            EnumerableAssert.AreEqual(exception.Errors, error);
        }

        [TestMethod]
        public void Message_ShouldIncludeElementGraph()
        {
            var expectations = new ExpectationCollection<CompositionError, string>();
            CompositionError error = null;

            error = CreateCompositionErrorWithElementChain(1);
            expectations.Add(error, GetElementGraphString(error));

            error = CreateCompositionErrorWithElementChain(2);
            expectations.Add(error, GetElementGraphString(error));

            error = CreateCompositionErrorWithElementChain(3);
            expectations.Add(error, GetElementGraphString(error));

            error = CreateCompositionErrorWithElementChain(10);
            expectations.Add(error, GetElementGraphString(error));


            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(new CompositionError[] { e.Input });

                string result = exception.ToString();
                string expected = FixMessage(e.Output);
                StringAssert.Contains(result, expected);}
        }

        [TestMethod]
        public void Message_ShouldIncludeErrors()
        { 
            var expectations = new ExpectationCollection<IEnumerable<CompositionError>, string>();
            expectations.Add(ErrorFactory.CreateFromDsl("Error"),                           "1<Separator> Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error|Error"),                     "1<Separator> Error|2<Separator> Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error|Error|Error"),               "1<Separator> Error|2<Separator> Error|3<Separator> Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error)"),                    "1<Separator> Error|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error|Error)"),              "1<Separator> Error|<Prefix>Error|2<Separator> Error|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error|Error|Error)"),        "1<Separator> Error|<Prefix>Error|2<Separator> Error|<Prefix>Error|3<Separator> Error|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error(Exception))"),         "1<Separator> Exception|<Prefix>Error|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error|Exception)"),          "1<Separator> Error|<Prefix>Error|2<Separator> Exception|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Exception)"),                "1<Separator> Exception|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Exception(Exception))"),     "1<Separator> Exception|<Prefix>Exception|<Prefix>Error");
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error(Exception)|Error)"),   "1<Separator> Exception|<Prefix>Error|<Prefix>Error|2<Separator> Error|<Prefix>Error");
            
            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(e.Input);

                AssertMessage(exception, e.Output.Split('|'));
            }
        }

        [TestMethod]
        public void Messsage_ShouldIncludeCountOfRootCauses()
        {
            var expectations = new ExpectationCollection<IEnumerable<CompositionError>, int>();
            expectations.Add(ErrorFactory.CreateFromDsl("Error"),                            1);
            expectations.Add(ErrorFactory.CreateFromDsl("Error|Error"),                      2);
            expectations.Add(ErrorFactory.CreateFromDsl("Error|Error|Error"),                3);
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error)"),                     1);
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error)|Error(Error)"),        2);
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error|Error)"),               2);
            expectations.Add(ErrorFactory.CreateFromDsl("Error(Error|Error|Exception)"),     3);
            
            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(e.Input);

                AssertMessage(exception, e.Output, CultureInfo.CurrentCulture);
            }          
        }

        [TestMethod]
        public void Message_ShouldFormatCountOfRootCausesUsingTheCurrentCulture()
        {
            var cultures = Expectations.GetCulturesForFormatting();

            foreach (var culture in cultures)
            {
                using (new CurrentCultureContext(culture))
                {
                    var errors = CreateCompositionErrors(1000);
                    var exception = CreateCompositionException(errors);
                    AssertMessage(exception, 1000, culture);

                    errors = CreateCompositionErrors(1);
                    exception = CreateCompositionException(errors);
                    AssertMessage(exception, 1, culture);
                }
            }
        }

#if !SILVERLIGHT

        [TestMethod]
        public void Constructor6_NullAsInfoArgument_ShouldThrowArgumentNull()
        {
            var context = new StreamingContext();

            ExceptionAssert.ThrowsArgument<ArgumentNullException>("info", () =>
            {
                SerializationTestServices.Create<CompositionException>((SerializationInfo)null, context);
            });
        }

        [TestMethod]
        public void Constructor6_SerializationInfoWithMissingIdEntryAsInfoArgument_ShouldThrowSerialization()
        {
            var info = SerializationTestServices.CreateSerializationInfoRemovingMember<CompositionException>("Errors");
            var context = new StreamingContext();

            ExceptionAssert.ThrowsSerialization("Errors", () =>
            {
                SerializationTestServices.Create<CompositionException>(info, context);
            });
        }

        [TestMethod]
        public void Constructor6_SerializationInfoWithWrongTypeForIdEntryAsInfoArgument_ShouldThrowInvalidCast()
        {
            var info = SerializationTestServices.CreateSerializationInfoReplacingMember<CompositionException>("Errors", 10);
            var context = new StreamingContext();

            ExceptionAssert.Throws<InvalidCastException>(() =>
            {
                SerializationTestServices.Create<CompositionException>(info, context);
            });
        }

        [TestMethod]
        public void InnerException_CanBeSerialized()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                ExtendedAssert.IsInstanceOfSameType(exception.InnerException, result.InnerException);
            }
        }

        [TestMethod]
        public void Message_CanBeSerialized()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                Assert.AreEqual(exception.Message, result.Message);
            }
        }

        [TestMethod]
        public void Errors_CanBeSerialized()
        {
            var expectations = Expectations.GetCompositionErrors();

            foreach (var e in expectations)
            {
                var exception = CreateCompositionException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                EnumerableAssert.AreSequenceEqual(exception.Errors, result.Errors, (index, expected, actual) =>
                {
                    CompositionAssert.AreEqual(expected, actual);
                });
            }
        }

        [TestMethod]
        public void GetObjectData_NullAsInfoArgument_ShouldThrowArgumentNull()
        {
            var exception = (ISerializable)CreateCompositionException();
            var context = new StreamingContext();

            ExceptionAssert.ThrowsArgument<ArgumentNullException>("info", () =>
            {
                exception.GetObjectData((SerializationInfo)null, context);
            });
        }

#endif
        private string GetElementGraphString(CompositionError error)
        {
            StringBuilder writer = new StringBuilder();
            ICompositionElement element = error.Element;
            writer.AppendFormat(CultureInfo.CurrentCulture, Strings.CompositionException_ElementPrefix, element.DisplayName);

            while ((element = element.Origin) != null)
            {
                writer.AppendFormat(CultureInfo.CurrentCulture, Strings.CompositionException_OriginFormat, Strings.CompositionException_OriginSeparator, element.DisplayName);
            }

            return writer.ToString();
        }

        private void AssertMessage(CompositionException exception, int rootCauseCount, CultureInfo culture)
        {
            using (StringReader reader = new StringReader(exception.Message))
            {
                string line = reader.ReadLine();

                if (rootCauseCount == 1)
                {
                    Assert.IsTrue(line.Contains(Strings.CompositionException_SingleErrorWithSinglePath));
                }
                else
                {
                    Assert.IsTrue(
                        line.Contains(string.Format(CultureInfo.CurrentCulture, Strings.CompositionException_SingleErrorWithMultiplePaths, rootCauseCount)) ||
                        line.Contains(string.Format(CultureInfo.CurrentCulture, Strings.CompositionException_MultipleErrorsWithMultiplePaths, rootCauseCount))
                        );
                }
            }
        }

        private void AssertMessage(CompositionException exception, string[] expected)
        {
            using (StringReader reader = new StringReader(exception.Message))
            {
                // Skip header
                reader.ReadLine();

                foreach (string expect in expected)
                {
                    // Skip blank line
                    reader.ReadLine();
                    Assert.AreEqual(FixMessage(expect), reader.ReadLine());
                }
            }
        }

        private string FixMessage(string expect)
        {
            string fixedPrefix = expect.Replace("<Prefix>", Strings.CompositionException_ErrorPrefix + " ");
            string fixedSeparator = fixedPrefix.Replace("<Separator>", Strings.CompositionException_PathsCountSeparator);
            return fixedSeparator.Replace("<OriginSeparator>", Strings.CompositionException_OriginSeparator);
        }

        private static CompositionError CreateCompositionError()
        {
            return CreateCompositionError("Description");
        }

        private static CompositionError CreateCompositionError(string message)
        {
            return new CompositionError(message);
        }

        private static CompositionError CreateCompositionErrorWithElementChain(int count)
        {
            return new CompositionError("Description", ElementFactory.CreateChain(count));
        }

        private static CompositionError[] CreateCompositionErrors(int count)
        {
            CompositionError[] errors = new CompositionError[count];

            for (int i = 0; i < count; i++)
            {
                errors[i] = CreateCompositionError("Description" + (i + 1));
            }

            return errors;
        }

        private static CompositionException CreateCompositionException()
        {
            return CreateCompositionException((string)null, (Exception)null, (IEnumerable<CompositionError>)null);
        }

        private static CompositionException CreateCompositionException(string message)
        {
            return CreateCompositionException(message, (Exception)null, (IEnumerable<CompositionError>)null);
        }

        private static CompositionException CreateCompositionException(IEnumerable<CompositionError> errors)
        {
            return CreateCompositionException((string)null, (Exception)null, errors);
        }

        private static CompositionException CreateCompositionException(Exception innerException)
        {
            return CreateCompositionException((string)null, innerException, (IEnumerable<CompositionError>)null);
        }

        private static CompositionException CreateCompositionException(string message, Exception innerException, IEnumerable<CompositionError> errors)
        {
            return new CompositionException(message, innerException, errors);
        }
    }
}