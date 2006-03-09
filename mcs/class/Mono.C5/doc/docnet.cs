/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

// DocNet.cs
// Author: Antonio Cisternino
// Version: 0.1
// Last update: 5/12/2001
// Modified Jan 2004 by kokholm@itu.dk

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text;
using System.Diagnostics;

namespace DocNet
{
  class DocNet
  {
    private Assembly assembly;

    //private XmlDocument xml;

    private string defaultNamespace;

    private string assemblyName;

    private static C5.HashDictionary<string, string> longtype2short;

    private static C5.HashDictionary<string, XmlNode> cachedDocComments;

    static DocNet()
    {
      longtype2short = new C5.HashDictionary<string, string>();
      cachedDocComments = new C5.HashDictionary<string, XmlNode>();
      longtype2short.Add("System.Boolean", "bool");
      longtype2short.Add("System.Byte", "byte");
      longtype2short.Add("System.Int32", "int");
      longtype2short.Add("System.Double", "double");
      longtype2short.Add("System.Void", "void");
      longtype2short.Add("System.Object", "object");
      longtype2short.Add("System.String", "string");
      longtype2short.Add("System.Collections.Generic.IEnumerable{T}", "IEnumerable{T}");
      longtype2short.Add("System.Collections.Generic.IEnumerable{U}", "IEnumerable{U}");
      //longtype2short.Add("", "");
    }


    DocNet(string a, string x, string defaultNamespace)
    {
      this.defaultNamespace = defaultNamespace;
      assembly = Assembly.LoadFrom(a, null);
      XmlDocument xml = new XmlDocument();
      xml.Load(x);
      assemblyName = xml.SelectSingleNode("doc/assembly/name").InnerXml;

      if (!assembly.FullName.StartsWith(assemblyName + ","))
        throw new Exception("Wrong assembly specified!\n>> " + assembly.FullName + "\n>> " + assemblyName);

      foreach (XmlNode node in xml.SelectNodes("doc/members/member"))
        cachedDocComments.Add(node.SelectNodes("@name").Item(0).Value, node);
    }


    private void CopyCodeDoc(XmlElement p, string xpath, XmlDocument ret)
    {
      XmlNode n;
      //xml.SelectSingleNode(xpath);

      if (cachedDocComments.Find(xpath, out n))
      {
        foreach (XmlNode child in n.ChildNodes)
          p.AppendChild(ret.ImportNode(child, true));
      }
      //else
      //  Console.Error.WriteLine("docNet: {0} not found", xpath);
    }

    string xmlClean(string s)
    {
//            return s.Replace("&", "&amp;").Replace("{", "&lt;").Replace("}", "&gt;").Replace("<", "&lt;").Replace(">", "&gt;");
      return s.Replace("{", "<").Replace("}", ">");
    }

    private void AddSignature(XmlElement p, string signtext, XmlDocument ret)
    {
      XmlElement sign = CreateElement(ret, "Signature");

      try
      {
        sign.InnerXml = signtext.Replace("&", "&amp;").Replace("{", "&lt;").Replace("}", "&gt;").Replace("<", "&lt;").Replace(">", "&gt;");
      }
      catch (XmlException)
      {
        Console.Error.WriteLine(signtext);
      }
      p.AppendChild(sign);
    }

    private void addImplements(XmlElement p, Type t, XmlDocument ret)
    {
      foreach (Type ty in t.GetInterfaces())
      {
        XmlElement impl = CreateElement(ret, "Implements");

        if (ty.Assembly == assembly)
        {
          impl.SetAttribute("refid", "T:" + canonicalTypeName(ty, null));
          impl.SetAttribute("C5", "");
        }
        AddSignature(impl, prettyTypeName(ty), ret);
        p.AppendChild(impl);
      }
    }

    private void addBases(XmlElement p, Type t, XmlDocument ret)
    {
      Type ty = t.BaseType;

      while (ty != null)
      {
        XmlElement @base = CreateElement(ret, "Bases");

        if (ty.Assembly == assembly)
        {
          @base.SetAttribute("refid", "T:" + canonicalTypeName(ty, null));
          @base.SetAttribute("C5", "");
        }

        AddSignature(@base, prettyTypeName(ty), ret);
        p.PrependChild(@base);
        ty = ty.BaseType;
      }
    }



    private XmlElement CreateElement(XmlDocument ret, string name)
    {
      return ret.CreateElement(null, name, null);
    }

