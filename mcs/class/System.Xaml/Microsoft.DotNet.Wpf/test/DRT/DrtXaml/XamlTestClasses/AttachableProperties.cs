using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace Test.Elements
{
    public static class HasAttachableDp
    {
        public static readonly DependencyProperty MyDpProperty =
            DependencyProperty.RegisterAttached("MyProperty", typeof(object), typeof(HasAttachableDp), null);

        public static object GetMyDp(DependencyObject o)
        {
            return o.GetValue(MyDpProperty);
        }
        public static void SetMyDp(DependencyObject o, object value)
        {
            o.SetValue(MyDpProperty, value);
        }
    }

    public class APP  // AttachablePropertyProvider
    {
        static readonly AttachableMemberIdentifier fooPropertyName = new AttachableMemberIdentifier(typeof(APP), "Foo");
        static readonly AttachableMemberIdentifier barPropertyName = new AttachableMemberIdentifier(typeof(APP), "Bar");
        static readonly AttachableMemberIdentifier internalFooPropertyName = new AttachableMemberIdentifier(typeof(APP), "InternalFoo");
        static readonly AttachableMemberIdentifier internalBarPropertyName = new AttachableMemberIdentifier(typeof(APP), "InternalBar");
        static readonly AttachableMemberIdentifier internalListPropertyName = new AttachableMemberIdentifier(typeof(APP), "InternalList");

        public static string GetFoo(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, fooPropertyName, out value) ? value : null;
        }

        public static void SetFoo(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, fooPropertyName, value);
        }

        public static int GetBar(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, barPropertyName, out value) ? value : 0;
        }

        public static void SetBar(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, barPropertyName, value);
        }

        internal static string GetInternalFoo(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, internalFooPropertyName, out value) ? value : null;
        }

        internal static void SetInternalFoo(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, internalFooPropertyName, value);
        }

        internal static int GetInternalBar(object target)
        {
            int value;
            return AttachablePropertyServices.TryGetProperty(target, internalBarPropertyName, out value) ? value : 0;
        }

        internal static void SetInternalBar(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, internalBarPropertyName, value);
        }

        internal static List<string> GetInternalList(object target)
        {
            List<string> result;
            if (!AttachablePropertyServices.TryGetProperty(target, internalListPropertyName, out result))
            {
                lock (internalListPropertyName)
                {
                    if (!AttachablePropertyServices.TryGetProperty(target, internalListPropertyName, out result))
                    {
                        result = new List<string>();
                        AttachablePropertyServices.SetProperty(target, internalListPropertyName, result);
                    }
                }
            }
            return result;
        }

        // Hook so partial-trust test code can access internals
        public static object PropertyAccessor(object target, string propertyName)
        {
            object value = null;
            AttachablePropertyServices.TryGetProperty(target,
                new AttachableMemberIdentifier(typeof(APP), propertyName), out value);
            return value;
        }
    }

    public class InheritsAPP : APP
    {
    }

    public class AttachableAndNonAtttachable
    {
        public string Foo { get; set; }
        
        public static string GetFoo(object target)
        {
            return null;
        }

        public static void SetFoo(object target, string value)
        {
        }
    }

    public class AOP //attachable overloads provider
    {
        static readonly AttachableMemberIdentifier fooPropertyName = new AttachableMemberIdentifier(typeof(AOP), "Foo");

        protected internal static string GetFoo(string target)
        {
            throw new NotImplementedException();
        }

        public static string GetFoo(object target)
        {
            string value;
            return AttachablePropertyServices.TryGetProperty(target, fooPropertyName, out value) ? value : null;
        }

        public static void SetFoo(object target, int value)
        {
            AttachablePropertyServices.SetProperty(target, fooPropertyName, value);
        }

        internal static void SetFoo(object target, string value)
        {
            throw new NotImplementedException();
        }
    }

    public class ReadOnlyAPP
    {
        static AttachableMemberIdentifier listPropertyID = new AttachableMemberIdentifier(typeof(ReadOnlyAPP), "StringsList");
        public static IList<string> GetStringsList(object target)
        {
            IList<string> result;
            if (!AttachablePropertyServices.TryGetProperty(target, listPropertyID, out result))
            {
                result = new List<string>();
                AttachablePropertyServices.SetProperty(target, listPropertyID, result);
            }
            return result;
        }

        static AttachableMemberIdentifier dictPropertyID = new AttachableMemberIdentifier(typeof(ReadOnlyAPP), "StringsDict");
        public static IDictionary<string, string> GetStringsDict(object target)
        {
            IDictionary<string, string> result;
            if (!AttachablePropertyServices.TryGetProperty(target, dictPropertyID, out result))
            {
                result = new Dictionary<string, string>();
                AttachablePropertyServices.SetProperty(target, dictPropertyID, result);
            }
            return result;
        }
    }
}
