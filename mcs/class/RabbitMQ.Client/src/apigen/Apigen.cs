// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using RabbitMQ.Client.Apigen.Attributes;

namespace RabbitMQ.Client.Apigen {
    public class Apigen {
        ///////////////////////////////////////////////////////////////////////////
        // Entry point

        public static void Main(string[] args) {
            Apigen instance = new Apigen(new ArrayList(args));
            instance.Generate();
        }

        ///////////////////////////////////////////////////////////////////////////
        // XML utilities

        public static XmlNodeList GetNodes(XmlNode n0, string path) {
            return n0.SelectNodes(path);
        }

        public static string GetString(XmlNode n0, string path, string d) {
            XmlNode n = n0.SelectSingleNode(path);
            return (n == null) ? d : n.InnerText;
        }

        public static string GetString(XmlNode n0, string path) {
            string s = GetString(n0, path, null);
            if (s == null) {
                throw new Exception("Missing spec XML node: " + path);
            }
            return s;
        }

        public static int GetInt(XmlNode n0, string path, int d) {
            string s = GetString(n0, path, null);
            return (s == null) ? d : int.Parse(s);
        }

        public static int GetInt(XmlNode n0, string path) {
            return int.Parse(GetString(n0, path));
        }

        ///////////////////////////////////////////////////////////////////////////
        // Name manipulation and mangling for C#

        public static string MangleConstant(string name) {
            // Previously, we used C_STYLE_CONSTANT_NAMES:
            /*
              return name
              .Replace(" ", "_")
              .Replace("-", "_").
              ToUpper();
            */
            // ... but TheseKindsOfNames are more in line with .NET style guidelines.
            return MangleClass(name);
        }

        public static ArrayList IdentifierParts(string name) {
            ArrayList result = new ArrayList();
            foreach (String s1 in name.Split(new Char[] { '-' })) {
                foreach (String s2 in s1.Split(new Char[] { ' ' })) {
                    result.Add(s2);
                }
            }
            return result;
        }

        public static string MangleClass(string name) {
            StringBuilder sb = new StringBuilder();
            foreach (String s in IdentifierParts(name)) {
                sb.Append(Char.ToUpper(s[0]) + s.Substring(1).ToLower());
            }
            return sb.ToString();
        }

        public static string MangleMethod(string name) {
            StringBuilder sb = new StringBuilder();
            bool useUpper = false;
            foreach (String s in IdentifierParts(name)) {
                if (useUpper) {
                    sb.Append(Char.ToUpper(s[0]) + s.Substring(1).ToLower());
                } else {
                    sb.Append(s.ToLower());
                    useUpper = true;
                }
            }
            return sb.ToString();
        }

        public static string MangleMethodClass(AmqpClass c, AmqpMethod m) {
            return MangleClass(c.Name) + MangleClass(m.Name);
        }

        ///////////////////////////////////////////////////////////////////////////

        public string framingSubnamespace = null;
        public string inputXmlFilename;
        public string outputFilename;

        public XmlDocument spec = null;
        public TextWriter outputFile = null;

        public bool versionOverridden = false;
        public int majorVersion;
        public int minorVersion;
        public string apiName;

        public Type modelType = typeof(RabbitMQ.Client.Impl.IFullModel);
	public ArrayList modelTypes = new ArrayList();
        public ArrayList constants = new ArrayList();
        public ArrayList classes = new ArrayList();
        public Hashtable domains = new Hashtable();

        public static Hashtable primitiveTypeMap;
        public static Hashtable primitiveTypeFlagMap;
        static Apigen() {
            primitiveTypeMap = new Hashtable();
	    primitiveTypeFlagMap = new Hashtable();
            InitPrimitiveType("octet", "byte", false);
            InitPrimitiveType("shortstr", "string", true);
            InitPrimitiveType("longstr", "byte[]", true);
            InitPrimitiveType("short", "ushort", false);
            InitPrimitiveType("long", "uint", false);
            InitPrimitiveType("longlong", "ulong", false);
            InitPrimitiveType("bit", "bool", false);
            InitPrimitiveType("table", "System.Collections.IDictionary", true);
            InitPrimitiveType("timestamp", "AmqpTimestamp", false);
            InitPrimitiveType("content", "byte[]", true);
        }

	public static void InitPrimitiveType(string amqpType, string dotnetType, bool isReference)
	{
	    primitiveTypeMap[amqpType] = dotnetType;
	    primitiveTypeFlagMap[amqpType] = isReference;
	}

        public void HandleOption(string opt) {
            if (opt.StartsWith("/n:")) {
                framingSubnamespace = opt.Substring(3);
            } else if (opt.StartsWith("/apiName:")) {
                apiName = opt.Substring(9);
            } else if (opt.StartsWith("/v:")) {
                string[] parts = opt.Substring(3).Split(new char[] { '-' });
                versionOverridden = true;
                majorVersion = int.Parse(parts[0]);
                minorVersion = int.Parse(parts[1]);
            } else {
                Console.Error.WriteLine("Unsupported command-line option: " + opt);
                Usage();
            }
        }