    private void VisitField(bool inherited, FieldInfo f, XmlElement type, XmlDocument refman)
    {
      if (f.Name.Equals("value__"))
        return;
      string refid = "F:" + canonicalTypeName(f.DeclaringType, null) + "." + f.Name;
      //string xpath = "doc/members/member[@name = \"" + refid + "\"]";
      XmlElement el = CreateElement(refman, "Field");

      el.SetAttribute("Name", f.Name);
      el.SetAttribute("refid", refid);
      el.SetAttribute("Static", f.IsStatic.ToString());
      el.SetAttribute("Declared", xmlClean(prettyTypeName(f.DeclaringType)));
      el.SetAttribute("CDeclared", canonicalTypeName(f.DeclaringType, null));
      el.SetAttribute("Type", xmlClean(prettyTypeName(f.FieldType)));
      el.SetAttribute("Access", f.IsPublic ? "public" : (f.IsPrivate || f.IsAssembly ? "private" : "protected"));
      if (f.DeclaringType.Assembly == assembly)
        el.SetAttribute("C5", "");

      if (inherited)
        el.SetAttribute("Inherited", "");

      AddSignature(el, /*prettyTypeName(f.FieldType) + " " +*/ f.Name, refman);
      CopyCodeDoc(el, refid, refman);
      //AddSummary(el, xpath + "/summary", ret, doc);
      type.AppendChild(el);
    }

    private void VisitEvent(bool inherited, EventInfo e, XmlElement type, XmlDocument ret)
    {
      string refid = "E:" + canonicalTypeName(e.DeclaringType, null) + "." + e.Name;
      //string xpath = "doc/members/member[@name = \"" + refid + "\"]";
      XmlElement el = CreateElement(ret, "Event");

      el.SetAttribute("Name", e.Name);
      el.SetAttribute("refid", refid);
      //el.SetAttribute("Static", f.IsStatic.ToString());
      //TODO: check virtual and final values on adders/removers
      //el.SetAttribute("Virtual", e..IsVirtual.ToString());
      //el.SetAttribute("Final", e.IsFinal.ToString());
      el.SetAttribute("Declared", xmlClean(prettyTypeName(e.DeclaringType)));
      el.SetAttribute("CDeclared", canonicalTypeName(e.DeclaringType, null));
      el.SetAttribute("Type", xmlClean(prettyTypeName(e.EventHandlerType)));
      MethodInfo addMethod = e.GetAddMethod(true);
      el.SetAttribute("Access", addMethod.IsPublic ? "public" : addMethod.IsFamily ? "protected" : "private");//NBNBNB! e.IsPublic ? "public" : (e.IsPrivate || e.IsAssembly ? "private" : "protected"));
      if (e.DeclaringType.Assembly == assembly)
        el.SetAttribute("C5", "");

      if (inherited)
        el.SetAttribute("Inherited", "");

      AddSignature(el, /*prettyTypeName(e.EventHandlerType) + " " +*/ e.Name, ret);
      CopyCodeDoc(el, refid, ret);
      //AddSummary(el, xpath + "/summary", ret, doc);
      type.AppendChild(el);
    }


    private void VisitProperty(bool inherited, PropertyInfo p, XmlElement type, XmlDocument ret)
    {
      string refid = "P:" + canonicalPropertyName(p);
      string xpath = "doc/members/member[@name = \"" + refid + "\"]";
      XmlElement el = CreateElement(ret, "Property");

      el.SetAttribute("Name", p.Name);
      el.SetAttribute("refid", refid);
      el.SetAttribute("Access", "public");//TODO: check if reasonable
      MethodInfo m = p.CanRead ? p.GetGetMethod() : p.GetSetMethod();
      if (m != null)
      {
        el.SetAttribute("Static", m.IsStatic.ToString());
        el.SetAttribute("Abstract", m.IsAbstract.ToString());
        el.SetAttribute("Virtual", m.IsVirtual.ToString());
        el.SetAttribute("Final", m.IsFinal.ToString());
      }
      //else
      //Console.Error.WriteLine("%%%%% {0} | {1}", p, p.DeclaringType);
      el.SetAttribute("Declared", xmlClean(prettyTypeName(p.DeclaringType)));
      el.SetAttribute("CDeclared", canonicalTypeName(p.DeclaringType, null));
      el.SetAttribute("Get", p.CanRead.ToString());
      el.SetAttribute("Set", p.CanWrite.ToString());
      el.SetAttribute("Type", xmlClean(prettyTypeName(p.PropertyType)));

      if (p.DeclaringType.Assembly == assembly)
        el.SetAttribute("C5", "");

      if (inherited)
        el.SetAttribute("Inherited", "");

      if (p.Name.Equals("Item"))
        AddSignature(el, prettyIndexerSignature(p), ret);
      else
        AddSignature(el, /*prettyTypeName(p.PropertyType) + " " +*/ p.Name, ret);

      //AddSummary(el, xpath + "/summary", ret, doc);
      CopyCodeDoc(el, refid, ret);
      //AddValue(el, xpath + "/value", ret, doc);
      VisitParameters(p.GetIndexParameters(), el, ret, xpath);
      type.AppendChild(el);
    }


