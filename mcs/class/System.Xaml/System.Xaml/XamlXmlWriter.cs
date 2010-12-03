//
// Copyright (C) 2010 Novell Inc. http://novell.com
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

// To use this under .NET, compile sources as:
//
//	dmcs -d:DOTNET -r:System.Xaml -debug System.Xaml/XamlXmlWriter.cs System.Xaml/XamlWriterInternalBase.cs System.Xaml/TypeExtensionMethods.cs System.Xaml/XamlWriterStateManager.cs System.Xaml/XamlNameResolver.cs System.Xaml/PrefixLookup.cs System.Xaml/ValueSerializerContext.cs ../../build/common/MonoTODOAttribute.cs Test/System.Xaml/TestedTypes.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;

//
// XamlWriter expects write operations in premised orders.
// The most basic one is:
//
//	[NamespaceDeclaration]* -> StartObject -> [ StartMember -> Value | StartObject ... EndObject -> EndMember ]* -> EndObject
//
// For collections:
//	[NamespaceDeclaration]* -> StartObject -> (members)* -> StartMember XamlLanguage.Items -> [ StartObject ... EndObject ]* -> EndMember -> EndObject
//
// For MarkupExtension with PositionalParameters:
//
//	[NamespaceDeclaration]* -> StartObject -> StartMember XamlLanguage.PositionalParameters -> [Value]* -> EndMember -> ... -> EndObject
//