        public void Usage() {
            Console.Error.WriteLine("Usage: Apigen.exe [options ...] <input-spec-xml> <output-csharp-file>");
            Console.Error.WriteLine("  Options include:");
            Console.Error.WriteLine("    /apiName:<identifier>");
            Console.Error.WriteLine("    /n:<name.space.prefix>");
            Console.Error.WriteLine("    /v:<majorversion>-<minorversion>");
            Console.Error.WriteLine("  The apiName option is required.");
            Environment.Exit(1);
        }

        public Apigen(ArrayList args) {
            while (args.Count > 0 && ((string) args[0]).StartsWith("/")) {
                HandleOption((string) args[0]);
                args.RemoveAt(0);
            }
            if ((args.Count < 2)
                || (apiName == null))
            {
                Usage();
            }
            this.inputXmlFilename = (string) args[0];
            this.outputFilename = (string) args[1];
        }

        ///////////////////////////////////////////////////////////////////////////

        public string FramingSubnamespace {
            get {
                if (framingSubnamespace == null) {
                    return VersionToken();
                } else {
                    return framingSubnamespace;
                }
            }
        }

        public string ApiNamespaceBase {
            get {
                return "RabbitMQ.Client.Framing."+FramingSubnamespace;
            }
        }

        public string ImplNamespaceBase {
            get {
                return "RabbitMQ.Client.Framing.Impl."+FramingSubnamespace;
            }
        }

        public void Generate() {
            LoadSpec();
            ParseSpec();
	    ReflectModel();
            GenerateOutput();
        }

        public void LoadSpec() {
            Console.WriteLine("* Loading spec from '" + this.inputXmlFilename + "'");
            this.spec = new XmlDocument();
            this.spec.Load(this.inputXmlFilename);
        }

        public void ParseSpec() {
            Console.WriteLine("* Parsing spec");
            if (!versionOverridden) {
                majorVersion = GetInt(spec, "/amqp/@major");
                minorVersion = GetInt(spec, "/amqp/@minor");
            }
            foreach (XmlNode n in spec.SelectNodes("/amqp/constant")) {
                constants.Add(new DictionaryEntry(GetString(n, "@name"), GetInt(n, "@value")));
            }
            foreach (XmlNode n in spec.SelectNodes("/amqp/class")) {
                classes.Add(new AmqpClass(n));
            }
            foreach (XmlNode n in spec.SelectNodes("/amqp/domain")) {
                domains[GetString(n, "@name")] = GetString(n, "@type");
            }
        }

	public void ReflectModel() {
            modelTypes.Add(modelType);
            for (int i = 0; i < modelTypes.Count; i++) {
                foreach (Type intf in ((Type) modelTypes[i]).GetInterfaces()) {
                    modelTypes.Add(intf);
                }
            }
	}

        public string ResolveDomain(string d) {
            while (domains[d] != null) {
                string newD = (string) domains[d];
                if (d.Equals(newD))
                    break;
                d = newD;
            }
            return d;
        }

        public string MapDomain(string d) {
            return (string) primitiveTypeMap[ResolveDomain(d)];
        }

        public string VersionToken() {
            return "v" + majorVersion + "_" + minorVersion;
        }

        public void GenerateOutput() {
            Console.WriteLine("* Generating code into '" + this.outputFilename + "'");
            this.outputFile = new StreamWriter(this.outputFilename);
            EmitPrelude();
            EmitPublic();
            EmitPrivate();
            this.outputFile.Close();
        }

        public void Emit(object o) {
            this.outputFile.Write(o);
        }

        public void EmitLine(object o) {
            this.outputFile.WriteLine(o);
        }

        public void EmitPrelude() {
            EmitLine("// Autogenerated code. Do not edit.");
            EmitLine("");
            EmitLine("using RabbitMQ.Client;");
            EmitLine("using RabbitMQ.Client.Exceptions;");
            EmitLine("");
        }

