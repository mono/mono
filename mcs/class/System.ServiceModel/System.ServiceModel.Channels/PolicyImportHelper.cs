//
// PolicyImportHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Xml;
using System.Collections.Generic;
using System.ServiceModel.Description;

using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Channels {

	internal static class PolicyImportHelper {

		internal const string SecurityPolicyNS = "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";
		internal const string PolicyNS = "http://schemas.xmlsoap.org/ws/2004/09/policy";
		internal const string MimeSerializationNS = "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization";
		internal const string HttpAuthNS = "http://schemas.microsoft.com/ws/06/2004/policy/http";

		internal const string FramingPolicyNS = "http://schemas.microsoft.com/ws/2006/05/framing/policy";
		internal const string NetBinaryEncodingNS = "http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1";

		internal const string WSSecurityNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

		internal static XmlElement GetTransportBindingPolicy (PolicyAssertionCollection collection)
		{
			return FindAndRemove (collection, "TransportBinding", SecurityPolicyNS);
		}
			
		internal static XmlElement GetStreamedMessageFramingPolicy (PolicyAssertionCollection collection)
		{
			return FindAndRemove (collection, "Streamed", FramingPolicyNS);	
		}

		internal static XmlElement GetBinaryMessageEncodingPolicy (PolicyAssertionCollection collection)
		{
			return FindAndRemove (collection, "BinaryEncoding", NetBinaryEncodingNS);
		}

		internal static XmlElement GetMtomMessageEncodingPolicy (PolicyAssertionCollection collection)
		{
			return FindAndRemove (collection, "OptimizedMimeSerialization", MimeSerializationNS);
		}

		static XmlElement FindAndRemove (PolicyAssertionCollection collection, string name, string ns)
		{
			var element = collection.Find (name, ns);
			if (element != null)
				collection.Remove (element);
			return element;
		}

		internal static List<XmlElement> FindAssertionByNS (
			PolicyAssertionCollection collection, string ns)
		{
			var list = new List<XmlElement> ();
			foreach (var assertion in collection) {
				if (assertion.NamespaceURI.Equals (ns))
					list.Add (assertion);
			}
			return list;
		}

		internal static List<XmlElement> GetPolicyElements (XmlElement root, out bool error)
		{
			XmlElement policy = null;
			var list = new List<XmlElement> ();

			foreach (var node in root.ChildNodes) {
				var e = node as XmlElement;
				if (e == null)
					continue;
				if (!PolicyNS.Equals (e.NamespaceURI) || !e.LocalName.Equals ("Policy")) {
					error = true;
					return list;
				}
				if (policy != null) {
					error = true;
					return list;
				}
				policy = e;
			}

			if (policy == null) {
				error = true;
				return list;
			}

			foreach (var node in policy.ChildNodes) {
				var e = node as XmlElement;
				if (e != null)
					list.Add (e);
			}

			error = false;
			return list;
		}

		internal static bool FindPolicyElement (MetadataImporter importer, XmlElement root,
		                                        QName name, bool required, bool removeWhenFound,
		                                        out XmlElement element)
		{
			if (!FindPolicyElement (root, name, removeWhenFound, out element)) {
				importer.AddWarning ("Invalid policy element: {0}", root.OuterXml);
				return false;
			}
			if (required && (element == null)) {
				importer.AddWarning ("Did not find policy element `{0}'.", name);
				return false;
			}
			return true;
		}

		internal static bool FindPolicyElement (XmlElement root, QName name,
		                                        bool removeWhenFound, out XmlElement element)
		{
			XmlElement policy = null;
			foreach (var node in root.ChildNodes) {
				var e = node as XmlElement;
				if (e == null)
					continue;
				if (!PolicyNS.Equals (e.NamespaceURI) || !e.LocalName.Equals ("Policy")) {
					element = null;
					return false;
				}
				if (policy != null) {
					element = null;
					return false;
				}
				policy = e;
			}

			if (policy == null) {
				element = null;
				return true;
			}

			element = null;
			foreach (var node in policy.ChildNodes) {
				var e = node as XmlElement;
				if (e == null)
					continue;
				if (!name.Namespace.Equals (e.NamespaceURI) || !name.Name.Equals (e.LocalName))
					continue;

				element = e;
				break;
			}

			if (!removeWhenFound || (element == null))
				return true;

			policy.RemoveChild (element);

			bool foundAnother = false;
			foreach (var node in policy.ChildNodes) {
				var e = node as XmlElement;
				if (e != null) {
					foundAnother = true;
					break;
				}
			}

			if (!foundAnother)
				root.RemoveChild (policy);
			return true;
		}

		internal static XmlElement GetElement (MetadataImporter importer,
		                                       XmlElement root, string name, string ns)
		{
			return GetElement (importer, root, name, ns, false);
		}

		internal static XmlElement GetElement (MetadataImporter importer,
		                                       XmlElement root, string name, string ns,
		                                       bool required)
		{
			return GetElement (importer, root, new QName (name, ns), required);
		}

		internal static XmlElement GetElement (MetadataImporter importer,
		                                       XmlElement root, QName name, bool required)
		{
			var list = root.GetElementsByTagName (name.Name, name.Namespace);
			if (list.Count < 1) {
				if (required)
					importer.AddWarning ("Did not find required policy element `{0}'", name);
				return null;
			}

			if (list.Count > 1) {
				importer.AddWarning ("Found duplicate policy element `{0}'", name);
				return null;
			}

			var element = list [0] as XmlElement;
			if (required && (element == null))
				importer.AddWarning ("Did not find required policy element `{0}'", name);
			return element;
		}

		internal static XmlElement WrapPolicy (XmlElement element)
		{
			var policy = element.OwnerDocument.CreateElement ("wsp", "Policy", PolicyNS);
			policy.AppendChild (element);
			return policy;
		}

		//
		// Add a single element, wrapping it inside <wsp:Policy>
		//
		internal static void AddWrappedPolicyElement (XmlElement root, XmlElement element)
		{
			if (root.OwnerDocument != element.OwnerDocument)
				element = (XmlElement)root.OwnerDocument.ImportNode (element, true);
			if (!element.NamespaceURI.Equals (PolicyNS) || !element.LocalName.Equals ("Policy"))
				element = WrapPolicy (element);
			root.AppendChild (element);
		}

		//
		// Add multiple elements, wrapping them inside a single <wsp:Policy>
		//
		internal static void AddWrappedPolicyElements (XmlElement root, params XmlElement[] elements)
		{
			var policy = root.OwnerDocument.CreateElement ("wsp", "Policy", PolicyNS);
			root.AppendChild (policy);

			foreach (var element in elements) {
				XmlElement imported;
				if (root.OwnerDocument != element.OwnerDocument)
					imported = (XmlElement)root.OwnerDocument.ImportNode (element, true);
				else
					imported = element;
				policy.AppendChild (element);
			}
		}
	}
}

