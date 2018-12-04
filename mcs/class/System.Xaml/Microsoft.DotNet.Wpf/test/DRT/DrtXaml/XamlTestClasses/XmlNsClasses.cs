using System;
using System.Collections.Generic;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://test.xaml/ns1/2005", "XmlNsClasses.Ns1")]
[assembly: XmlnsDefinition("http://test.xaml/ns1/2008", "XmlNsClasses.Ns1")]
[assembly: XmlnsPrefix("http://test.xaml/ns1/2005","ns1")]
[assembly: XmlnsPrefix("http://test.xaml/ns1/2008","ns1")]
[assembly: XmlnsCompatibleWith("http://test.xaml/ns1/2005", "http://test.xaml/ns1/2008")]
[assembly: XmlnsCompatibleWith("http://test.xaml/ns1/2007", "http://test.xaml/ns1/2008")]

namespace XmlNsClasses.Ns1
{
    public class Foo
    {
        public string Text { get; set; }
    }
}