        public void EmitPublic() {
            EmitLine("namespace "+ApiNamespaceBase+" {");
            EmitLine("  public class Protocol: "+ImplNamespaceBase+".ProtocolBase {");
            EmitLine("    ///<summary>Protocol major version (= "+majorVersion+")</summary>");
            EmitLine("    public override int MajorVersion { get { return " + majorVersion + "; } }");
            EmitLine("    ///<summary>Protocol minor version (= "+minorVersion+")</summary>");
            EmitLine("    public override int MinorVersion { get { return " + minorVersion + "; } }");
            EmitLine("    ///<summary>Protocol API name (= "+apiName+")</summary>");
            EmitLine("    public override string ApiName { get { return \"" + apiName + "\"; } }");
            int port = GetInt(spec, "/amqp/@port");
            EmitLine("    ///<summary>Default TCP port (= "+port+")</summary>");
            EmitLine("    public override int DefaultPort { get { return " + port + "; } }");
            EmitLine("");
            EmitMethodArgumentReader();
            EmitLine("");
            EmitContentHeaderReader();
            EmitLine("  }");
            EmitLine("  public class Constants {");
            foreach (DictionaryEntry de in constants) {
                EmitLine("    ///<summary>(= "+de.Value+")</summary>");
                EmitLine("    public const int "+MangleConstant((string) de.Key)+" = "+de.Value+";");
            }
            EmitLine("  }");
            foreach (AmqpClass c in classes) {
                EmitClassMethods(c);
            }
            foreach (AmqpClass c in classes) {
                if (c.NeedsProperties) {
                    EmitClassProperties(c);
                }
            }
            EmitLine("}");
        }

        public void EmitAutogeneratedSummary(string prefixSpaces, string extra) {
            EmitLine(prefixSpaces+"/// <summary>Autogenerated type. "+extra+"</summary>");
        }

        public void EmitClassMethods(AmqpClass c) {
            foreach (AmqpMethod m in c.Methods) {
                EmitAutogeneratedSummary("  ",
                                         "AMQP specification method \""+c.Name+"."+m.Name+"\".");
                EmitLine(m.DocumentationCommentVariant("  ", "remarks"));
                EmitLine("  public interface I"+MangleMethodClass(c, m)+": IMethod {");
                foreach (AmqpField f in m.Fields) {
                    EmitLine(f.DocumentationComment("    "));
                    EmitLine("    "+MapDomain(f.Domain)+" "+MangleClass(f.Name)+" { get; }");
                }
                EmitLine("  }");
            }
        }

	public bool HasFactoryMethod(AmqpClass c) {
	    foreach (Type t in modelTypes) {
		foreach (MethodInfo method in t.GetMethods()) {
		    AmqpContentHeaderFactoryAttribute f = (AmqpContentHeaderFactoryAttribute)
			Attribute(method, typeof(AmqpContentHeaderFactoryAttribute));
		    if (f != null && MangleClass(f.m_contentClass) == MangleClass(c.Name)) {
			return true;
		    }
		}
	    }
	    return false;
	}

	public bool IsBoolean(AmqpField f) {
	    return ResolveDomain(f.Domain) == "bit";
	}

	public bool IsReferenceType(AmqpField f) {
	    return (bool) primitiveTypeFlagMap[ResolveDomain(f.Domain)];
	}

