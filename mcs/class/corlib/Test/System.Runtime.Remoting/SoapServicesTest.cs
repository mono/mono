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
	public class SoapServicesTest: Assertion
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
			Assert ("E1",res);
			AssertEquals ("E2", "ename", name);
			AssertEquals ("E3", "ens", ns);

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest1), out name, out ns);
			Assert ("E4",!res);

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest2), out name, out ns);
			Assert ("E5",res);
			AssertEquals ("E6", "ename", name);
			AssertEquals ("E7", ThisNamespace, ns);

			res = SoapServices.GetXmlElementForInteropType (typeof(SoapTest3), out name, out ns);
			Assert ("E8",res);
			AssertEquals ("E9", "SoapTest3", name);
			AssertEquals ("E10", "ens", ns);

			// XmlElement

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest), out name, out ns);
			Assert ("T1",res);
			AssertEquals ("T2", "tname", name);
			AssertEquals ("T3", "tns", ns);

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest1), out name, out ns);
			Assert ("T4",!res);

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest2), out name, out ns);
			Assert ("T5",res);
			AssertEquals ("T6", "tname", name);
			AssertEquals ("T7", ThisNamespace, ns);

			res = SoapServices.GetXmlTypeForInteropType (typeof(SoapTest3), out name, out ns);
			Assert ("T8",res);
			AssertEquals ("T9", "SoapTest3", name);
			AssertEquals ("T10", "tns", ns);
		}

		[Test]
		public void TestGetInteropType ()
		{
			Type t;

			// Manual registration

			t = SoapServices.GetInteropTypeFromXmlElement ("aa","bb");
			AssertEquals ("M1", t, null);

			SoapServices.RegisterInteropXmlElement ("aa","bb",typeof(SoapTest));
			t = SoapServices.GetInteropTypeFromXmlElement ("aa","bb");
			AssertEquals ("M2", typeof (SoapTest), t);


			t = SoapServices.GetInteropTypeFromXmlType ("aa","bb");
			AssertEquals ("M3", null, t);

			SoapServices.RegisterInteropXmlType ("aa","bb",typeof(SoapTest));
			t = SoapServices.GetInteropTypeFromXmlType ("aa","bb");
			AssertEquals ("M4", typeof (SoapTest), t);

			// Preload type

			SoapServices.PreLoad (typeof(SoapTest2));

			t = SoapServices.GetInteropTypeFromXmlElement ("ename",ThisNamespace);
			AssertEquals ("T1", typeof (SoapTest2), t);

			t = SoapServices.GetInteropTypeFromXmlType ("tname",ThisNamespace);
			AssertEquals ("T2", typeof (SoapTest2), t);

			// Preload assembly

			SoapServices.PreLoad (typeof(SoapTest).Assembly);

			t = SoapServices.GetInteropTypeFromXmlElement ("SoapTest3","ens");
			AssertEquals ("A1", typeof (SoapTest3), t);

			t = SoapServices.GetInteropTypeFromXmlType ("SoapTest3","tns");
			AssertEquals ("A2", typeof (SoapTest3), t);
			
		}

		[Test]
		public void TestSoapFields ()
		{
			string name;
			Type t;

			SoapServices.GetInteropFieldTypeAndNameFromXmlAttribute (typeof(SoapTest), "atrib", "ns1", out t, out name);
			AssertEquals ("#1", "atribut", name);
			AssertEquals ("#2", typeof(string), t);

			SoapServices.GetInteropFieldTypeAndNameFromXmlElement (typeof(SoapTest), "elem", "ns1", out t, out name);
			AssertEquals ("#3", "element", name);
			AssertEquals ("#4", typeof(int), t);

			SoapServices.GetInteropFieldTypeAndNameFromXmlElement (typeof(SoapTest), "elem2", null, out t, out name);
			AssertEquals ("#5", "element2", name);
			AssertEquals ("#6", typeof(int), t);
		}

		[Test]
		[Category("NotWorking")]
		public void TestSoapActions ()
		{
			string act;
			MethodBase mb;

			mb = typeof(SoapTest).GetMethod ("FesAlgo");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			AssertEquals ("S1", "myaction", act);

			mb = typeof(SoapTest).GetMethod ("FesAlgoMes");
			SoapServices.RegisterSoapActionForMethodBase (mb, "anotheraction");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			AssertEquals ("S2", "anotheraction", act);

			mb = typeof(SoapTest).GetMethod ("FesAlgoMesEspecial");
			act = SoapServices.GetSoapActionFromMethodBase (mb);
			AssertEquals ("S3", GetClassNs (typeof(SoapTest))+ "#FesAlgoMesEspecial", act);

			string typeName, methodName;
			bool res;

			res = SoapServices.GetTypeAndMethodNameFromSoapAction ("myaction", out typeName, out methodName);
			Assert ("M1", res);
			AssertEquals ("M2", GetSimpleTypeName (typeof(SoapTest)), typeName);
			AssertEquals ("M3", "FesAlgo", methodName);

			res = SoapServices.GetTypeAndMethodNameFromSoapAction ("anotheraction", out typeName, out methodName);
			Assert ("M4", res);
			AssertEquals ("M5", GetSimpleTypeName (typeof(SoapTest)), typeName);
			AssertEquals ("M6", "FesAlgoMes", methodName);

			res = SoapServices.GetTypeAndMethodNameFromSoapAction (GetClassNs (typeof(SoapTest))+ "#FesAlgoMesEspecial", out typeName, out methodName);
			Assert ("M7", res);
			AssertEquals ("M8", GetSimpleTypeName (typeof(SoapTest)), typeName);
			AssertEquals ("M9", "FesAlgoMesEspecial", methodName);
		}
	}
}