#if DOTNET
namespace Mono.Xaml
#else
namespace System.Xaml
#endif
{
	public class XamlXmlWriter : XamlWriter
	{
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext)
			: this (stream, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (stream), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext)
			: this (xmlWriter, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
		{
			if (xmlWriter == null)
				throw new ArgumentNullException ("xmlWriter");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			this.w = xmlWriter;
			this.sctx = schemaContext;
			this.settings = settings ?? new XamlXmlWriterSettings ();
			var manager = new XamlWriterStateManager<XamlXmlWriterException, InvalidOperationException> (true);
			intl = new XamlXmlWriterInternal (xmlWriter, sctx, manager);
		}

		XmlWriter w;
		XamlSchemaContext sctx;
		XamlXmlWriterSettings settings;

		XamlXmlWriterInternal intl;

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public XamlXmlWriterSettings Settings {
			get { return settings; }
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			intl.CloseAll ();

			if (settings.CloseOutput)
				w.Close ();
		}

		public void Flush ()
		{
			w.Flush ();
		}

		public override void WriteGetObject ()
		{
			intl.WriteGetObject ();
		}

		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			intl.WriteNamespace (namespaceDeclaration);
		}

		public override void WriteStartObject (XamlType xamlType)
		{
			intl.WriteStartObject (xamlType);
		}
		
		public override void WriteValue (object value)
		{
			intl.WriteValue (value);
		}
		
		public override void WriteStartMember (XamlMember property)
		{
			intl.WriteStartMember (property);
		}
		
		public override void WriteEndObject ()
		{
			intl.WriteEndObject ();
		}

		public override void WriteEndMember ()
		{
			intl.WriteEndMember ();
		}
	}
	
	// specific implementation
	class XamlXmlWriterInternal : XamlWriterInternalBase
	{
		const string Xmlns2000Namespace = "http://www.w3.org/2000/xmlns/";

		public XamlXmlWriterInternal (XmlWriter w, XamlSchemaContext schemaContext, XamlWriterStateManager manager)
			: base (schemaContext, manager)
		{
			this.w = w;
			this.sctx = schemaContext;
		}
		
		XmlWriter w;
		XamlSchemaContext sctx;
		
		// Here's a complication.
		// - local_nss holds namespace declarations that are written *before* current element.
		// - local_nss2 holds namespace declarations that are wrtten *after* current element.
		//   (current element == StartObject or StartMember)
		// - When the next element or content is being written, local_nss items are written *within* current element, BUT after all attribute members are written. Hence I had to preserve all those nsdecls at such late.
		// - When current *start* element is closed, then copy local_nss2 items into local_nss.
		// - When there was no children i.e. end element immediately occurs, local_nss should be written at this stage too, and local_nss2 are *ignored*.
		List<NamespaceDeclaration> local_nss = new List<NamespaceDeclaration> ();
		List<NamespaceDeclaration> local_nss2 = new List<NamespaceDeclaration> ();
		bool inside_toplevel_positional_parameter;
		bool inside_attribute_object;

		protected override void OnWriteEndObject ()
		{
			WritePendingStartMember (XamlNodeType.EndObject);

			var state = object_states.Count > 0 ? object_states.Peek () : null;
			if (state != null && state.IsGetObject) {
				// do nothing
				state.IsGetObject = false;
			} else if (w.WriteState == WriteState.Attribute) {
				w.WriteString ("}");
				inside_attribute_object = false;
			} else {
				WritePendingNamespaces ();
				w.WriteEndElement ();
			}
		}

		protected override void OnWriteEndMember ()
		{
			WritePendingStartMember (XamlNodeType.EndMember);

			var member = CurrentMember;
			if (member == XamlLanguage.Initialization)
				return;
			if (member == XamlLanguage.Items)
				return;
			if (member.Type.IsCollection && member.IsReadOnly)
				return;
			if (member.DeclaringType != null && member == member.DeclaringType.ContentProperty)
				return;

			if (inside_toplevel_positional_parameter) {
				w.WriteEndAttribute ();
				inside_toplevel_positional_parameter = false;
			} else if (inside_attribute_object) {
				// do nothing. It didn't open this attribute.
			} else {
				switch (CurrentMemberState.OccuredAs) {
				case AllowedMemberLocations.Attribute:
					w.WriteEndAttribute ();
					break;
				case AllowedMemberLocations.MemberElement:
					WritePendingNamespaces ();
					w.WriteEndElement ();
					break;
				// case (AllowedMemberLocations) 0xFF:
				//	do nothing
				}
			}
		}
		
		protected override void OnWriteStartObject ()
		{
			var tmp = object_states.Pop ();
			XamlType xamlType = tmp.Type;

			WritePendingStartMember (XamlNodeType.StartObject);

			string ns = xamlType.PreferredXamlNamespace;
			string prefix = GetPrefix (ns); // null prefix is not rejected...

			if (w.WriteState == WriteState.Attribute) {
				// MarkupExtension
				w.WriteString ("{");
				if (!String.IsNullOrEmpty (prefix)) {
					w.WriteString (prefix);
					w.WriteString (":");
				}
				string name = ns == XamlLanguage.Xaml2006Namespace ? xamlType.GetInternalXmlName () : xamlType.Name;
				w.WriteString (name);
				// space between type and first member (if any).
				if (xamlType.IsMarkupExtension && xamlType.GetSortedConstructorArguments ().GetEnumerator ().MoveNext ())
					w.WriteString (" ");
			} else {
				WritePendingNamespaces ();
				w.WriteStartElement (prefix, xamlType.GetInternalXmlName (), xamlType.PreferredXamlNamespace);
				var l = xamlType.TypeArguments;
				if (l != null) {
					w.WriteStartAttribute ("x", "TypeArguments", XamlLanguage.Xaml2006Namespace);
					for (int i = 0; i < l.Count; i++) {
						if (i > 0)
							w.WriteString (", ");
						w.WriteString (new XamlTypeName (l [i]).ToString (prefix_lookup));
					}
					w.WriteEndAttribute ();
				}
			}

			object_states.Push (tmp);
		}

		protected override void OnWriteGetObject ()
		{
			if (object_states.Count > 1) {
				var state = object_states.Pop ();

				if (!CurrentMember.Type.IsCollection)
					throw new InvalidOperationException (String.Format ("WriteGetObject method can be invoked only when current member '{0}' is of collection type", CurrentMember));

				object_states.Push (state);
			}

			WritePendingStartMember (XamlNodeType.GetObject);
		}
		
		void WritePendingStartMember (XamlNodeType nodeType)
		{
			var cm = CurrentMemberState;
			if (cm == null || cm.OccuredAs != AllowedMemberLocations.Any)
				return;

			var state = object_states.Peek ();
			if (nodeType == XamlNodeType.Value)
				OnWriteStartMemberAttribute (state.Type, CurrentMember);
			else
				OnWriteStartMemberElement (state.Type, CurrentMember);
		}
		
		protected override void OnWriteStartMember (XamlMember member)
		{
			if (member == XamlLanguage.Initialization)
				return;
			if (member == XamlLanguage.Items)
				return;
			if (member.Type.IsCollection && member.IsReadOnly)
				return;
			if (member.DeclaringType != null && member == member.DeclaringType.ContentProperty)
				return;

			var state = object_states.Peek ();
			
			// Top-level positional parameters are somehow special.
			// - If it has only one parameter, it is written as an
			//   attribute using the actual argument's member name.
			// - If there are more than one, then it is an error at
			//   the second constructor argument.
			// (Here "top-level" means an object that involves
			//  StartObject i.e. the root or a collection item.)
			var posprms = member == XamlLanguage.PositionalParameters && IsAtTopLevelObject () && object_states.Peek ().Type.HasPositionalParameters (service_provider) ? state.Type.GetSortedConstructorArguments ().GetEnumerator () : null;
			if (posprms != null) {
				posprms.MoveNext ();
				var arg = posprms.Current;
				w.WriteStartAttribute (arg.Name);
				inside_toplevel_positional_parameter = true;
			}
			else if (w.WriteState == WriteState.Attribute)
				inside_attribute_object = true;

			if (w.WriteState == WriteState.Attribute) {
				if (state.PositionalParameterIndex < 0) {
					w.WriteString (" ");
					w.WriteString (member.Name);
					w.WriteString ("=");
				}
			}
			else if (member == XamlLanguage.PositionalParameters && posprms == null && state.Type.GetSortedConstructorArguments ().All (m => m == state.Type.ContentProperty)) // PositionalParameters and ContentProperty, excluding such cases that it is already processed above (as attribute).
				OnWriteStartMemberContent (state.Type, member);
			else {
				switch (IsAttribute (state.Type, member)) {
				case AllowedMemberLocations.Attribute:
					OnWriteStartMemberAttribute (state.Type, member);
					break;
				case AllowedMemberLocations.MemberElement:
					OnWriteStartMemberElement (state.Type, member);
					break;
				default: // otherwise - pending output
					CurrentMemberState.OccuredAs = AllowedMemberLocations.Any; // differentiate from .None
					break;
				}
			}
		}

		bool IsAtTopLevelObject ()
		{
			if (object_states.Count == 1)
				return true;
			var tmp = object_states.Pop ();
			var parentMember = object_states.Peek ().WrittenProperties.LastOrDefault ().Member;
			object_states.Push (tmp);

			return parentMember == XamlLanguage.Items;
		}

		AllowedMemberLocations IsAttribute (XamlType ownerType, XamlMember xm)
		{
			var xt = ownerType;
			var mt = xm.Type;
			if (xm == XamlLanguage.Key) {
				var tmp = object_states.Pop ();
				mt = object_states.Peek ().Type.KeyType;
				object_states.Push (tmp);
			}

			if (xm == XamlLanguage.Initialization)
				return AllowedMemberLocations.MemberElement;
			if (mt.HasPositionalParameters (service_provider))
				return AllowedMemberLocations.Attribute;
			if (w.WriteState == WriteState.Content)
				return AllowedMemberLocations.MemberElement;
			if (xt.IsDictionary && xm != XamlLanguage.Key)
				return AllowedMemberLocations.MemberElement; // as each item holds a key.

			var xd = xm as XamlDirective;
			if (xd != null && (xd.AllowedLocation & AllowedMemberLocations.Attribute) == 0)
				return AllowedMemberLocations.MemberElement;

			// surprisingly, WriteNamespace() can affect this.
			if (local_nss2.Count > 0)
				return AllowedMemberLocations.MemberElement;

			// Somehow such a "stranger" is processed as an element.
			if (xd == null && !xt.GetAllMembers ().Contains (xm))
				return AllowedMemberLocations.None;

			if (xm.IsContentValue (service_provider) || mt.IsContentValue (service_provider))
				return AllowedMemberLocations.Attribute;

			return AllowedMemberLocations.MemberElement;
		}

		void OnWriteStartMemberElement (XamlType xt, XamlMember xm)
		{
			CurrentMemberState.OccuredAs = AllowedMemberLocations.MemberElement;
			string prefix = GetPrefix (xm.PreferredXamlNamespace);
			string name = xm.IsDirective ? xm.Name : String.Concat (xt.GetInternalXmlName (), ".", xm.Name);
			WritePendingNamespaces ();
			w.WriteStartElement (prefix, name, xm.PreferredXamlNamespace);
		}
		
		void OnWriteStartMemberAttribute (XamlType xt, XamlMember xm)
		{
			CurrentMemberState.OccuredAs = AllowedMemberLocations.Attribute;
			if (xt.PreferredXamlNamespace == xm.PreferredXamlNamespace &&
			    !(xm is XamlDirective)) // e.g. x:Key inside x:Int should not be written as Key.
				w.WriteStartAttribute (xm.Name);
			else {
				string prefix = GetPrefix (xm.PreferredXamlNamespace);
				w.WriteStartAttribute (prefix, xm.Name, xm.PreferredXamlNamespace);
			}
		}

		void OnWriteStartMemberContent (XamlType xt, XamlMember member)
		{
			// FIXME: well, it is sorta nasty, would be better to define different enum.
			CurrentMemberState.OccuredAs = (AllowedMemberLocations) 0xFF;
		}

		protected override void OnWriteValue (object value)
		{
			if (value != null && !(value is string))
				throw new ArgumentException ("Non-string value cannot be written.");

			XamlMember xm = CurrentMember;
			WritePendingStartMember (XamlNodeType.Value);

			if (w.WriteState != WriteState.Attribute)
				WritePendingNamespaces ();

			string s = GetValueString (xm, value);

			var state = object_states.Peek ();
			switch (state.PositionalParameterIndex) {
			case -1:
				break;
			case 0:
				state.PositionalParameterIndex++;
				break;
			default:
				if (inside_toplevel_positional_parameter)
					throw new XamlXmlWriterException (String.Format ("The XAML reader input has more than one positional parameter values within a top-level object {0} because it tries to write all of the argument values as an attribute value of the first argument. While XamlObjectReader can read such an object, XamlXmlWriter cannot write such an object to XML.", state.Type));

				state.PositionalParameterIndex++;
				w.WriteString (", ");
				break;
			}
			w.WriteString (s);
		}

		protected override void OnWriteNamespace (NamespaceDeclaration nd)
		{
			local_nss2.Add (nd);
		}
		
		void WritePendingNamespaces ()
		{
			foreach (var nd in local_nss) {
				if (String.IsNullOrEmpty (nd.Prefix))
					w.WriteAttributeString ("xmlns", nd.Namespace);
				else
					w.WriteAttributeString ("xmlns", nd.Prefix, Xmlns2000Namespace, nd.Namespace);
			}
			local_nss.Clear ();

			local_nss.AddRange (local_nss2);
			local_nss2.Clear ();
		}
	}