        public void EmitClassProperties(AmqpClass c) {
	    bool hasCommonApi = HasFactoryMethod(c);
	    string propertiesBaseClass =
		hasCommonApi
		? "RabbitMQ.Client.Impl."+MangleClass(c.Name)+"Properties"
		: "RabbitMQ.Client.Impl.ContentHeaderBase";
	    string maybeOverride = hasCommonApi ? "override " : "";

            EmitAutogeneratedSummary("  ",
                                     "AMQP specification content header properties for "+
                                     "content class \""+c.Name+"\"");
            EmitLine(c.DocumentationCommentVariant("  ", "remarks"));
            EmitLine("  public class "+MangleClass(c.Name)
                     +"Properties: "+propertiesBaseClass+" {");
            foreach (AmqpField f in c.Fields) {
                EmitLine("    private "+MapDomain(f.Domain)+" m_"+MangleMethod(f.Name)+";");
            }
            EmitLine("");
            foreach (AmqpField f in c.Fields) {
		if (!IsBoolean(f)) {
		    EmitLine("    private bool "+MangleMethod(f.Name)+"_present = false;");
		}
            }
            EmitLine("");
            foreach (AmqpField f in c.Fields) {
                EmitLine(f.DocumentationComment("    ", "@label"));
                EmitLine("    public "+maybeOverride+MapDomain(f.Domain)+" "+MangleClass(f.Name)+" {");
                EmitLine("      get {");
                EmitLine("        return m_"+MangleMethod(f.Name)+";");
                EmitLine("      }");
                EmitLine("      set {");
		if (!IsBoolean(f)) {
		    EmitLine("        "+MangleMethod(f.Name)+"_present = true;");
		}
                EmitLine("        m_"+MangleMethod(f.Name)+" = value;");
                EmitLine("      }");
                EmitLine("    }");
            }
            EmitLine("");
            foreach (AmqpField f in c.Fields) {
		if (!IsBoolean(f)) {
		    EmitLine("    public "+maybeOverride+"void Clear"+MangleClass(f.Name)+"() { "+MangleMethod(f.Name)+"_present = false; }");
		}
            }
            EmitLine("");
            EmitLine("    public "+MangleClass(c.Name)+"Properties() {}");
            EmitLine("    public override int ProtocolClassId { get { return "+c.Index+"; } }");
            EmitLine("    public override string ProtocolClassName { get { return \""+c.Name+"\"; } }");
            EmitLine("");
            EmitLine("    public override void ReadPropertiesFrom(RabbitMQ.Client.Impl.ContentHeaderPropertyReader reader) {");
            foreach (AmqpField f in c.Fields) {
		if (IsBoolean(f)) {
		    EmitLine("      m_"+MangleMethod(f.Name)+" = reader.ReadBit();");
		} else {
		    EmitLine("      "+MangleMethod(f.Name)+"_present = reader.ReadPresence();");
		}
            }
	    EmitLine("      reader.FinishPresence();");
            foreach (AmqpField f in c.Fields) {
		if (!IsBoolean(f)) {
		    EmitLine("      if ("+MangleMethod(f.Name)+"_present) { m_"+MangleMethod(f.Name)+" = reader.Read"+MangleClass(ResolveDomain(f.Domain))+"(); }");
		}
            }
            EmitLine("    }");
            EmitLine("");
            EmitLine("    public override void WritePropertiesTo(RabbitMQ.Client.Impl.ContentHeaderPropertyWriter writer) {");
            foreach (AmqpField f in c.Fields) {
		if (IsBoolean(f)) {
		    EmitLine("      writer.WriteBit(m_"+MangleMethod(f.Name)+");");
		} else {
		    EmitLine("      writer.WritePresence("+MangleMethod(f.Name)+"_present);");
		}
            }
	    EmitLine("      writer.FinishPresence();");
            foreach (AmqpField f in c.Fields) {
		if (!IsBoolean(f)) {
		    EmitLine("      if ("+MangleMethod(f.Name)+"_present) { writer.Write"+MangleClass(ResolveDomain(f.Domain))+"(m_"+MangleMethod(f.Name)+"); }");
		}
            }
            EmitLine("    }");
            EmitLine("");
            EmitLine("    public override void AppendPropertyDebugStringTo(System.Text.StringBuilder sb) {");
            EmitLine("      sb.Append(\"(\");");
            {
                int remaining = c.Fields.Count;
                foreach (AmqpField f in c.Fields) {
                    Emit("      sb.Append(\""+f.Name+"=\");");
		    if (IsBoolean(f)) {
			Emit(" sb.Append(m_"+MangleMethod(f.Name)+");");
		    } else {
			string x = MangleMethod(f.Name);
			if (IsReferenceType(f)) {
			    Emit(" sb.Append("+x+"_present ? (m_"+x+" == null ? \"(null)\" : m_"+x+".ToString()) : \"_\");");
			} else {
			    Emit(" sb.Append("+x+"_present ? m_"+x+".ToString() : \"_\");");
			}
		    }
                    remaining--;
                    if (remaining > 0) {
                        EmitLine(" sb.Append(\", \");");
                    } else {
                        EmitLine("");
                    }
                }
            }
            EmitLine("      sb.Append(\")\");");
            EmitLine("    }");
            EmitLine("  }");
        }

        public void EmitPrivate() {
            EmitLine("namespace "+ImplNamespaceBase+" {");
            EmitLine("  using "+ApiNamespaceBase+";");
            EmitLine("  public enum ClassId {");
            foreach (AmqpClass c in classes) {
                EmitLine("    "+MangleConstant(c.Name)+" = "+c.Index+",");
            }
            EmitLine("    Invalid = -1");
            EmitLine("  }");
            foreach (AmqpClass c in classes) {
                EmitClassMethodImplementations(c);
            }
            EmitLine("");
            EmitModelImplementation();
            EmitLine("}");
        }

