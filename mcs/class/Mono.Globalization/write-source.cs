using System;
using System.IO;
using System.Text;
using System.Xml;

class ClassWriter {

        public StreamWriter writer;
        XmlDocument document;
        static readonly string ns = "Mono.Globalization";
        static readonly string path = "MonoCultures.cs";

        public ClassWriter ()
        {
                this.writer = new StreamWriter (path);
                this.document = new XmlDocument ();
        }

        public void WriteStartNamespaceDecl ()
        {
                writer.Write ("namespace {0} {{\n", ns);
        }

        public void WriteEndNamespaceDecl ()
        {
                writer.Write ("}");
        }

        public void WriteStartClassDef (string class_name, string base_class)
        {
                writer.Write ("\tclass {0} : {1}\n\t{{\n", class_name, base_class);
        }

        public void WriteEndClassDef ()
        {
                writer.Write ("\t}\n\n");
        }

        public void WriteStartMethod (string method)
        {
                writer.Write ("\t\tvoid {0} ()\n\t\t{{\n", method);
        }

        public void WriteEndMethod ()
        {
                writer.Write ("\t\t}\n");
        }

        public void WriteConstructor (string name)
        {
                writer.Write ("\t\tprivate {0} () : base (0) {{}}\n", name);
        }

        public void WriteStartConstructor (string class_name)
        {
                writer.Write ("\t\tpublic {0} ()\n\t\t{{\n", class_name);
        }

        public void WriteEndConstructor ()
        {
                writer.Write ("\t\t}\n");
        }

        public void WriteMember (string member)
        {
                string expression = String.Format ("/CultureInfo/{0}", member);
                XmlNode result = document.SelectSingleNode (expression);
                if (result == null)
                        return;

                WriteMember (member, result);
        }

        public void WriteMember (string member, XmlNode node)
        {
                string value;

                if (member.EndsWith ("Name") || member.EndsWith ("Separator") || 
                        member.EndsWith ("Symbol") || member.EndsWith ("Sign")) 

                        value = String.Format ("\"{0}\"", node.InnerXml);
                else
                        value = node.InnerXml;

                writer.Write ("\t\t\t{0} = {1};\n", member, value);
        }

        public void WriteCalendar ()
        {
                string expression = String.Format ("/CultureInfo/Calendar");
                XmlNode result = document.SelectSingleNode (expression);

                writer.Write ("\t\t\tCalendar = new {0} ();\n", result.InnerXml);
        }

        public void WriteCompareInfo ()
        {
                XmlNode compare_info_lcid = document.SelectSingleNode ("/CultureInfo/CompareInfo/@LCID");
                WriteMember ("CompareInfo_LCID", compare_info_lcid);
        }
        
        public void WriteTextInfo ()
        {
                XmlNode info = document.SelectSingleNode ("/CultureInfo/TextInfo");
                
                WriteMember ("TextInfo_ANSICodePage", info.SelectSingleNode ("ANSICodePage"));
                WriteMember ("TextInfo_EBCDICCodePage", info.SelectSingleNode ("EBCDICCodePage"));
                WriteMember ("TextInfo_MacCodePage", info.SelectSingleNode ("MacCodePage"));
                WriteMember ("TextInfo_OEMCodePage", info.SelectSingleNode ("OEMCodePage"));
                WriteMember ("TextInfo_ListSeparator", info.SelectSingleNode ("ListSeparator"));
        }

        public string WriteArray (XmlNode node)
        {
                string value = String.Format ("new int [{0}] {{", node.ChildNodes.Count);

                foreach (XmlNode n in node.ChildNodes)
                        value += n.InnerText;
                
                value = value + " }";

                return value;
        }

        public string WriteString (XmlNode node)
        {
                return "\"" + node.InnerText + "\"";
        }

