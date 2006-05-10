using System;
using System.Configuration;
using System.Collections.Generic;

namespace TestLib1
{
    public class Class1
    {
        public void testIT()
        {
	    object o = System.Configuration.ConfigurationManager.GetSection("testlib1");
            List<System.Xml.XmlNode> sectionList = (List<System.Xml.XmlNode>) o;
            Console.WriteLine("count: " + sectionList.Count);
        }
    }
}

namespace TestApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestLib1.Class1 testApp1 = new TestLib1.Class1();
            testApp1.testIT();
        }
    }
}

namespace TestLib1
{
    public sealed class ConfigSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<System.Xml.XmlNode> sectionList = new List<System.Xml.XmlNode>();
            foreach (System.Xml.XmlNode sectionXmlNode in section.SelectSingleNode("mynodes").ChildNodes)
            {
                sectionList.Add(sectionXmlNode);
            }

            return sectionList;
        }
    }
}