        public void EmitClassMethodImplementations(AmqpClass c) {
            foreach (AmqpMethod m in c.Methods) {
                EmitAutogeneratedSummary("  ",
                                         "Private implementation class - do not use directly.");
                EmitLine("  public class "+MangleMethodClass(c,m)
                         +": RabbitMQ.Client.Impl.MethodBase, I"+MangleMethodClass(c,m)+" {");
                EmitLine("    public const int ClassId = "+c.Index+";");
                EmitLine("    public const int MethodId = "+m.Index+";");
                EmitLine("");
                foreach (AmqpField f in m.Fields) {
                    EmitLine("    public "+MapDomain(f.Domain)+" m_"+MangleMethod(f.Name)+";");
                }
                EmitLine("");
                foreach (AmqpField f in m.Fields) {
                    EmitLine("    "+MapDomain(f.Domain)+" I"+MangleMethodClass(c,m)+
                             "."+MangleClass(f.Name)+" { get {"
                             +" return m_"+MangleMethod(f.Name)+"; } }");
                }
                EmitLine("");
                if (m.Fields.Count > 0) {
                    EmitLine("    public "+MangleMethodClass(c,m)+"() {}");
                }
                EmitLine("    public "+MangleMethodClass(c,m)+"(");
                {
                    int remaining = m.Fields.Count;
                    foreach (AmqpField f in m.Fields) {
                        Emit("      "+MapDomain(f.Domain)+" init"+MangleClass(f.Name));
                        remaining--;
                        if (remaining > 0) {
                            EmitLine(",");
                        }
                    }
                }
                EmitLine(")");
                EmitLine("    {");
                foreach (AmqpField f in m.Fields) {
                    EmitLine("      m_"+MangleMethod(f.Name)+" = init"+MangleClass(f.Name)+";");
                }
                EmitLine("    }");
                EmitLine("");
                EmitLine("    public override int ProtocolClassId { get { return "+c.Index+"; } }");
                EmitLine("    public override int ProtocolMethodId { get { return "+m.Index+"; } }");
                EmitLine("    public override string ProtocolMethodName { get { return \""+c.Name+"."+m.Name+"\"; } }");
                EmitLine("    public override bool HasContent { get { return "
                         +(m.HasContent ? "true" : "false")+"; } }");
                EmitLine("");
                EmitLine("    public override void ReadArgumentsFrom(RabbitMQ.Client.Impl.MethodArgumentReader reader) {");
                foreach (AmqpField f in m.Fields) {
                    EmitLine("      m_"+MangleMethod(f.Name)+" = reader.Read"+MangleClass(ResolveDomain(f.Domain))+"();");
                }
                EmitLine("    }");
                EmitLine("");
                EmitLine("    public override void WriteArgumentsTo(RabbitMQ.Client.Impl.MethodArgumentWriter writer) {");
                foreach (AmqpField f in m.Fields) {
                    EmitLine("      writer.Write"+MangleClass(ResolveDomain(f.Domain))
                             +"(m_"+MangleMethod(f.Name)+");");
                }
                EmitLine("    }");
                EmitLine("");
                EmitLine("    public override void AppendArgumentDebugStringTo(System.Text.StringBuilder sb) {");
                EmitLine("      sb.Append(\"(\");");
                {
                    int remaining = m.Fields.Count;
                    foreach (AmqpField f in m.Fields) {
                        Emit("      sb.Append(m_"+MangleMethod(f.Name)+");");
                        remaining--;
                        if (remaining > 0) {
                            EmitLine(" sb.Append(\",\");");
                        } else {
                            EmitLine("");
                        }
                    }
                }
                EmitLine("      sb.Append(\")\");");
                EmitLine("    }");
                EmitLine("  }");
            }
        }

        public void EmitMethodArgumentReader() {
            EmitLine("    public override RabbitMQ.Client.Impl.MethodBase DecodeMethodFrom(RabbitMQ.Util.NetworkBinaryReader reader) {");
            EmitLine("      ushort classId = reader.ReadUInt16();");
            EmitLine("      ushort methodId = reader.ReadUInt16();");
            EmitLine("");
            EmitLine("      switch (classId) {");
            foreach (AmqpClass c in classes) {
                EmitLine("        case "+c.Index+": {");
                EmitLine("          switch (methodId) {");
                foreach (AmqpMethod m in c.Methods) {
                    EmitLine("            case "+m.Index+": {");
                    EmitLine("              "+ImplNamespaceBase+"."+MangleMethodClass(c,m)+" result = new "+ImplNamespaceBase+"."+MangleMethodClass(c,m)+"();");
                    EmitLine("              result.ReadArgumentsFrom(new RabbitMQ.Client.Impl.MethodArgumentReader(reader));");
                    EmitLine("              return result;");
                    EmitLine("            }");
                }
                EmitLine("            default: break;");
                EmitLine("          }");
                EmitLine("          break;");
                EmitLine("        }");
            }
            EmitLine("        default: break;");
            EmitLine("      }");
            EmitLine("      throw new RabbitMQ.Client.Impl.UnknownClassOrMethodException(classId, methodId);");
            EmitLine("    }");
        }

        public void EmitContentHeaderReader() {
            EmitLine("    public override RabbitMQ.Client.Impl.ContentHeaderBase DecodeContentHeaderFrom(RabbitMQ.Util.NetworkBinaryReader reader) {");
            EmitLine("      ushort classId = reader.ReadUInt16();");
            EmitLine("");
            EmitLine("      switch (classId) {");
            foreach (AmqpClass c in classes) {
                if (c.NeedsProperties) {
                    EmitLine("        case "+c.Index+": return new "
                             +MangleClass(c.Name)+"Properties();");
                }
            }
            EmitLine("        default: break;");
            EmitLine("      }");
            EmitLine("      throw new RabbitMQ.Client.Impl.UnknownClassOrMethodException(classId, 0);");
            EmitLine("    }");
        }

