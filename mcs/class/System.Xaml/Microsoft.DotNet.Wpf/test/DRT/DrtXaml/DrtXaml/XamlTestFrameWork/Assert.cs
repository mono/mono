using System;
using System.Collections.Generic;
using System.Collections;
using Xunit;
using Xunit.Sdk;

namespace DrtXaml.XamlTestFramework
{
    public class EqualException2 : EqualException
    {
        public EqualException2(object expected, object actual) : base(expected, actual)
        {
        }

        public EqualException2(string expected, string actual, int expectedIndex, int actualIndex) : base(expected, actual, expectedIndex, actualIndex)
        {
        }

        public EqualException2(object expected, object actual, string message): this(expected, actual)
        {
            UserMessage = message;
        }
    }

    public class NotEqualException2 : NotEqualException
    {
        public NotEqualException2(NotEqualException inner, string message): base(inner.Expected, inner.Actual)
        {
            UserMessage = message;
        }
    }

    public class NotNullException2 : NotNullException
    {
        public NotNullException2()
        {
        }

        public NotNullException2(string message)
        {
            UserMessage = message;
        }
    }

    public class NullException2 : NullException
    {
        public NullException2(object actual) : base(actual)
        {
        }

        public NullException2(object actual, string message): this(actual)
        {
            UserMessage = message;
        }
    }

    public class IsAssignableFromException2 : IsAssignableFromException
    {
        public IsAssignableFromException2(Type expected, object actual) : base(expected, actual)
        {
        }

        public IsAssignableFromException2(Type expected, object actual, string message) : this(expected, actual)
        {
            UserMessage = message;
        }
    }


    public static class Assert
    {

        public static void AreEqual<T>(T expected, T actual)
        {
            AreEqual<T>(expected, actual, "Are not equal");
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            try
            {
                Xunit.Assert.Equal<T>(expected, actual);
            }
            catch (EqualException)
            {
                throw new EqualException2(expected, actual, message);
            }
        }

        public static void AreNotEqual<T>(T expected, T actual)
        {
            AreNotEqual<T>(expected, actual, "Should not be equal");
        }

        public static void AreNotEqual<T>(T expected, T actual, string message)
        {
            try
            {
                Xunit.Assert.NotEqual<T>(expected, actual);
            }
            catch (NotEqualException e)
            {
                throw new NotEqualException2(e, message);
            }
        }



        public static void AreEqualOrdered<T>(IList<T> actual, params T[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        public static void AreEqualUnordered<T>(ICollection<T> actual, params T[] expected)
        {
            AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                IsTrue(actual.Contains(expected[i]));
            }
        }
        
        

        public static void AreSame(object expected, object actual)
        {
            AreSame(expected, actual, "Objects are not the same");
        }

        public static void AreSame(object expected, object actual, string message)
        {
            Assert.AreEqual<object>(expected, actual, message);
        }

        public static void AreNotSame(object expected, object actual)
        {
            AreNotSame(expected, actual, "Objects are the same");
        }

        public static void AreNotSame(object expected, object actual, string message)
        {
            Assert.AreNotEqual<object>(expected, actual, message);
        }

        public static void IsEmpty(ICollection collection)
        {
            IsEmpty(collection, "Collection is not empty");
        }
        
        public static void IsEmpty(ICollection collection, string message)
        {
            if (collection != null)
            {
                AreEqual(0, collection.Count, message);
            }
        }
        
        public static void IsFalse(bool condition)
        {
            IsFalse(condition, "Is not False");
        }

        public static void IsFalse(bool condition, string message)
        {
            Xunit.Assert.False(condition, message);
        }

        public static void IsNotNull(object o)
        {
            IsNotNull(o, "Object is null");
        }

        public static void IsNotNull(object o, string message)
        {
            try
            {
                Xunit.Assert.NotNull(o);
            }
            catch (NotNullException)
            {
                throw new NotNullException2(message);
            }
        }

        public static void IsNull(object o)
        {
            IsNull(o, "Reference is not null");
        }

        public static void IsNull(object o, string message)
        {
            try
            {
                Xunit.Assert.Null(o);
            }
            catch (NullException)
            {
                throw new NullException2(o, message);
            }
        }

        public static void IsTrue(bool condition)
        {
            IsTrue(condition, "is not True");
        }

        public static void IsTrue(bool condition, string message)
        {
            Xunit.Assert.True(condition, message);
        }

        public static void IsInstanceOfType(Type expected, object actual)
        {
            IsInstanceOfType(expected, actual, String.Format("Object is not an instance of type '{0}'", expected.ToString()));
        }

        public static void IsInstanceOfType(Type expected, object actual, string message)
        {
            try
            {
                Xunit.Assert.IsAssignableFrom(expected, actual);
            }
            catch(IsAssignableFromException)
            {
                throw new IsAssignableFromException2(expected, actual, message);
            }
        }

        public static void Fail(string message)
        {
            Xunit.Assert.True(false, message);
        }
    }
}
