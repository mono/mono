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
using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Xaml
{
	public class XamlObjectWriter : XamlWriter, IXamlLineInfoConsumer
	{
		public XamlObjectWriter (XamlSchemaContext schemaContext)
			: this (schemaContext, null)
		{
		}

		public XamlObjectWriter (XamlSchemaContext schemaContext, XamlObjectWriterSettings settings)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			this.sctx = schemaContext;
			this.settings = settings ?? new XamlObjectWriterSettings ();
		}

		XamlSchemaContext sctx;
		XamlObjectWriterSettings settings;

		XamlWriterStateManager manager = new XamlWriterStateManager<XamlObjectWriterException, XamlObjectWriterException> (false);
		object result;
		int line = -1, column = -1;
		Stack<object> objects = new Stack<object> ();
		Stack<XamlType> types = new Stack<XamlType> ();
		Stack<XamlMember> members = new Stack<XamlMember> ();

		List<object> arguments = new List<object> ();
		string factory_method;
		bool object_instantiated;
		List<object> contents = new List<object> ();
		List<object> objects_from_getter = new List<object> ();
		Stack<List<XamlMember>> written_properties_stack = new Stack<List<XamlMember>> ();

		public virtual object Result {
			get { return result; }
		}

		public INameScope RootNameScope {
			get { throw new NotImplementedException (); }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public bool ShouldProvideLineInfo {
			get { return line > 0 && column > 0; }
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			while (types.Count > 0) {
				WriteEndObject ();
				if (types.Count > 0)
					WriteEndMember ();
			}
		}

		protected virtual void OnAfterBeginInit (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAfterEndInit (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAfterProperties (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnBeforeProperties (object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool OnSetValue (object eventSender, XamlMember member, object value)
		{
			if (settings.XamlSetValueHandler != null) {
				settings.XamlSetValueHandler (eventSender, new XamlSetValueEventArgs (member, value));
				return true;
			}
			return false;
		}

		public void SetLineInfo (int lineNumber, int linePosition)
		{
			line = lineNumber;
			column = linePosition;
		}

		[MonoTODO ("Array and Dictionary needs implementation")]
		public override void WriteEndMember ()
		{
			manager.EndMember ();
			
			var xm = members.Pop ();
			var xt = xm.Type;

			if (xt.IsArray) {
				throw new NotImplementedException ();
			} else if (xt.IsCollection) {
				var obj = objects.Peek ();
				foreach (var content in contents)
					xt.Invoker.AddToCollection (obj, content);
			} else if (xt.IsDictionary) {
				throw new NotImplementedException ();
			} else {
				if (contents.Count > 1)
					throw new XamlDuplicateMemberException (String.Format ("Value for {0} is assigned more than once", xm.Name));
				if (contents.Count == 1) {
					var value = GetCorrectlyTypedValue (xm, contents [0]);
					if (!objects_from_getter.Remove (value))
						if (!OnSetValue (this, xm, value))
							xm.Invoker.SetValue (objects.Peek (), value);
				}
			}

			contents.Clear ();
			written_properties_stack.Peek ().Add (xm);
		}

		object GetCorrectlyTypedValue (XamlMember xm, object value)
		{
			var xt = xm.Type;
			if (IsAllowedType (xt, value))
				return value;
			if (xt.TypeConverter != null && value != null) {
				var tc = xt.TypeConverter.ConverterInstance;
				if (tc != null && tc.CanConvertFrom (value.GetType ()))
					value = tc.ConvertFrom (value);
				if (IsAllowedType (xt, value))
					return value;
			}
			throw new XamlObjectWriterException (String.Format ("Value is not of type {0}", xt));
		}

		bool IsAllowedType (XamlType xt, object value)
		{
			return xt == null || xt.UnderlyingType == null || xt.UnderlyingType.IsInstanceOfType (value);
		}

		public override void WriteEndObject ()
		{
			manager.EndObject (types.Count > 0);

			InitializeObjectIfRequired (); // this is required for such case that there was no StartMember call.

			types.Pop ();
			written_properties_stack.Pop ();
			var obj = objects.Pop ();
			if (members.Count > 0)
				contents.Add (obj);
			if (objects.Count == 0)
				result = obj;
		}

		public override void WriteGetObject ()
		{
			manager.GetObject ();

			var xm = members.Peek ();
			// see GetObjectOnNonNullString() test
			//if (!xm.Type.IsCollection)
			//	throw new XamlObjectWriterException (String.Format ("WriteGetObject method can be invoked only when current member '{0}' is of collection type", xm.Name));

			var obj = xm.Invoker.GetValue (objects.Peek ());
			if (obj == null)
				throw new XamlObjectWriterException (String.Format ("The value  for '{0}' property is null", xm.Name));

			types.Push (SchemaContext.GetXamlType (obj.GetType ()));
			ObjectInitialized (obj);
			objects_from_getter.Add (obj);
		}

		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			if (namespaceDeclaration == null)
				throw new ArgumentNullException ("namespaceDeclaration");

			manager.Namespace ();

			// FIXME: find out what to do.
		}

		public override void WriteStartMember (XamlMember property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			manager.StartMember ();

			var wpl = written_properties_stack.Peek ();
			if (wpl.Contains (property))
				throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", property.Name, types.Peek ().Name));
			wpl.Add (property);

			members.Push (property);

			if (property == XamlLanguage.Initialization)
				return;
			else
				InitializeObjectIfRequired ();
		}

		void InitializeObjectIfRequired ()
		{
			if (object_instantiated)
				return;

			// FIXME: "The default techniques in absence of a factory method are to attempt to find a default constructor, then attempt to find an identified type converter on type, member, or destination type."
			// http://msdn.microsoft.com/en-us/library/system.xaml.xamllanguage.factorymethod%28VS.100%29.aspx
			object obj;
			var args = arguments.ToArray ();
			if (factory_method != null)
				obj = types.Peek ().UnderlyingType.GetMethod (factory_method).Invoke (null, args);
			else
				obj = types.Peek ().Invoker.CreateInstance (args);
			ObjectInitialized (obj);
		}

		public override void WriteStartObject (XamlType xamlType)
		{
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");

			manager.StartObject ();

			types.Push (xamlType);

			object_instantiated = false;

			written_properties_stack.Push (new List<XamlMember> ());
		}

		public override void WriteValue (object value)
		{
			manager.Value ();

			var xm = members.Peek ();

			if (xm == XamlLanguage.Initialization)
				ObjectInitialized (value);
			else if (xm == XamlLanguage.Arguments)
				arguments.Add (value);
			else if (xm == XamlLanguage.FactoryMethod)
				factory_method = (string) value;
			else
				contents.Add (value);
		}

		void ObjectInitialized (object obj)
		{
			objects.Push (obj);
			object_instantiated = true;
			arguments.Clear ();
			factory_method = null;
		}
	}
}
