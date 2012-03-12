//
// Copyright (C) 2010 Novell Inc. http://novell.com
// Copyright (C) 2012 Xamarin Inc. http://xamarin.com
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

#if DOTNET
namespace Mono.Xaml
#else
namespace System.Xaml
#endif
{
	internal abstract class XamlWriterInternalBase
	{
		public XamlWriterInternalBase (XamlSchemaContext schemaContext, XamlWriterStateManager manager)
		{
			this.sctx = schemaContext;
			this.manager = manager;
			var p = new PrefixLookup (sctx) { IsCollectingNamespaces = true }; // it does not raise unknown namespace error.
			service_provider = new ValueSerializerContext (p, schemaContext, AmbientProvider);
		}

		XamlSchemaContext sctx;
		XamlWriterStateManager manager;

		internal IValueSerializerContext service_provider;

		internal ObjectState root_state;
		internal Stack<ObjectState> object_states = new Stack<ObjectState> ();
		internal PrefixLookup prefix_lookup {
			get { return (PrefixLookup) service_provider.GetService (typeof (INamespacePrefixLookup)); }
		}

		List<NamespaceDeclaration> namespaces {
			get { return prefix_lookup.Namespaces; }
		}

		internal virtual IAmbientProvider AmbientProvider {
			get { return null; }
		}

		internal class ObjectState
		{
			public XamlType Type;
			public bool IsGetObject;
			public int PositionalParameterIndex = -1;

			public string FactoryMethod;
			public object Value;
			public object KeyValue;
			public List<MemberAndValue> WrittenProperties = new List<MemberAndValue> ();
			public bool IsInstantiated;
			public bool IsXamlWriterCreated; // affects AfterProperties() calls.
		}
		
		internal class MemberAndValue
		{
			public MemberAndValue (XamlMember xm)
			{
				Member = xm;
			}

			public XamlMember Member;
			public object Value;
			public AllowedMemberLocations OccuredAs = AllowedMemberLocations.None;
		}

		public void CloseAll ()
		{
			while (object_states.Count > 0) {
				switch (manager.State) {
				case XamlWriteState.MemberDone:
				case XamlWriteState.ObjectStarted: // StartObject without member
					WriteEndObject ();
					break;
				case XamlWriteState.ValueWritten:
				case XamlWriteState.ObjectWritten:
				case XamlWriteState.MemberStarted: // StartMember without content
					manager.OnClosingItem ();
					WriteEndMember ();
					break;
				default:
					throw new NotImplementedException (manager.State.ToString ()); // there shouldn't be anything though
				}
			}
		}

		internal string GetPrefix (string ns)
		{
			foreach (var nd in namespaces)
				if (nd.Namespace == ns)
					return nd.Prefix;
			return null;
		}

		protected MemberAndValue CurrentMemberState {
			get { return object_states.Count > 0 ? object_states.Peek ().WrittenProperties.LastOrDefault () : null; }
		}

		protected XamlMember CurrentMember {
			get {
				var mv = CurrentMemberState;
				return mv != null ? mv.Member : null;
			}
		}

		public void WriteGetObject ()
		{
			manager.GetObject ();

			var xm = CurrentMember;

			var state = new ObjectState () {Type = xm.Type, IsGetObject = true};

			object_states.Push (state);

			OnWriteGetObject ();
		}

		public void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			if (namespaceDeclaration == null)
				throw new ArgumentNullException ("namespaceDeclaration");

			manager.Namespace ();

			namespaces.Add (namespaceDeclaration);
			OnWriteNamespace (namespaceDeclaration);
		}

		public void WriteStartObject (XamlType xamlType)
		{
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");

			manager.StartObject ();

			var cstate = new ObjectState () {Type = xamlType};
			object_states.Push (cstate);

			OnWriteStartObject ();
		}
		
		public void WriteValue (object value)
		{
			manager.Value ();

			OnWriteValue (value);
		}
		
		public void WriteStartMember (XamlMember property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			manager.StartMember ();
			if (property == XamlLanguage.PositionalParameters)
				// this is an exception that indicates the state manager to accept more than values within this member.
				manager.AcceptMultipleValues = true;

			var state = object_states.Peek ();
			var wpl = state.WrittenProperties;
			if (wpl.Any (wp => wp.Member == property))
				throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", property, object_states.Peek ().Type));
			wpl.Add (new MemberAndValue (property));
			if (property == XamlLanguage.PositionalParameters)
				state.PositionalParameterIndex = 0;

			OnWriteStartMember (property);
		}
		
		public void WriteEndObject ()
		{
			manager.EndObject (object_states.Count > 1);

			OnWriteEndObject ();

			object_states.Pop ();
		}

		public void WriteEndMember ()
		{
			manager.EndMember ();

			OnWriteEndMember ();
			
			var state = object_states.Peek ();
			if (CurrentMember == XamlLanguage.PositionalParameters) {
				manager.AcceptMultipleValues = false;
				state.PositionalParameterIndex = -1;
			}
		}

		protected abstract void OnWriteEndObject ();

		protected abstract void OnWriteEndMember ();

		protected abstract void OnWriteStartObject ();

		protected abstract void OnWriteGetObject ();

		protected abstract void OnWriteStartMember (XamlMember xm);

		protected abstract void OnWriteValue (object value);

		protected abstract void OnWriteNamespace (NamespaceDeclaration nd);
		
		protected string GetValueString (XamlMember xm, object value)
		{
			// change XamlXmlReader too if we change here.
			if ((value as string) == String.Empty) // FIXME: there could be some escape syntax.
				return "\"\"";

			var xt = value == null ? XamlLanguage.Null : sctx.GetXamlType (value.GetType ());
			var vs = xm.ValueSerializer ?? xt.ValueSerializer;
			if (vs != null)
				return vs.ConverterInstance.ConvertToString (value, service_provider);
			else
				throw new XamlXmlWriterException (String.Format ("Value type is '{0}' but it must be either string or any type that is convertible to string indicated by TypeConverterAttribute.", value != null ? value.GetType () : null));
		}
	}
}