    private void VisitParameters(ParameterInfo[] pars, XmlElement n, XmlDocument ret, string xpath)
    {
      foreach (ParameterInfo p in pars)
      {
        XmlElement el = CreateElement(ret, "Parameter");

        el.SetAttribute("Name", p.Name);
        el.SetAttribute("Type", prettyTypeName(p.ParameterType));
        //AddSummary(el, xpath + "/param[@name = \"" + p.Name + "\"]", ret, doc);
        CopyCodeDoc(el, xpath + "/param[@name = \"" + p.Name + "\"]", ret);

        n.AppendChild(el);
      }
    }


    private void VisitConstructor(Type t, ConstructorInfo c, XmlElement type, XmlDocument ret)
    {
      Type declaringType = c.DeclaringType;
      string refid = "M:" + canonicalTypeName(c.DeclaringType, null) + "." + "#ctor";

      refid += canonicalParameters(c.GetParameters(), new string[]{});

      string xpath = "doc/members/member[@name = \"" + refid + "\"]";
      XmlElement el = CreateElement(ret, "Constructor");
      el.SetAttribute("Foo", c.IsConstructor ? "Con" : "San");
      el.SetAttribute("refid", refid);
      el.SetAttribute("Declared", prettyTypeName(declaringType));
      el.SetAttribute("CDeclared", canonicalTypeName(declaringType, null));
      el.SetAttribute("Access", c.IsPublic ? "public" : (c.IsPrivate ? "private" : "protected"));
      //el.SetAttribute("Access", c.IsPublic ? "public" : (c.IsPrivate || c.IsAssembly ? "private" : "protected"));
      if (declaringType.Assembly == assembly)
        el.SetAttribute("C5", "");
      if (declaringType != t)
        el.SetAttribute("Inherited", "");
      AddSignature(el, prettyConstructorSignature(c), ret);
      CopyCodeDoc(el, refid, ret);
      //AddSummary(el, xpath + "/summary", ret, doc);
      VisitParameters(c.GetParameters(), el, ret, xpath);
      type.AppendChild(el);
    }


    private void VisitMethod(bool inherited, MethodInfo m, XmlElement type, XmlDocument ret)
    {
      if (m.Name.StartsWith("get_") || m.Name.StartsWith("set_") || m.Name.StartsWith("add_") || m.Name.StartsWith("remove_"))
        return;
      bool isOperator = m.Name.StartsWith("op_");

      string refid = "M:" + canonicalMethodName(m);

      string xpath = "doc/members/member[@name = \"" + refid + "\"]";
      XmlElement el = CreateElement(ret, isOperator ? "Operator" : "Method");

      string mangledName = m.Name;
      if (isOperator)
      {
        switch (mangledName)
        {
          case "op_Equality": mangledName = "operator =="; break;
          case "op_Inequality": mangledName = "operator !="; break;
          default: throw new ApplicationException("unknown operatorname, " + mangledName);
        }
      }
      el.SetAttribute("Name", mangledName);
      el.SetAttribute("refid", refid);
      el.SetAttribute("Static", m.IsStatic.ToString());
      el.SetAttribute("Abstract", m.IsAbstract.ToString());
      el.SetAttribute("Virtual", m.IsVirtual.ToString());
      el.SetAttribute("Final", m.IsFinal.ToString());
      el.SetAttribute("Declared", xmlClean(prettyTypeName(m.DeclaringType)));
      el.SetAttribute("CDeclared", canonicalTypeName(m.DeclaringType, null));
      el.SetAttribute("ReturnType", xmlClean(prettyTypeName(m.ReturnType)));
      if (m.DeclaringType.Assembly == assembly)
        el.SetAttribute("C5", "");
      if (inherited)
        el.SetAttribute("Inherited", "");
      el.SetAttribute("Access", m.IsPublic ? "public" : (m.IsPrivate || m.IsAssembly ? "private" : "protected"));
      el.SetAttribute("Sealed", m.IsFinal.ToString());
      AddSignature(el, prettyMethodSignature(mangledName, m), ret);
      CopyCodeDoc(el, refid, ret);
      VisitParameters(m.GetParameters(), el, ret, xpath);

      foreach (Type gp in m.GetGenericArguments())
        foreach (Type gc in gp.GetGenericParameterConstraints())
          if (gc != typeof(object))
          {
            XmlElement constraint = CreateElement(ret, "constraint");
            constraint.SetAttribute("Value", prettyTypeName(gp) + " : " + xmlClean(prettyTypeName(gc)));
            el.AppendChild(constraint);
          }
      type.AppendChild(el);
    }

