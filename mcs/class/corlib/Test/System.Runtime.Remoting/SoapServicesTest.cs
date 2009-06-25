//
// MonoTests.System.Runtime.Remoting.SoapServicesTest.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	[SoapTypeAttribute (XmlElementName="ename", XmlNamespace="ens", XmlTypeName="tname", XmlTypeNamespace="tns")]
	public class SoapTest
	{
		[SoapField(XmlElementName="atrib",XmlNamespace="ns1",UseAttribute=true)]
		public string atribut;

		[SoapField(XmlElementName="elem",XmlNamespace="ns1")]
		public int element;

		[SoapField(XmlElementName="elem2")]
		public int element2;

		[SoapMethod (SoapAction="myaction")]
		public void FesAlgo ()
		{
		}

		public void FesAlgoMes ()
		{
		}

		public void FesAlgoMesEspecial ()
		{
		}
	}

	public class SoapTest1
	{
	}

	[SoapTypeAttribute (XmlElementName="ename", XmlTypeName="tname")]
	public class SoapTest2
	{
	}

	[SoapTypeAttribute (XmlNamespace="ens", XmlTypeNamespace="tns")]
	public class SoapTest3
	{
	}

	[TestFixture]
	public class SoapServicesTest
	{
		public string ThisNamespace
		{
			get
			{
				string tn = "http://schemas.microsoft.com/clr/nsassem/";
				tn += GetType ().Namespace + "/" + GetType ().Assembly.GetName().Name;
				return tn;
			}
		}

		public string GetClassNs (Type t)
		{
			string tn = "http://schemas.microsoft.com/clr/nsassem/";
			tn += t.FullName + "/" + t.Assembly.GetName().Name;
			return tn;
		}

		public string GetSimpleTypeName (Type t)
		{
			return t.FullName + ", " + t.Assembly.GetName().Name;
		}

		[Test]
		public void TestGetXmlType ()
		{
			bool res;
			string name, ns;

			// XmlType

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest), out name, out ns);
			Assert.IsTrue (res, "E1");
			Assert.AreEqual ("ename", name, "E2");
			Assert.AreEqual ("ens", ns, "E3");

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest1), out name, out ns);
			Assert.IsTrue (!res, "E4");

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest2), out name, out ns);
			Assert.IsTrue (res, "E5");
			Assert.AreEqual ("ename", name, "E6");
			Assert.AreEqual (ThisNamespace, ns, "E7");

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest3), out name, out ns);
			Assert.IsTrue (res, "E8");
			Assert.AreEqual ("SoapTest3", name, "E9");
			Assert.AreEqual ("ens", ns, "E10");

			// XmlElement

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest), out name, out ns);
			Assert.IsTrue (res, "T1");
			Assert.AreEqual ("tname", name, "T2");
			Assert.AreEqual ("tns", ns, "T3");

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest1), out name, out ns);
			Assert.IsTrue (!res, "T4");

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest2), out name, out ns);
			Assert.IsTrue (res, "T5");
			Assert.AreEqual ("tname", name, "T6");
			Assert.AreEqual (ThisNamespace, ns, "T7");

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest3), out name, out ns);
			Assert.IsTrue (res, "T8");
			Assert.AreEqual ("SoapTest3", name, "T9");
			Assert.AreEqual ("tns", ns, "T10");
		}

		[Test]
		public void TestGetInteropType ()
		{
			Type t;

			// Manual registration

			t = SoapServices.GetInteropTypeFromXmlElement ("aa","bb");
			Assert.AreEqual (t, null, "M1");

			SoapServices.RegisterInteropXmlElement ("aa","bb",typeof(SoapTest));
			t = SoapServices.GetInteropTypeFromXmlElement ("aa","bb");
			Assert.AreEqual (typeof (SoapTest), t, "M2");


			t = SoapServices.GetInteropTypeFromXmlType ("aa","bb");
			Assert.AreEqual (null, t, "M3");

			SoapServices.RegisterInteropXmlType ("aa","bb",typeof(SoapTest));
			t = SoapServices.GetInteropTypeFromXmlType ("aa","bb");
			Assert.AreEqual (typeof (SoapTest), t, "M4");

			// Preload type

			SoapServices.PreLoad (typeof(SoapTest2));

			t = SoapServices.GetInteropTypeFromXmlElement ("ename",ThisNamespace);
			Assert.AreEqual (typeof (SoapTest2), t, "T1");

			t = SoapServices.GetInteropTypeFromXmlType ("tname",ThisNamespace);
			Assert.AreEqual (typeof (SoapTest2), t, "T2");

			// Preload assembly

			SoapServices.PreLoad (typeof(SoapTest).Assembly);

			t = SoapServices.GetInteropTypeFromXmlElement ("SoapTest3","ens");
			Assert.AreEqual (typeof (SoapTest3), t, "A1");

			t = SoapServices.GetInteropTypeFromXmlType ("SoapTest3","tns");
			Assert.AreEqual (typeof (SoapTest3), t, "A2");
			
		}

		[Test]
		public void TestSoapFields ()
		{
			string name;
			Type t;

			SoapServices.GetInteropFieldTypeAndNameFromXmlAttribute (typeof(SoapTest), "atrib", "ns1", out t, out name);
			Assert.AreEqual ("atribut", name, "#1");
			Assert.AreEqual (typeof(string), t, "#2");

			SoapServices.GetInteropFieldTypeAndNameFromXmlElement (typeof(SoapTest), "elem", "ns1", out t, out name);
			Assert.AreEqual ("element", name, "#3");
			Assert.AreEqual (typeof(int), t, "#4");

			SoapServices.GetInteropFieldTypeAndNameFromXmlElement (typeof(SoapTest), "elem2", null, out t, out name);
			Assert.AreEqual ("element2", name, "#5");
			Assert.AreEqual (typeof(int), t, "#6");
		}

		[Test]
		[Category("NotWorking")]
		public void TestSoapActions ()
		{
			string act;
			MethodBase mb;

			mb = typeof(SoapTest).GetMethod ("FesAlgo");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			Assert.AreEqual ("myaction", act, "S1");

			mb = typeof(SoapTest).GetMethod ("FesAlgoMes");
			SoapServices.RegisterSoapActionForMethodBase (mb, "anotheraction");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			Assert.AreEqual ("anotheraction", act, "S2");

			mb = typeof(SoapTest).GetMethod ("FesAlgoMesEspecial");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			Assert.AreEqual (GetClassNs (typeof(SoapTest))+ "#FesAlgoMesEspecial", act, "S3");

			string typeName, methodName;
			bool res;

			res = SoapServices.GetTypeAndMethodNameFromSoapAction ("myaction", out typeName, out methodName);
			Assert.IsTrue (res, "M1");
			Assert.AreEqual (GetSimpleTypeName (typeof(SoapTest)), typeName, "M2");
			Assert.AreEqual ("FesAlgo", methodName, "M3");

			res = SoapServices.GetTypeAndMethodNameFromSoapAction ("anotheraction", out typeName, out methodName);
			Assert.IsTrue (res, "M4");
			Assert.AreEqual (GetSimpleTypeName (typeof(SoapTest)), typeName, "M5");
			Assert.AreEqual ("FesAlgoMes", methodName, "M6");

			res = SoapServices.GetTypeAndMethodNameFromSoapAction (GetClassNs (typeof(SoapTest))+ "#FesAlgoMesEspecial", out typeName, out methodName);
			Assert.IsTrue (res, "M7");
			Assert.AreEqual (GetSimpleTypeName (typeof(SoapTest)), typeName, "M8");
			Assert.AreEqual ("FesAlgoMesEspecial", methodName, "M9");
		}
	}
}