        public Attribute Attribute(MemberInfo mi, Type t) {
            return Attribute(mi.GetCustomAttributes(t, false), t);
        }

        public Attribute Attribute(ParameterInfo pi, Type t) {
            return Attribute(pi.GetCustomAttributes(t, false), t);
        }

        public Attribute Attribute(ICustomAttributeProvider p, Type t) {
            return Attribute(p.GetCustomAttributes(t, false), t);
        }

        public Attribute Attribute(IEnumerable attributes, Type t) {
            if (t.IsSubclassOf(typeof(AmqpApigenAttribute))) {
                AmqpApigenAttribute result = null;
                foreach (AmqpApigenAttribute candidate in attributes) {
                    if (candidate.m_namespaceName == null && result == null) {
                        result = candidate;
                    }
                    if (candidate.m_namespaceName == ApiNamespaceBase) {
                        result = candidate;
                    }
                }
                return result;
            } else {
                foreach (Attribute attribute in attributes) {
                    return attribute;
                }
                return null;
            }
        }

        public void EmitModelImplementation() {
            EmitLine("  public class Model: RabbitMQ.Client.Impl.ModelBase {");
            EmitLine("    public Model(RabbitMQ.Client.Impl.ISession session): base(session) {}");
            ArrayList asynchronousHandlers = new ArrayList();
            foreach (Type t in modelTypes) {
                foreach (MethodInfo method in t.GetMethods()) {
                    if (method.DeclaringType.Namespace != null &&
                        method.DeclaringType.Namespace.StartsWith("RabbitMQ.Client")) {
                        if (method.Name.StartsWith("Handle") ||
                            (Attribute(method, typeof(AmqpAsynchronousHandlerAttribute)) != null))
                        {
                            asynchronousHandlers.Add(method);
                        } else {
                            MaybeEmitModelMethod(method);
                        }
                    }
                }
            }
            EmitAsynchronousHandlers(asynchronousHandlers);
            EmitLine("  }");
        }

	public void EmitContentHeaderFactory(MethodInfo method) {
	    AmqpContentHeaderFactoryAttribute factoryAnnotation = (AmqpContentHeaderFactoryAttribute)
		Attribute(method, typeof(AmqpContentHeaderFactoryAttribute));
	    string contentClass = factoryAnnotation.m_contentClass;
	    EmitModelMethodPreamble(method);
	    EmitLine("    {");
	    EmitLine("      return new "+MangleClass(contentClass)+"Properties();");
	    EmitLine("    }");
	}

        public void MaybeEmitModelMethod(MethodInfo method) {
            if (method.IsSpecialName) {
                // It's some kind of event- or property-related method.
                // It shouldn't be autogenerated.
            } else if (Attribute(method, typeof(AmqpMethodDoNotImplementAttribute)) != null) {
                // Skip this method, by request (AmqpMethodDoNotImplement)
	    } else if (Attribute(method, typeof(AmqpContentHeaderFactoryAttribute)) != null) {
		EmitContentHeaderFactory(method);
            } else if (Attribute(method, typeof(AmqpUnsupportedAttribute)) != null) {
                EmitModelMethodPreamble(method);
                EmitLine("    {");
                EmitLine("      throw new UnsupportedMethodException(\""+method.Name+"\");");
                EmitLine("    }");
            } else {
                EmitModelMethod(method);
            }
        }

        public string SanitisedFullName(Type t) {
            if (t == typeof(void)) {
                return "void";
            } else {
                return t.FullName;
            }
        }

        public void EmitModelMethodPreamble(MethodInfo method) {
            Emit("    public override "+SanitisedFullName(method.ReturnType)+" "+method.Name);
            ParameterInfo[] parameters = method.GetParameters();
            int remaining = parameters.Length;
            if (remaining == 0) {
                EmitLine("()");
            } else {
                EmitLine("(");
                foreach (ParameterInfo pi in parameters) {
                    Emit("      "+SanitisedFullName(pi.ParameterType)+" @"+pi.Name);
                    remaining--;
                    if (remaining > 0) {
                        EmitLine(",");
                    } else {
                        EmitLine(")");
                    }
                }
            }
        }

