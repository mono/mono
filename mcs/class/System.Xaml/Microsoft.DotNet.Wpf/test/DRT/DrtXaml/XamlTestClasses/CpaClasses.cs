using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;

namespace Test.Elements
{
    [ContentProperty("Title")]
    public class HasTitleCpa : ElementWithTitle
    {
    }

    [ContentProperty("Text")]
    public class ChangesInheritedCPAToText : ElementWithTitle
    {
        public string Text { get; set; }   // new syntax for trival properties.
    }

    [ContentProperty("Text")]
    public class HasTextCpa : Element
    {
        public string Text { get; set; }
    }

    public class InheritesTitleCP : HasTextCpa
    {
        public Double SomeOtherDouble { get; set; }
    }

    [ContentProperty("")]
    public class TurnsOffInheritedCPA : HasTextCpa
    {
        public int CountOfSomething { get; set; }
    }

    [ContentProperty(null)]
    public class TurnsOffInheritedCPAwNull : HasTextCpa
    {
        public int CountOfSomething { get; set; }
    }

    //-------------------------------------------------


    [ContentProperty("Content")]
    public class InheritedContentType1: Element
    {
        public object Content { get; set; }
        public string Name { get; set; }
    }

    [ContentProperty("Content")]
    public class InheritedContentType2 : InheritedContentType1
    {
        new public object Content { get; set; }
    }

    [ContentProperty("Content")]
    public class InheritedContentType3 : InheritedContentType1
    {
        new public String Content { get; set; }
    }

}
