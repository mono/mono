using System;
using System.Windows.Markup;
using System.Collections;
using System.Windows;

namespace Test.Properties
{
    public class NameScopeArrayList : ArrayList, INameScope
    {
        NameScope ns = new NameScope();

        void INameScope.RegisterName(string s, object o)
        {
            ns.RegisterName(s, o);
        }

        object INameScope.FindName(string s)
        {
            return ns.FindName(s);
        }

        void INameScope.UnregisterName(string s)
        {
            ns.UnregisterName(s);
        }
    }

    public class NameScopeArrayListHolder : INameScope
    {
        NameScope ns = new NameScope();

        NameScopeArrayList list = new NameScopeArrayList();

        public ArrayList ArrayList { get { return list; } }

        public NameScopeArrayList NameScopeArrayList { get { return list; } }

        void INameScope.RegisterName(string s, object o)
        {
            ns.RegisterName(s, o);
        }

        object INameScope.FindName(string s)
        {
            return ns.FindName(s);
        }

        void INameScope.UnregisterName(string s)
        {
            ns.UnregisterName(s);
        }
    }

    [ContentProperty("Recurse")]
    public class ProtectedRecursive
    {
        protected ProtectedRecursive Recurse { get; set; }
    }

    [ContentProperty("Recurse")]
    public class InternalRecursive
    {
        internal InternalRecursive Recurse { get; set; }
    }
}