        public void LookupAmqpMethod(MethodInfo method,
                                     string methodName,
                                     out AmqpClass amqpClass,
                                     out AmqpMethod amqpMethod)
        {
            amqpClass = null;
            amqpMethod = null;

            // First, try autodetecting the class/method via the
            // IModel method name.

            foreach (AmqpClass c in classes) {
                foreach (AmqpMethod m in c.Methods) {
                    if (methodName.Equals(MangleMethodClass(c,m))) {
                        amqpClass = c;
                        amqpMethod = m;
                        goto stopSearching; // wheee
                    }
                }
            }
            stopSearching:

            // If an explicit mapping was provided as an attribute,
            // then use that instead, whether the autodetect worked or
            // not.

            {
                AmqpMethodMappingAttribute methodMapping =
                    Attribute(method, typeof(AmqpMethodMappingAttribute)) as AmqpMethodMappingAttribute;
                if (methodMapping != null) {
                    amqpClass = null;
                    foreach (AmqpClass c in classes) {
                        if (c.Name == methodMapping.m_className) {
                            amqpClass = c;
                            break;
                        }
                    }
                    amqpMethod = amqpClass.MethodNamed(methodMapping.m_methodName);
                }
            }

            // At this point, if can't find either the class or the
            // method, we can't proceed. Complain.

            if (amqpClass == null || amqpMethod == null) {
                throw new Exception("Could not find AMQP class or method for IModel method " + method.Name);
            }
        }

        public void EmitModelMethod(MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();

            AmqpClass amqpClass = null;
            AmqpMethod amqpMethod = null;
            LookupAmqpMethod(method, method.Name, out amqpClass, out amqpMethod);

            string requestImplClass = MangleMethodClass(amqpClass, amqpMethod);

            // At this point, we know which request method to
            // send. Now compute whether it's an RPC or not.

            AmqpMethod amqpReplyMethod = null;
            AmqpMethodMappingAttribute replyMapping =
                Attribute(method.ReturnTypeCustomAttributes, typeof(AmqpMethodMappingAttribute))
                as AmqpMethodMappingAttribute;
            if (Attribute(method, typeof(AmqpForceOneWayAttribute)) == null &&
                (amqpMethod.IsSimpleRpcRequest || replyMapping != null))
            {
                // We're not forcing oneway, and either are a simple
                // RPC request, or have an explicit replyMapping
                amqpReplyMethod = amqpClass.MethodNamed(replyMapping == null
                                                        ? (string) amqpMethod.ResponseMethods[0]
                                                        : replyMapping.m_methodName);
                if (amqpReplyMethod == null) {
                    throw new Exception("Could not find AMQP reply method for IModel method " + method.Name);
                }
            }

            // If amqpReplyMethod is null at this point, it's a
            // one-way operation, and no continuation needs to be
            // consed up. Otherwise, we should expect a reply of kind
            // identified by amqpReplyMethod - unless there's a nowait
            // parameter thrown into the equation!
            //
            // Examine the parameters to discover which might be
            // nowait, content header or content body.

            ParameterInfo nowaitParameter = null;
            string nowaitExpression = "null";
            ParameterInfo contentHeaderParameter = null;
            ParameterInfo contentBodyParameter = null;
            foreach (ParameterInfo pi in parameters) {
                AmqpNowaitArgumentAttribute nwAttr =
                    Attribute(pi, typeof(AmqpNowaitArgumentAttribute)) as AmqpNowaitArgumentAttribute;
                if (nwAttr != null) {
                    nowaitParameter = pi;
                    if (nwAttr.m_replacementExpression != null) {
                        nowaitExpression = nwAttr.m_replacementExpression;
                    }
                }
                if (Attribute(pi, typeof(AmqpContentHeaderMappingAttribute)) != null) {
                    contentHeaderParameter = pi;
                }
                if (Attribute(pi, typeof(AmqpContentBodyMappingAttribute)) != null) {
                    contentBodyParameter = pi;
                }
            }

            // Compute expression text for the content header and body.

            string contentHeaderExpr =
                contentHeaderParameter == null
                ? "null"
                : " ("+MangleClass(amqpClass.Name)+"Properties) "+contentHeaderParameter.Name;
            string contentBodyExpr =
                contentBodyParameter == null ? "null" : contentBodyParameter.Name;

            // Emit the method declaration and preamble.

            EmitModelMethodPreamble(method);
            EmitLine("    {");

            // Emit the code to build the request.

            EmitLine("      "+requestImplClass+" __req = new "+requestImplClass+"();");
            foreach (ParameterInfo pi in parameters) {
                if (pi != contentHeaderParameter &&
                    pi != contentBodyParameter)
                {
                    if (Attribute(pi, typeof(AmqpUnsupportedAttribute)) != null) {
                        EmitLine("      if (@"+pi.Name+" != null) {");
                        EmitLine("        throw new UnsupportedMethodFieldException(\""+method.Name+"\",\""+pi.Name+"\");");
                        EmitLine("      }");
                    } else {
                        AmqpFieldMappingAttribute fieldMapping =
                            Attribute(pi, typeof(AmqpFieldMappingAttribute)) as AmqpFieldMappingAttribute;
                        if (fieldMapping != null) {
                            EmitLine("      __req.m_"+fieldMapping.m_fieldName+" = @" + pi.Name + ";");
                        } else {
                            EmitLine("      __req.m_"+pi.Name+" = @" + pi.Name + ";");
                        }
                    }
                }
            }

            // If we have a nowait parameter, sometimes that can turn
            // a ModelRpc call into a ModelSend call.

            if (nowaitParameter != null) {
                EmitLine("      if ("+nowaitParameter.Name+") {");
                EmitLine("        ModelSend(__req,"+contentHeaderExpr+","+contentBodyExpr+");");
                if (method.ReturnType != typeof(void)) {
                    EmitLine("        return "+nowaitExpression+";");
                }
                EmitLine("      }");
            }

            // At this point, perform either a ModelRpc or a
            // ModelSend.

            if (amqpReplyMethod == null) {
                EmitLine("      ModelSend(__req,"+contentHeaderExpr+","+contentBodyExpr+");");
            } else {
                string replyImplClass = MangleMethodClass(amqpClass, amqpReplyMethod);

                EmitLine("      RabbitMQ.Client.Impl.MethodBase __repBase = ModelRpc(__req,"+contentHeaderExpr+","+contentBodyExpr+");");
                EmitLine("      "+replyImplClass+" __rep = __repBase as "+replyImplClass+";");
                EmitLine("      if (__rep == null) throw new UnexpectedMethodException(__repBase);");

                if (method.ReturnType == typeof(void)) {
                    // No need to further examine the reply.
                } else {
                    // At this point, we have the reply method. Extract values from it.

                    AmqpFieldMappingAttribute returnMapping =
                        Attribute(method.ReturnTypeCustomAttributes, typeof(AmqpFieldMappingAttribute))
                        as AmqpFieldMappingAttribute;
                    if (returnMapping == null) {
                        // No field mapping --> it's assumed to be a struct to fill in.
                        EmitLine("      "+method.ReturnType+" __result = new "+method.ReturnType+"();");
                        foreach (FieldInfo fi in method.ReturnType.GetFields()) {
                            AmqpFieldMappingAttribute returnFieldMapping =
                                Attribute(fi, typeof(AmqpFieldMappingAttribute)) as AmqpFieldMappingAttribute;
                            if (returnFieldMapping != null) {
                                EmitLine("      __result."+fi.Name+" = __rep.m_"+returnFieldMapping.m_fieldName+";");
                            } else {
                                EmitLine("      __result."+fi.Name+" = __rep.m_"+fi.Name+";");
                            }
                        }
                        EmitLine("      return __result;");
                    } else {
                        // Field mapping --> return just the field we're interested in.
                        EmitLine("      return __rep.m_"+returnMapping.m_fieldName+";");
                    }
                }
            }

            // All the IO and result-extraction has been done. Emit
            // the method postamble.

            EmitLine("    }");
        }