    public XmlDocument GenerateDoc()
    {
      BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

      XmlDocument ret = new XmlDocument();
      XmlElement root = CreateElement(ret, "Assembly");

      root.SetAttribute("Name", assemblyName);

      ret.AppendChild(root);

      XmlElement type = null;
      //string xpath = null;

      foreach (Type t in assembly.GetTypes())
      {
        if (t.Name.StartsWith("DocNet"))
          continue;

        if (t.IsInterface)
        {
          type = CreateElement(ret, "Interface");
          foreach (EventInfo e in t.GetEvents(flags))
            VisitEvent(e.DeclaringType != t, e, type, ret);

          foreach (PropertyInfo p in t.GetProperties(flags))
            VisitProperty(false, p, type, ret);

          foreach (MethodInfo m in t.GetMethods(flags))
            VisitMethod(false, m, type, ret);
        }
        else if (t.IsValueType)
        {
          type = CreateElement(ret, "Struct");
          foreach (FieldInfo f in t.GetFields(flags))
            VisitField(f.DeclaringType != t, f, type, ret);

          foreach (EventInfo e in t.GetEvents(flags))
            VisitEvent(e.DeclaringType != t, e, type, ret);

          foreach (PropertyInfo p in t.GetProperties(flags))
            VisitProperty(p.DeclaringType != t, p, type, ret);

          foreach (ConstructorInfo c in t.GetConstructors(flags))
            VisitConstructor(t, c, type, ret);

          foreach (MethodInfo m in t.GetMethods(flags))
            VisitMethod(m.DeclaringType != t, m, type, ret);
        }
        else if (t.IsSubclassOf(typeof(Delegate)))
        {
          type = CreateElement(ret, "Delegate");
          VisitMethod(false, t.GetMethod("Invoke"), type, ret);
        }
        else
        { // Class
          type = CreateElement(ret, "Class");
          foreach (FieldInfo f in t.GetFields(flags))
            VisitField(f.DeclaringType != t, f, type, ret);

          foreach (EventInfo e in t.GetEvents(flags))
            VisitEvent(e.DeclaringType != t, e, type, ret);

          foreach (PropertyInfo p in t.GetProperties(flags))
            VisitProperty(p.DeclaringType != t, p, type, ret);

          foreach (ConstructorInfo c in t.GetConstructors(flags))
            VisitConstructor(t, c, type, ret);

          foreach (MethodInfo m in t.GetMethods(flags))
            VisitMethod(m.DeclaringType != t, m, type, ret);
        }

        type.SetAttribute("Name", xmlClean(prettyTypeName(t)));
        type.SetAttribute("Access", t.IsPublic || t.IsNestedPublic ? "public" : t.IsNestedFamily ? "protected" : "private");

        string refid = "T:" + canonicalTypeName(t, null);

        type.SetAttribute("refid", refid);
        type.SetAttribute("C5", "");
        AddSignature(type, prettyTypeName(t), ret);
        addImplements(type, t, ret);
        addBases(type, t, ret);

        foreach (Type gp in t.GetGenericArguments())
        {
          if (gp.GenericParameterAttributes != GenericParameterAttributes.None)
          {
            XmlElement constraint = CreateElement(ret, "constraint");
            string constraintText = null;
            switch (gp.GenericParameterAttributes)
            {
              case GenericParameterAttributes.Contravariant:
                break;
              case GenericParameterAttributes.Covariant:
                break;
              case GenericParameterAttributes.DefaultConstructorConstraint:
                constraintText = "new()";
                break;
              case GenericParameterAttributes.None:
                break;
              case GenericParameterAttributes.ReferenceTypeConstraint:
                constraintText = "class";
                break;
              case GenericParameterAttributes.SpecialConstraintMask:
                break;
              case GenericParameterAttributes.NotNullableValueTypeConstraint:
                constraintText = "struct";
                break;
              case GenericParameterAttributes.VarianceMask:
                break;
            }
            constraint.SetAttribute("Value", String.Format("{0} : {1}", gp, constraintText));
            type.AppendChild(constraint);
          }
          foreach (Type gc in gp.GetGenericParameterConstraints())
          {
            if (gc != typeof(object))
            {
              XmlElement constraint = CreateElement(ret, "constraint");
              constraint.SetAttribute("Value", String.Format("{0} : {1}", prettyTypeName(gp), xmlClean(prettyTypeName(gc))));
              type.AppendChild(constraint);
            }
          }
        }

        CopyCodeDoc(type, refid, ret);
        root.AppendChild(type);
      }

      return ret;
    }

