using System;
using System.IO;
using System.Diagnostics;
using System.Xaml;
using System.Reflection;

namespace BamlAvoidXmlTest
{
    class Entry
    {
        enum ReturnValues { GOOD = 0, ALREADYLOADED = 1, WRITERLOADED = 2 };
        static ReturnValues retValue;

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadHandler);
            if (XmlIsLoaded())
            {
                return (int)ReturnValues.ALREADYLOADED;
            }
            retValue = ReturnValues.GOOD;
            LocalClass local = BuildSomething();
            return (int)retValue;
        }

        static LocalClass BuildSomething()
        {
            XamlSchemaContext schema = new XamlSchemaContext();
            XamlObjectWriter xow = new XamlObjectWriter(schema);
            XamlType xamlType = schema.GetXamlType(typeof(LocalClass));
            XamlMember xamlProperty = xamlType.GetMember("Title");

            xow.WriteStartObject(xamlType);
            xow.WriteStartMember(xamlProperty);
            xow.WriteValue("This is a string");
            xow.WriteEndMember();
            xow.WriteEndObject();

            object o = xow.Result;

            LocalClass local = (LocalClass)o;
            return local;
        }

        static void AssemblyLoadHandler(object sender, AssemblyLoadEventArgs args)
        {
            AssemblyName aName = args.LoadedAssembly.GetName();
            if (IsAssemblyNameSystemXml(aName))
            {
                retValue = ReturnValues.WRITERLOADED;
            }
        }

        static bool XmlIsLoaded()
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly a in asms)
            {
                AssemblyName aName = a.GetName();
                if (IsAssemblyNameSystemXml(aName))
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsAssemblyNameSystemXml(AssemblyName aName)
        {
            if (aName.Name.Equals("system.xml", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("BamlAvoidXmlTest FAILED.  System.Xml was loaded and it should not be.");
                return true;
            }
            return false;
        }
    }

    class LocalClass
    {
        public string Title { get; set; }
    }
}