        public void EmitAsynchronousHandlers(ArrayList asynchronousHandlers) {
            EmitLine("    public override bool DispatchAsynchronous(RabbitMQ.Client.Impl.Command cmd) {");
            EmitLine("      RabbitMQ.Client.Impl.MethodBase __method = (RabbitMQ.Client.Impl.MethodBase) cmd.Method;");
            EmitLine("      switch ((__method.ProtocolClassId << 16) | __method.ProtocolMethodId) {");
            foreach (MethodInfo method in asynchronousHandlers) {
                string methodName = method.Name;
                if (methodName.StartsWith("Handle")) {
                    methodName = methodName.Substring(6);
                }

                AmqpClass amqpClass = null;
                AmqpMethod amqpMethod = null;
                LookupAmqpMethod(method, methodName, out amqpClass, out amqpMethod);

                string implClass = MangleMethodClass(amqpClass, amqpMethod);

                EmitLine("        case "+((amqpClass.Index << 16) | amqpMethod.Index)+": {");
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0) {
		    EmitLine("          "+implClass+" __impl = ("+implClass+") __method;");
                    EmitLine("          "+method.Name+"(");
                    int remaining = parameters.Length;
                    foreach (ParameterInfo pi in parameters) {
                        if (Attribute(pi, typeof(AmqpContentHeaderMappingAttribute)) != null) {
                            Emit("            ("+pi.ParameterType+") cmd.Header");
                        } else if (Attribute(pi, typeof(AmqpContentBodyMappingAttribute)) != null) {
                            Emit("            cmd.Body");
                        } else {
                            AmqpFieldMappingAttribute fieldMapping =
                                Attribute(pi, typeof(AmqpFieldMappingAttribute)) as AmqpFieldMappingAttribute;
                            Emit("            __impl.m_"+(fieldMapping == null
                                                          ? pi.Name
                                                          : fieldMapping.m_fieldName));
                        }
                        remaining--;
                        if (remaining > 0) {
                            EmitLine(",");
                        }
                    }
                    EmitLine(");");
                } else {
                    EmitLine("          "+method.Name+"();");
                }
                EmitLine("          return true;");
                EmitLine("        }");
            }
            EmitLine("        default: return false;");
            EmitLine("      }");
            EmitLine("    }");
        }
    }
}