    C5.HashDictionary<Type, string> t2ptn = new C5.HashDictionary<Type, string>();
    private string prettyTypeName(Type t)
    {
      string retval;
      //if (!t2ptn.Find(t, out retval))
      //{
      int consumed = 0;
      retval = prettyTypeName(t, ref consumed);
      //    t2ptn.Add(t, retval);
      //}
      return retval;
    }

    private string prettyTypeName(Type t, ref int consumed)
    {
      StringBuilder ret = new StringBuilder();

      if (t.IsGenericParameter)
        ret.Append(t.Name);
      else if (t.IsArray)
        ret.Append(prettyTypeName(t.GetElementType()) + "[]");
      else if (t.IsByRef)
        ret.Append("ref ").Append(prettyTypeName(t.GetElementType()));
      else if (!t.IsGenericType)
        ret.Append(t.IsNested ? prettyTypeName(t.DeclaringType, ref consumed) + "." + t.Name : t.FullName);
      else
      {
        bool first = true;
        StringBuilder gps = new StringBuilder();
        Type[] gp = t.GetGenericArguments();

        ret.Append(t.IsNested ? prettyTypeName(t.DeclaringType, ref consumed) : t.Namespace).Append(".").Append(t.Name);
        if (consumed < gp.Length)
        {
          //TODO: fix this ugly hack to remove `n 
          ret.Remove(ret.Length - 2, 2);
          //ret = ret.Substring(0, ret.Length - 2);
          for (int i = consumed, length = gp.Length; i < length; i++)
          {
            Type ty = gp[i];

            if (first) first = false;
            else
              gps.Append(",");

            gps.Append(prettyTypeName(ty));
          }

          consumed = gp.Length;
          ret.Append("{").Append(gps.ToString()).Append("}");
        }
      }

      string retval = ret.ToString();

      if (retval.StartsWith(defaultNamespace + "."))
        retval = retval.Substring(defaultNamespace.Length + 1);

      if (longtype2short.Contains(retval))
        retval = longtype2short[retval];

      return retval;
    }

    private string prettyParameters(ParameterInfo[] pars)
    {
      string ret = "";
      bool first = true;

      foreach (ParameterInfo p in pars)
      {
        if (first) first = false;
        else
          ret += ", ";
        Type pt = p.ParameterType;
        if (p.IsOut)
        {
          ret += "out ";
          pt = pt.GetElementType();
        }

        ret += prettyTypeName(pt) + " " + p.Name;
      }

      return ret;
    }

    private string prettyMethodSignature(string name, MethodInfo m)
    {
      string gp = "";
      if (m.IsGenericMethod)
      {
        Type[] gps = m.GetGenericArguments();
        gp = "<";

        for (int i = 0; i < gps.Length; i++)
          gp += (i == 0 ? "" : ",") + gps[i].Name;

        gp += ">";
      }

      return name + gp + "(" + prettyParameters(m.GetParameters()) + ")";
    }

    private string prettyConstructorSignature(ConstructorInfo c)
    {
      Type t = c.DeclaringType;

      return prettyTypeName(t) + "(" + prettyParameters(c.GetParameters()) + ")";
    }

    private string prettyIndexerSignature(PropertyInfo p)
    {
      return /*prettyTypeName(p.PropertyType) + " " + */ "this[" + prettyParameters(p.GetIndexParameters()) + "]";
    }


    private string simpleTypeName(Type t)
    {
      return (t.IsNested ? simpleTypeName(t.DeclaringType) : t.Namespace) + "." + t.Name;
    }


