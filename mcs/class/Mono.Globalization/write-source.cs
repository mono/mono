using System;
using System.IO;
using System.Text;
using System.Xml;

class ClassWriter {

        StreamWriter writer;
        XmlDocument document;
        static readonly string ns = "Mono.Globalization";

        public ClassWriter (string filename)
        {
                string path = Path.GetFileName (filename);
                path = Path.ChangeExtension (path, "cs");
                path = Path.Combine ("src", path);

                this.writer = new StreamWriter (path);
                this.document = new XmlDocument ();

                document.Load (filename);
        }

        public void WriteStartNamespaceDecl ()
        {
                writer.Write ("namespace {0} {{\n", ns);
        }

        public void WriteEndNamespaceDecl ()
        {
                writer.Write ("}");
        }

        public void WriteStartClassDef (string class_name)
        {
                writer.Write ("\tpublic class {0} : MonoCulture\n\t{{\n", class_name);
        }

        public void WriteEndClassDef ()
        {
                writer.Write ("\t}\n");
        }

        public void WriteStartConstructor (string class_name)
        {
                writer.Write ("\t\tstatic {0} ()\n\t\t{{\n", class_name);
        }

        public void WriteEndConstructor ()
        {
                writer.Write ("\t\t}\n");
        }

        public void SetMember (string member)
        {
                string expression = String.Format ("/CultureInfo/{0}", member);
                XmlNode result = document.SelectSingleNode (expression);
                if (result == null)
                        return;

                SetMember (member, result.InnerXml);
        }

        public void SetMember (string member, string value)
        {
                if (member.EndsWith ("Name"))
                        value = String.Format ("\"{0}\"", value);

                writer.Write ("\t\t\t{0} = {1};\n", member, value);
        }

        public void SetCalendar ()
        {
                string expression = String.Format ("/CultureInfo/Calendar");
                XmlNode result = document.SelectSingleNode (expression);

                writer.Write ("\t\t\tCalendar = new {0} ();\n", result.InnerXml);
        }

        public void SetParent ()
        {
                string expression = String.Format ("/CultureInfo/Parent");
                XmlNode result = document.SelectSingleNode (expression);

                if (result == null)
                        return;

                writer.Write (String.Format ("\t\t\tParent = System.Globalization.CultureInfo.CreateSpecificCulture (\"{0}\");\n", result.InnerXml));
        }

        public void WriteComment (string comment)
        {
                writer.Write ("\t\t\t// {0}\n", comment);
        }

        public void Write ()
        {
                WriteStartNamespaceDecl ();
                WriteStartClassDef (ClassName);
                WriteStartConstructor (ClassName);
                SetCalendar ();
                SetMember ("EnglishName");
                SetMember ("LCID");
                SetMember ("Name");
                SetMember ("NativeName");
                SetParent ();
                SetMember ("ThreeLetterISOLanguageName");
                SetMember ("TwoLetterISOLanguageName");


                WriteEndConstructor ();
                WriteEndClassDef ();
                WriteEndNamespaceDecl ();

                Console.WriteLine ("Writing {0}", ClassName);
                writer.Flush ();
        }

        public string ClassName {
                get {
                        string culture =  document.SelectSingleNode ("/CultureInfo/@name").InnerText;
                        return String.Format ("MonoCulture__{0}", culture.Replace ("-", "_"));
                }
        }

        public bool IsNeutral {
                get {
                        if (document.SelectSingleNode (String.Format ("/CultureInfo/Parent")) == null)
                                return true;

                        return false;
                }
        }
}

class Driver {
        static void Main ()
        {
                int i = 0;
                ClassWriter w;

                foreach (string file in Directory.GetFiles ("./Data", "*.xml")) {
                        w = new ClassWriter (file);
                        w.Write ();
                }
        }
}
