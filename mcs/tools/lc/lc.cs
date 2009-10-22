//
// License compiler for mono
//
// Authors:
//   Carlo Kok (ck@remobjects.com)
//
// (C) 2009 RemObjects Software
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace LC
{
    public class LCLicenseContext : DesigntimeLicenseContext
    {
        public string OutputFilename { get; set; }
        public string LicxFilename { get; set; }
    }

    class Program
    {
        static List<String> privatePaths = new List<string>();

        static int Main(string[] args)
        {
            bool verbose = false;
            string target = null;
            string complist = null;
            string targetdir = ".";
            List<string> references = new List<string>();

            bool nologo = false;
            bool help = false;
            OptionSet p = new OptionSet() {
                {"v|verbose", "Verbose output", v => verbose = v!= null },
                {"t|target=", "Target assembly name", v => target = v },
                {"c|complist=","licx file to compile", v => complist = v },
                {"i|load=", "Reference to load", v=> {if (v != null) references.Add(v);}},
                {"o|outdir=", "Output directory for the .licenses file", v=> targetdir = v },
                {"nologo", "Do not display logo", v=> nologo = null != v },
                {"h|?|help", "Show help", v=>help = v != null }
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch(OptionException e) 
            {
                Console.WriteLine("lc: " + e.Message);
                Console.WriteLine("try lc --help for more information");
                return 1;
            }
            if (!nologo) {
                Console.WriteLine("Mono License Compiler");
                Console.WriteLine("Copyright (c) 2009 by RemObjects Software");
            }
            if (help) {
                Console.WriteLine();
                Console.WriteLine("lc -c filename -t targetassembly [-i references] [-v] [-o] [-nologo]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return 1;
            }
            if (extra.Count > 0) {
                Console.WriteLine("Unexpected arguments passed on cmd line");
                return 1;
            }
            if (target == null || complist == null){
                Console.WriteLine("No target/complist passed");
                return 1;
            }
            try {
                if (!File.Exists(complist)) {
                    Console.WriteLine("Could not find file: "+complist);
                    return 1;
                }

                LCLicenseContext ctx = new LCLicenseContext();
                ctx.LicxFilename = complist;
                if (verbose) Console.WriteLine("Input file: "+complist);
                ctx.OutputFilename = Path.Combine(targetdir ??".", target)+".licenses";
                if (verbose) Console.WriteLine("Output filename: "+ctx.OutputFilename);
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                privatePaths.Add(".");
                Dictionary<string, Assembly> loaded = new Dictionary<string, Assembly>();
                foreach (string reference in references) {
                    string path = Path.GetDirectoryName(reference);
                    if (!privatePaths.Contains(path))
                    {
                        if (verbose) Console.WriteLine("Adding " + Path.GetDirectoryName(reference) + " to private paths");
                        privatePaths.Add(path);
                       }
                    Assembly asm = Assembly.LoadFrom(reference);
                    loaded.Add(asm.GetName().Name, asm);
                    if (verbose) Console.WriteLine("Loaded assembly: "+asm.GetName().ToString());

                }

                using (StreamReader sr = new StreamReader(complist))
                {
                    int lineno = 0;
                    string line = "";
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            line = sr.ReadLine();
                            if (line == null || line == "" || line[0] == '#' ) continue;
                            if (verbose) Console.WriteLine("Generating license for: "+line);

                            string[] sLine = line.Split(new char[] { ',' }, 2);
                            Type stype = null;
                            if (sLine.Length == 1)
                            {
                                stype = Type.GetType(line, false, true);
                                if (stype == null)
                                {
                                    foreach (KeyValuePair<string, Assembly> et in loaded)
                                    {
                                        stype = et.Value.GetType(sLine[0], false, true);
                                        if (stype != null) {
                                            if (verbose) Console.WriteLine("Found type in "+et.Key);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (sLine[1].IndexOf(',') >= 0)
                                {
                                    stype = Type.GetType(line, false, true);
                                }
                                else
                                {
                                    string s = sLine[1].Trim();
                                    foreach (KeyValuePair<string, Assembly> et in loaded)
                                    {
                                        if (String.Compare(et.Key, s, true, CultureInfo.InvariantCulture) == 0)
                                        {
                                            stype = et.Value.GetType(sLine[0], false, true);
                                            if (stype != null) {
                                                if (verbose) Console.WriteLine("Found type in "+et.Key);
                                                break;
                                            }
                                        }
                                    }
                                    if (stype == null)
                                    {
                                        foreach (KeyValuePair<string, Assembly> et in loaded)
                                        {
                                            stype = et.Value.GetType(sLine[0], false, true);
                                            if (stype != null) {
                                                if (verbose) Console.WriteLine("Found type in "+et.Key);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (stype == null)
                                throw new Exception("Unable to find type: " + line);
                            LicenseManager.CreateWithContext(stype, ctx);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Exception during compiling " + complist + ": " + lineno);
                            Console.WriteLine(e.ToString());
                        }
                    }
                }

                using (FileStream fs = new FileStream(ctx.OutputFilename, FileMode.Create)) {
                    try {
                    DesigntimeLicenseContextSerializer.Serialize(fs, target.ToUpper(CultureInfo.InvariantCulture), ctx);
                    } catch {}
                    if (fs.Length == 0) // older mono does not support this, but when it does, we should use the proper version.
                        IntSerialize(fs, target.ToUpper(CultureInfo.InvariantCulture), ctx);
                }
                if (verbose)
                    Console.WriteLine("Saved to: "+ Path.GetFullPath(ctx.OutputFilename));
                return 0;
            } catch(Exception e){
                Console.WriteLine("Exception: "+e.ToString());
                return 1;
            }

        }


        private static void IntSerialize(Stream o,
                          string cryptoKey,
                          DesigntimeLicenseContext context)
        {
            Object[] lData = new Object[2];
            lData[0] = cryptoKey;
            Hashtable lNewTable = new Hashtable();
            FieldInfo fi =
                typeof(DesigntimeLicenseContext).GetField("savedLicenseKeys", BindingFlags.NonPublic | BindingFlags.Instance) ??
                typeof(DesigntimeLicenseContext).GetField("keys", BindingFlags.NonPublic | BindingFlags.Instance)
                ;
            Hashtable lOrgTable = (Hashtable)fi.GetValue(context);
            foreach (DictionaryEntry et in lOrgTable)
            {
                if (et.Key is string)
                    lNewTable.Add(et.Key, et.Value);
                else
                    lNewTable.Add(((Type)et.Key).AssemblyQualifiedName, et.Value);
            }
            lData[1] = lNewTable;

            BinaryFormatter lFormatter = new BinaryFormatter();
            lFormatter.Serialize(o, lData);

        }
        static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        static bool CompareAssemblyName(string s1, string s2)
        {
            s1 = s1.ToLowerInvariant().Replace(" ", "");
            s2 = s2.ToLowerInvariant().Replace(" ", "");
            return s1 == s2;
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] lArgs = args.Name.Split(',');
            string lName = lArgs[0].Trim();
            if (loadedAssemblies.ContainsKey(args.Name))
            {
                return loadedAssemblies[args.Name];
            }
            for (int i = 0; i < privatePaths.Count; i++)
            {
                string sPath = Path.Combine(privatePaths[i].ToString(), lName);
                if (File.Exists(sPath + ".dll"))
                    sPath += ".dll";
                else if (File.Exists(sPath + ".DLL"))
                    sPath += ".DLL";
                else if (File.Exists(sPath + ".exe"))
                    sPath += ".exe";
                else if (File.Exists(sPath + ".EXE"))
                    sPath += ".EXE";
                else
                    continue;
                AssemblyName an2 = AssemblyName.GetAssemblyName(sPath);
                if (CompareAssemblyName(an2.ToString(), args.Name) || (lArgs.Length == 1 && CompareAssemblyName(an2.Name, lName)))
                {
                    Assembly asm;
                    try
                    {
                        asm = Assembly.LoadFrom(sPath);
                    }
                    catch
                    {
                        asm = Assembly.LoadFile(sPath);
                    }
                    if (asm != null)
                    {
                        loadedAssemblies.Add(args.Name, asm);
                        return asm;
                    }
                }
            }
            throw new Exception("Unable to find assembly "+args.Name);
        }
    }
}