#if DOTNET
	internal static class TypeExtensionMethods2
	{
		static TypeExtensionMethods2 ()
		{
			SpecialNames = new SpecialTypeNameList ();
		}

		public static string GetInternalXmlName (this XamlType type)
		{
			if (type.IsMarkupExtension && type.Name.EndsWith ("Extension", StringComparison.Ordinal))
				return type.Name.Substring (0, type.Name.Length - 9);
			var stn = SpecialNames.FirstOrDefault (s => s.Type == type);
			return stn != null ? stn.Name : type.Name;
		}

		// FIXME: I'm not sure if these "special names" should be resolved like this. I couldn't find any rule so far.
		internal static readonly SpecialTypeNameList SpecialNames;

		internal class SpecialTypeNameList : List<SpecialTypeName>
		{
			internal SpecialTypeNameList ()
			{
				Add (new SpecialTypeName ("Member", XamlLanguage.Member));
				Add (new SpecialTypeName ("Property", XamlLanguage.Property));
			}

			public XamlType Find (string name, string ns)
			{
				if (ns != XamlLanguage.Xaml2006Namespace)
					return null;
				var stn = this.FirstOrDefault (s => s.Name == name);
				return stn != null ? stn.Type : null;
			}
		}

		internal class SpecialTypeName
		{
			public SpecialTypeName (string name, XamlType type)
			{
				Name = name;
				Type = type;
			}
			
			public string Name { get; private set; }
			public XamlType Type { get; private set; }
		}
	}
#endif
}
