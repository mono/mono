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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

// To use this under .NET, compile sources as:
//
//	dmcs -d:DOTNET -r:System.Xaml -debug System.Xaml/XamlObjectWriter.cs System.Xaml/XamlWriterInternalBase.cs System.Xaml/TypeExtensionMethods.cs System.Xaml/XamlWriterStateManager.cs System.Xaml/XamlNameResolver.cs System.Xaml/PrefixLookup.cs System.Xaml/ValueSerializerContext.cs ../../build/common/MonoTODOAttribute.cs Test/System.Xaml/TestedTypes.cs

/*

State transition:

* StartObject or GetObject
	These start a new object instance, either by creating new or getting
	from parent.
* Value
	This either becomes an entire property value, or an item of current
	collection, or a key or a value item of current dictionary, or an
	entire object if it is either Initialization.
* EndObject
	Almost the same as Value. Though the it is likely already instantiated.
* StartMember
	Indicates a new property as current.
* EndMember
	It accompanies a property value (might be lacking), or ends a
	collection (including those for PositionalParameters), or ends a key
	property of a dictionary element (if it is Key), or ends an entire
	value of current object if it is Initialization.


*/

#if DOTNET
namespace Mono.Xaml
#else
namespace System.Xaml
#endif
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
			var manager = new XamlWriterStateManager<XamlObjectWriterException, XamlObjectWriterException> (false);
			intl = new XamlObjectWriterInternal (this, sctx, manager);
		}

		XamlSchemaContext sctx;
		XamlObjectWriterSettings settings;

		XamlObjectWriterInternal intl;

		int line, column;
		bool lineinfo_was_given;

		public virtual object Result {
			get { return intl.Result; }
		}

		public INameScope RootNameScope {
			get { throw new NotImplementedException (); }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public bool ShouldProvideLineInfo {
			get { return lineinfo_was_given; }
		}

		public void SetLineInfo (int lineNumber, int linePosition)
		{
			line = lineNumber;
			column = linePosition;
			lineinfo_was_given = true;
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			intl.CloseAll ();
		}

		protected internal virtual void OnAfterBeginInit (object value)
		{
			if (settings.AfterBeginInitHandler != null)
				settings.AfterBeginInitHandler (this, new XamlObjectEventArgs (value));
		}

		protected internal virtual void OnAfterEndInit (object value)
		{
			if (settings.AfterEndInitHandler != null)
				settings.AfterEndInitHandler (this, new XamlObjectEventArgs (value));
		}

		protected internal virtual void OnAfterProperties (object value)
		{
			if (settings.AfterPropertiesHandler != null)
				settings.AfterPropertiesHandler (this, new XamlObjectEventArgs (value));
		}

		protected internal virtual void OnBeforeProperties (object value)
		{
			if (settings.BeforePropertiesHandler != null)
				settings.BeforePropertiesHandler (this, new XamlObjectEventArgs (value));
		}

		protected internal virtual bool OnSetValue (object eventSender, XamlMember member, object value)
		{
			if (settings.XamlSetValueHandler != null) {
				settings.XamlSetValueHandler (eventSender, new XamlSetValueEventArgs (member, value));
				return true;
			}
			return false;
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
	class XamlObjectWriterInternal : XamlWriterInternalBase
	{
		const string Xmlns2000Namespace = "http://www.w3.org/2000/xmlns/";

		public XamlObjectWriterInternal (XamlObjectWriter source, XamlSchemaContext schemaContext, XamlWriterStateManager manager)
			: base (schemaContext, manager)
		{
			this.source = source;
			this.sctx = schemaContext;
		}
		
		XamlObjectWriter source;
		XamlSchemaContext sctx;
		
		public object Result { get; set; }
		
		protected override void OnWriteStartObject ()
		{
			var state = object_states.Pop ();
			if (object_states.Count > 0) {
				var pstate = object_states.Peek ();
				if (CurrentMemberState.Value != null)
					throw new XamlDuplicateMemberException (String.Format ("Member '{0}' is already written to current type '{1}'", CurrentMember, pstate.Type));
			}
			object_states.Push (state);
			if (!state.Type.IsContentValue (service_provider))
				InitializeObjectIfRequired (true);


		}

		protected override void OnWriteGetObject ()
		{
			var state = object_states.Pop ();
			var xm = CurrentMember;
			var instance = xm.Invoker.GetValue (object_states.Peek ().Value);
			if (instance == null)
				throw new XamlObjectWriterException (String.Format ("The value  for '{0}' property is null", xm.Name));
			state.Value = instance;
			state.IsInstantiated = true;
			object_states.Push (state);
		}

		protected override void OnWriteEndObject ()
		{
			InitializeObjectIfRequired (false); // this is required for such case that there was no StartMember call.

			var state = object_states.Pop ();
			var obj = state.Value;
			
			if (obj is MarkupExtension) {
				try {
					obj = ((MarkupExtension) obj).ProvideValue (service_provider);
				} catch (XamlObjectWriterException) {
					throw;
				} catch (Exception ex) {
					throw new XamlObjectWriterException ("An error occured on getting provided value", ex);
				}
			}
			StoreAppropriatelyTypedValue (obj, state.KeyValue);
			object_states.Push (state);
			if (object_states.Count == 1)
				Result = obj;
		}

		Stack<object> escaped_objects = new Stack<object> ();

		protected override void OnWriteStartMember (XamlMember property)
		{
			if (property == XamlLanguage.PositionalParameters ||
			    property == XamlLanguage.Arguments) {
				var state = object_states.Peek ();
				escaped_objects.Push (state.Value);
				state.Value = new List<object> ();
			}

			// FIXME: this condition needs to be examined. What is known to be prevented are: PositionalParameters, Initialization and Base (the last one sort of indicates there's a lot more).
			else if (!(property is XamlDirective))
				InitializeObjectIfRequired (false);
		}

		static readonly BindingFlags static_flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		protected override void OnWriteEndMember ()
		{
			var xm = CurrentMember;
			var state = object_states.Peek ();

			if (xm == XamlLanguage.PositionalParameters) {
				var l = (List<object>) state.Value;
				state.Value = escaped_objects.Pop ();
				state.IsInstantiated = true;
				PopulateObject (true, l);
				return;
			} else if (xm == XamlLanguage.Arguments) {
				if (state.FactoryMethod != null) {
					var contents = (List<object>) state.Value;
					var mi = state.Type.UnderlyingType.GetMethods (static_flags).FirstOrDefault (mii => mii.Name == state.FactoryMethod && mii.GetParameters ().Length == contents.Count);
					if (mi == null)
						throw new XamlObjectWriterException (String.Format ("Specified static factory method '{0}' for type '{1}' was not found", state.FactoryMethod, state.Type));
					state.Value = mi.Invoke (null, contents.ToArray ());
				}
				else
					PopulateObject (false, (List<object>) state.Value);
				state.IsInstantiated = true;
				escaped_objects.Pop ();
			} else if (xm == XamlLanguage.Initialization) {
				// ... and no need to do anything. The object value to pop *is* the return value.
			} else {
				if (!xm.IsReadOnly) // exclude read-only object.
					SetValue (xm, CurrentMemberState.Value);
			}
		}

		void SetValue (XamlMember member, object value)
		{
			if (member == XamlLanguage.FactoryMethod)
				object_states.Peek ().FactoryMethod = (string) value;
			else if (member.IsDirective)
				return;
			else if (!source.OnSetValue (this, member, value))
				member.Invoker.SetValue (object_states.Peek ().Value, value);
		}

		void PopulateObject (bool considerPositionalParameters, IList<object> contents)
		{
			var state = object_states.Peek ();

			var args = state.Type.GetSortedConstructorArguments ();
			var argt = args != null ? (IList<XamlType>) (from arg in args select arg.Type).ToArray () : considerPositionalParameters ? state.Type.GetPositionalParameters (contents.Count) : null;

			var argv = new object [argt.Count];
			for (int i = 0; i < argv.Length; i++)
				argv [i] = GetCorrectlyTypedValue (argt [i], contents [i]);
			state.Value = state.Type.Invoker.CreateInstance (argv);
			state.IsInstantiated = true;
		}

		protected override void OnWriteValue (object value)
		{
			if (CurrentMemberState.Value != null)
				throw new XamlDuplicateMemberException (String.Format ("Member '{0}' is already written to current type '{1}'", CurrentMember, object_states.Peek ().Type));
			StoreAppropriatelyTypedValue (value, null);
		}

		protected override void OnWriteNamespace (NamespaceDeclaration nd)
		{
			// nothing to do here.
		}
		
		void StoreAppropriatelyTypedValue (object obj, object keyObj)
		{
			var ms = CurrentMemberState; // note that this retrieves parent's current property for EndObject.
			if (ms != null) {
				var state = object_states.Peek ();
				var xm = ms.Member;
				if (xm == XamlLanguage.Initialization) {
					state.Value = GetCorrectlyTypedValue (state.Type, obj);
					state.IsInstantiated = true;
				}
				else if (xm == XamlLanguage.Base)
					ms.Value = GetCorrectlyTypedValue (xm.Type, obj);
				else {
					var mt = xm.Type;
					if (ms.Member == XamlLanguage.Items ||
					    ms.Member == XamlLanguage.PositionalParameters ||
					    ms.Member == XamlLanguage.Arguments) {
						if (state.Type.IsDictionary)
							mt.Invoker.AddToDictionary (state.Value, GetCorrectlyTypedValue (state.Type.KeyType, keyObj), GetCorrectlyTypedValue (state.Type.ItemType, obj));
						else // collection. Note that state.Type isn't usable for PositionalParameters to identify collection kind.
							state.Type.Invoker.AddToCollection (state.Value, GetCorrectlyTypedValue (state.Type.ItemType, obj));
					} else if (!ms.Member.IsReadOnly) {
						if (ms.Member == XamlLanguage.Key)
							state.KeyValue = GetCorrectlyTypedValue (state.Type.KeyType, obj);
						else
							ms.Value = GetCorrectlyTypedValue (ms.Member.Type, obj);
					}
				}
			}
		}

		object GetCorrectlyTypedValue (XamlType xt, object value)
		{
			try {
				return DoGetCorrectlyTypedValue (xt, value);
			} catch (XamlObjectWriterException) {
				throw;
			} catch (Exception ex) {
				// For + ex.Message, the runtime should print InnerException message like .NET does.
				throw new XamlObjectWriterException (String.Format ("Could not convert object \'{0}' (of type {1}) to {2}: ", value, value != null ? (object) value.GetType () : "(null)", xt)  + ex.Message, ex);
			}
		}

		// It expects that it is not invoked when there is no value to 
		// assign.
		// When it is passed null, then it returns a default instance.
		// For example, passing null as Int32 results in 0.
		object DoGetCorrectlyTypedValue (XamlType xt, object value)
		{
			if (value == null) {
				if (xt.IsContentValue (service_provider)) // it is for collection/dictionary key and item
					return null;
				else
					return xt.Invoker.CreateInstance (new object [0]);
			}
			if (xt == null)
				return value;

			// Not sure if this is really required though...
			var vt = sctx.GetXamlType (value.GetType ());
			if (vt.CanAssignTo (xt))
				return value;

			// FIXME: this could be generalized by some means, but I cannot find any.
			if (xt.UnderlyingType == typeof (XamlType) && value is string) {
				var nsr = (IXamlNamespaceResolver) service_provider.GetService (typeof (IXamlNamespaceResolver));
				value = sctx.GetXamlType (XamlTypeName.Parse ((string) value, nsr));
			}

			// FIXME: this could be generalized by some means, but I cannot find any.
			if (xt.UnderlyingType == typeof (Type))
				value = new TypeExtension ((string) value).ProvideValue (service_provider);
			if (xt == XamlLanguage.Type && value is string)
				value = new TypeExtension ((string) value);
			
			if (IsAllowedType (xt, value))
				return value;

			if (xt.TypeConverter != null && value != null) {
				var tc = xt.TypeConverter.ConverterInstance;
				if (tc != null && tc.CanConvertFrom (value.GetType ()))
					value = tc.ConvertFrom (value);
				return value;
			}

			throw new XamlObjectWriterException (String.Format ("Value '{1}' (of type {2}) is not of or convertible to type {0}", xt, value, value != null ? (object) value.GetType () : "(null)"));
		}

		bool IsAllowedType (XamlType xt, object value)
		{
			return  xt == null ||
				xt.UnderlyingType == null ||
				xt.UnderlyingType.IsInstanceOfType (value) ||
				value == null && xt == XamlLanguage.Null ||
				xt.IsMarkupExtension && IsAllowedType (xt.MarkupExtensionReturnType, value);
		}
		
		void InitializeObjectIfRequired (bool waitForParameters)
		{
			var state = object_states.Peek ();
			if (state.IsInstantiated)
				return;

			if (waitForParameters && (state.Type.ConstructionRequiresArguments || state.Type.HasPositionalParameters (service_provider)))
				return;

			// FIXME: "The default techniques in absence of a factory method are to attempt to find a default constructor, then attempt to find an identified type converter on type, member, or destination type."
			// http://msdn.microsoft.com/en-us/library/system.xaml.xamllanguage.factorymethod%28VS.100%29.aspx
			object obj;
			if (state.FactoryMethod != null) // FIXME: it must be implemented and verified with tests.
				throw new NotImplementedException ();
			else
				obj = state.Type.Invoker.CreateInstance (null);
			state.Value = obj;
			state.IsInstantiated = true;
		}
	}
}