        public void WriteNumberFormatInfo ()
        {
                XmlNode info = document.SelectSingleNode ("/CultureInfo/NumberFormat");
                
                if (info == null)
                        return;
                
                string number_format = 
                        String.Format (
                        @"NumberFormat = new System.Globalization.NumberFormatInfo (
                                {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, 
                                {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},
                                {21}, {22}, {23});",
                                info.SelectSingleNode ("CurrencyDecimalDigits").InnerXml,
                                WriteString (info.SelectSingleNode ("CurrencyDecimalSeparator")),
                                WriteString (info.SelectSingleNode ("CurrencyGroupSeparator")),
                                WriteArray (info.SelectSingleNode ("CurrencyGroupSizes")),
                                info.SelectSingleNode ("CurrencyNegativePattern").InnerXml,
                                info.SelectSingleNode ("CurrencyPositivePattern").InnerXml,
                                WriteString (info.SelectSingleNode ("CurrencySymbol")), 
                                WriteString (info.SelectSingleNode ("NaNSymbol")), 
                                WriteString (info.SelectSingleNode ("NegativeInfinitySymbol")),
                                WriteString (info.SelectSingleNode ("NegativeSign")),
                                info.SelectSingleNode ("NumberDecimalDigits").InnerXml, 
                                WriteString (info.SelectSingleNode ("NumberDecimalSeparator")),
                                WriteArray (info.SelectSingleNode ("NumberGroupSizes")),
                                info.SelectSingleNode ("NumberNegativePattern").InnerXml, 
                                info.SelectSingleNode ("PercentDecimalDigits").InnerXml,
                                WriteString (info.SelectSingleNode ("PercentDecimalSeparator")), 
                                WriteString (info.SelectSingleNode ("PercentGroupSeparator")), 
                                WriteArray (info.SelectSingleNode ("PercentGroupSizes")), 
                                info.SelectSingleNode ("PercentNegativePattern").InnerXml, 
                                info.SelectSingleNode ("PercentPositivePattern").InnerXml,
                                WriteString (info.SelectSingleNode ("PercentSymbol")), 
                                WriteString (info.SelectSingleNode ("PerMilleSymbol")), 
                                WriteString (info.SelectSingleNode ("PositiveInfinitySymbol")), 
                                WriteString (info.SelectSingleNode ("PositiveSign")));
                
                writer.Write ("\t\t\t" + number_format + "\n");
        }

        public void WriteParent ()
        {
                string expression = String.Format ("/CultureInfo/Parent");
                XmlNode result = document.SelectSingleNode (expression);

                if (result == null)
                        return;

                WriteMember ("ParentName", result);
        }

        public void WriteComment (string comment)
        {
                writer.Write ("\t\t// {0}\n", comment);
        }

        public void Load (string file)
        {
                document.Load (file);
        }

        public void Write ()
        {
                WriteStartClassDef (ClassName, "System.Globalization.CultureInfo.MonoCulture");

                WriteStartConstructor (ClassName);

                WriteCalendar ();
                WriteMember ("EnglishName");
                WriteMember ("LCID");
                WriteMember ("Name");
                WriteMember ("NativeName");
                WriteParent ();
                WriteMember ("ThreeLetterISOLanguageName");
                WriteMember ("ThreeLetterWindowsLanguageName"); 
                WriteMember ("TwoLetterISOLanguageName");
                WriteTextInfo ();
                WriteCompareInfo ();
                WriteNumberFormatInfo ();

                WriteEndConstructor ();
                WriteEndClassDef ();
                writer.Flush ();
        }

        public string ClassName {
                get {
                        string culture =  document.SelectSingleNode ("/CultureInfo/LCID").InnerText;
                        return String.Format ("MonoCulture__{0}", culture);
                }
        }
}

class Driver {
        static void Main ()
        {
                int i = 0;
                ClassWriter w = new ClassWriter ();
                w.WriteStartNamespaceDecl ();
                w.WriteStartClassDef ("MonoCultureInfo", "System.Globalization.CultureInfo");
                w.WriteComment ("This should never be invoked");
                w.WriteConstructor ("MonoCultureInfo");

                foreach (string file in Directory.GetFiles ("./Data", "*.xml")) {
                        Console.WriteLine (file);
                        w.Load (file);
                        w.Write ();
                }

                w.WriteEndClassDef ();
                w.WriteEndNamespaceDecl ();

                w.writer.Flush ();
                Console.WriteLine ("Done");
        }
}