    private string canonicalTypeName(Type t, string[] mgps)
    {
      string ret;

      if (t.IsGenericParameter)
        ret = "`" + t.GenericParameterPosition;
      else if (t.IsArray)
        ret = canonicalTypeName(t.GetElementType(), mgps) + "[]";
      else if (t.IsByRef)
        ret = canonicalTypeName(t.GetElementType(), mgps) + "@";
      else
      {
        ret = simpleTypeName(t);
        if (!t.IsGenericType)
          ret += "";
        else if (mgps == null)
          ret += "";//"`" + t.GetGenericArguments().Length;
        else
        {
          //TODO: fix this ugly hack to remove `n 
          ret = ret.Substring(0, ret.Length - 2);

          bool first = true;
          string gps = "";
          Type[] gp = t.GetGenericArguments();

          foreach (Type ty in gp)
          {
            if (first) first = false;
            else
              gps += ",";

            if (ty.IsGenericParameter)
            {
              bool ismgp = false;

              foreach (string s in mgps) if (s.Equals(ty.Name)) ismgp = true;

              gps += (ismgp ? "``" : "`") + ty.GenericParameterPosition;
            }
            else
              gps += canonicalTypeName(ty, mgps);
          }

          ret += "{" + gps + "}";
        }
      }

      return ret;
    }

    private string canonicalMethodName(MethodInfo m)
    {
      string ret = canonicalTypeName(m.DeclaringType, null) + "." + m.Name;

      string[] gmps;

      if (m.IsGenericMethod)
      {
        Type[] gps = m.GetGenericArguments();

        ret += "``" + gps.Length;
        gmps = new string[gps.Length];
        for (int i = 0; i < gps.Length; i++)
          gmps[i] = gps[i].Name;
      }
      else
        gmps = new string[]{};

      ret += canonicalParameters(m.GetParameters(), gmps);
      return ret;
    }

    private string canonicalPropertyName(PropertyInfo p)
    {
      string pname = canonicalTypeName(p.DeclaringType, null) + "." + p.Name;
      ParameterInfo[] pars = p.GetIndexParameters();

      if (pars.Length > 0)
        pname += canonicalParameters(pars, new string[]{});

      return pname;
    }

    private string canonicalParameters(ParameterInfo[] pars, string[] gmps)
    {
      if (pars.Length == 0) return "";

      string ret = "";
      bool first = true;

      foreach (ParameterInfo p in pars)
      {
        if (first) first = false;
        else
          ret += ",";

        ret += canonicalTypeName(p.ParameterType, gmps); ;
      }

      return "(" + ret + ")";
    }



    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        args = new string[] { @"C5.dll", @"C5.xml" };

      }
      {
        Timer timer = new Timer();
        timer.snap();
        DocNet doc = new DocNet(args[0], args[1], "C5");
        XmlDocument merged = doc.GenerateDoc();
        Console.Error.WriteLine("Time merge: {0} ms", timer.snap());

        System.Xml.Xsl.XslCompiledTransform overview = new System.Xml.Xsl.XslCompiledTransform();
        overview.Load(@"overview.xslt");
        overview.Transform(merged, new XmlTextWriter(new StreamWriter(@"docbuild\contents.htm")));
        Console.Error.WriteLine("Time, overview: {0} ms", timer.snap());

        StringBuilder megaDoc = new StringBuilder();
        using (XmlWriter writer = XmlWriter.Create(megaDoc))
        {
          writer.WriteStartElement("hack");
          System.Xml.Xsl.XslCompiledTransform trans = new System.Xml.Xsl.XslCompiledTransform();
          trans.Load(@"trans.xslt");
          trans.Transform(merged, writer);
          writer.WriteEndElement();
          writer.Close();
        }
        Console.Error.WriteLine("Time trans: {0} ms", timer.snap());
        System.Xml.XPath.XPathDocument megaXml =
          new System.Xml.XPath.XPathDocument(XmlReader.Create(new StringReader(megaDoc.ToString())));
        System.Xml.XPath.XPathNodeIterator nodes = megaXml.CreateNavigator().Select("/hack/*");
        string docfn = null;
        foreach (System.Xml.XPath.XPathNavigator var in nodes)
        {
          if (var.Name == "filestart")
            docfn = var.GetAttribute("name", "");
          if (var.Name == "html")
          {
            Console.Error.Write(".");
            XmlWriter w = new XmlTextWriter(new StreamWriter(@"docbuild\types\" + docfn));
            var.WriteSubtree(w);
            w.Close();
          }
        }
        Console.Error.WriteLine();
        Console.Error.WriteLine("Time split: {0} ms", timer.snap());
      }
      Console.Write("? ");
      Console.Read();
    }
  }
}

