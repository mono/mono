using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Documentation {
    public class Normalizer {
        
        public static void Run (string [] args)
        {
                if (args == null) {
                        Console.WriteLine ("normalize.exe <files>");
                        Environment.Exit (0);
                }

                foreach (string arg in args) {
                        
                        XmlDocument document = new XmlDocument ();
                        try {
                                document.Load (arg);
                                StreamWriter writer = new StreamWriter (arg, false, new UTF8Encoding (false));
                                document.Save (writer);
                                writer.Close ();
                                
                        } catch (XmlException e) {
                                Console.WriteLine (arg + " is not a wellformed XML document.");
                                Console.WriteLine (e.Message);
                        }
                }
        }
    }
}
