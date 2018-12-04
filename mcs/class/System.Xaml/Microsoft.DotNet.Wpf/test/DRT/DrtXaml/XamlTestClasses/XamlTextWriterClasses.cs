using System;
using Test.Elements.AnotherNamespace;
using System.Windows.Markup;
using System.Collections.Generic;

[assembly: XmlnsDefinition("http://foo", "Test.Elements.OneMoreNamespace")]
[assembly: XmlnsDefinition("http://bar", "Test.Elements.OneMoreNamespace")]

namespace Test.Elements
{
    [ContentProperty("Content")]
    [RuntimeNameProperty("Integer")]
    public class BigContainer
    {
        public int Integer { get; set; }
        public double Double { get; set; }
        public string Chars { get; set; }
        public object Obj { get; set; }
        public string Content { get; set; }
        public SmallContainer SmallContainer { get; set; }
        public MediumContainer MediumContainer { get; set; }
        public List<int> ListOfInts { get; set; }
    }

    [ContentProperty("Content")]
    public class SmallContainer
    {
        public int Integer { get; set; }
        public string Chars { get; set; }
        public string Content { get; set; }
    }

    public class WhiteSpaceSignificantCollectionWrapper
    {
        public WhitespaceSignificantCollectionType Collection { get; set; }
    }

    [WhitespaceSignificantCollection]
    public class WhitespaceSignificantCollectionType : List<Element>
    {
    }
}

namespace Test.Elements.AnotherNamespace
{
    public class AnotherContainer
    {
        public int Integer { get; set; }
    }

    public class MediumContainer
    {
        public int Integer { get; set; }
        public static object GetThing(int thing)
        {
            return null;
        }
        public static void SetThing(int thing, object value)
        {
        }
    }
}

namespace Test.Elements.ANamespace
{
    public class DummyContainer
    {
    }
}

namespace Test.Elements.OneMoreNamespace
{
    public class OneMoreContainer
    {
        public int Integer { get; set; }
        public static object GetThing(int thing)
        {
            return null;
        }
        public static void SetThing(int thing, object value)
        {
        }
    }

    public class DummyContainer
    {
    }
}

namespace X.M.L
{
    public class A
    {
        public string B { get; set; }
    }
}