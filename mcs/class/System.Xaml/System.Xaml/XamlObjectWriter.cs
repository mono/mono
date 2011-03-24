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
using System.Xml;
using System.Xml.Serialization;

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
		INameScope name_scope = new NameScope ();
		List<NameFixupRequired> pending_name_references = new List<NameFixupRequired> ();
		AmbientProvider ambient_provider = new AmbientProvider ();

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
			var nfr = obj as NameFixupRequired;
			if (nfr != null && object_states.Count > 0) { // IF the root object to be written is x:Reference, then the Result property will become the NameFixupRequired. That's what .NET also does.
				// actually .NET seems to seek "parent" object in its own IXamlNameResolver implementation.
				var pstate = object_states.Peek ();
				nfr.ParentType = pstate.Type;
				nfr.ParentMember = CurrentMember; // Note that it is a member of the pstate.
				nfr.ParentValue = pstate.Value;
				pending_name_references.Add ((NameFixupRequired) obj);
			}
			else
				StoreAppropriatelyTypedValue (obj, state.KeyValue);
			
			if (state.Type.IsAmbient)
				ambient_provider.Pop ();
			
			object_states.Push (state);
			if (object_states.Count == 1) {
				Result = obj;
				ResolvePendingReferences ();
			}
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
			} else if (xm == XamlLanguage.Name || xm == state.Type.GetAliasedProperty (XamlLanguage.Name)) {
				string name = (string) CurrentMemberState.Value;
				name_scope.RegisterName (name, state.Value);
			} else {
				if (xm.IsEvent)
					SetEvent (xm, (string) CurrentMemberState.Value);
				else if (!xm.IsReadOnly) // exclude read-only object such as collection item.
					SetValue (xm, CurrentMemberState.Value);
			}
		}

		void SetEvent (XamlMember member, string value)
		{
			if (member.UnderlyingMember == null)
				throw new XamlObjectWriterException (String.Format ("Event {0} has no underlying member to attach event", member));

			int idx = value.LastIndexOf ('.');
			var xt = idx < 0 ? member.DeclaringType : ResolveTypeFromName (value.Substring (0, idx));
			if (xt == null)
				throw new XamlObjectWriterException (String.Format ("Referenced type {0} in event {1} was not found", value, member));
			if (xt.UnderlyingType == null)
				throw new XamlObjectWriterException (String.Format ("Referenced type {0} in event {1} has no underlying type", value, member));
			string mn = idx < 0 ? value : value.Substring (idx + 1);
			var ev = (EventInfo) member.UnderlyingMember;
			// get an appropriate MethodInfo overload whose signature matches the event's handler type.
			// FIXME: this may need more strict match. RuntimeBinder may be useful here.
			var eventMethodParams = ev.EventHandlerType.GetMethod ("Invoke").GetParameters ();
			var mi = xt.UnderlyingType.GetMethod (mn, (from pi in eventMethodParams select pi.ParameterType).ToArray ());
			if (mi == null)
				throw new XamlObjectWriterException (String.Format ("Referenced value method {0} in type {1} indicated by event {2} was not found", mn, value, member));
			var obj = object_states.Peek ().Value;
			ev.AddEventHandler (obj, Delegate.CreateDelegate (ev.EventHandlerType, obj, mi));
		}

		void SetValue (XamlMember member, object value)
		{
			if (member == XamlLanguage.FactoryMethod)
				object_states.Peek ().FactoryMethod = (string) value;
			else if (member.IsDirective)
				return;
			else if (member.IsAttachable)
				AttachablePropertyServices.SetProperty (object_states.Peek ().Value, new AttachableMemberIdentifier (member.DeclaringType.UnderlyingType, member.Name), value);
			else if (!source.OnSetValue (this, member, value))
				member.Invoker.SetValue (object_states.Peek ().Value, value);
		}

		void PopulateObject (bool considerPositionalParameters, IList<object> contents)
		{
			var state = object_states.Peek ();

			var args = state.Type.GetSortedConstructorArguments ().ToArray ();
			var argt = args != null ? (IList<XamlType>) (from arg in args select arg.Type).ToArray () : considerPositionalParameters ? state.Type.GetPositionalParameters (contents.Count) : null;

			var argv = new object [argt.Count];
			for (int i = 0; i < argv.Length; i++)
				argv [i] = GetCorrectlyTypedValue (args [i], argt [i], contents [i]);
			state.Value = state.Type.Invoker.CreateInstance (argv);
			state.IsInstantiated = true;
			if (state.Type.IsAmbient)
				ambient_provider.Push (new AmbientPropertyValue (CurrentMember, state.Value));
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
				var parent = state.Value;
				var xt = state.Type;
				var xm = ms.Member;
				if (xm == XamlLanguage.Initialization) {
					state.Value = GetCorrectlyTypedValue (null, xt, obj);
					state.IsInstantiated = true;
				} else if (xm.IsEvent) {
					ms.Value = (string) obj; // save name of value delegate (method).
					state.IsInstantiated = true;
				} else if (xm.Type.IsXData) {
					var xdata = (XData) obj;
					var ixser = xm.Invoker.GetValue (state.Value) as IXmlSerializable;
					if (ixser != null)
						ixser.ReadXml ((XmlReader) xdata.XmlReader);
				}
				else if (xm == XamlLanguage.Base)
					ms.Value = GetCorrectlyTypedValue (null, xm.Type, obj);
				else if (xm == XamlLanguage.Name || xm == xt.GetAliasedProperty (XamlLanguage.Name))
					ms.Value = GetCorrectlyTypedValue (xm, XamlLanguage.String, obj);
				else if (xm == XamlLanguage.Key)
					state.KeyValue = GetCorrectlyTypedValue (null, xt.KeyType, obj);
				else {
					if (!AddToCollectionIfAppropriate (xt, xm, parent, obj, keyObj)) {
						if (!xm.IsReadOnly)
							ms.Value = GetCorrectlyTypedValue (xm, xm.Type, obj);
					}
				}
			}
		}

		bool AddToCollectionIfAppropriate (XamlType xt, XamlMember xm, object parent, object obj, object keyObj)
		{
			var mt = xm.Type;
			if (xm == XamlLanguage.Items ||
			    xm == XamlLanguage.PositionalParameters ||
			    xm == XamlLanguage.Arguments) {
				if (xt.IsDictionary)
					mt.Invoker.AddToDictionary (parent, GetCorrectlyTypedValue (null, xt.KeyType, keyObj), GetCorrectlyTypedValue (null, xt.ItemType, obj));
				else // collection. Note that state.Type isn't usable for PositionalParameters to identify collection kind.
					mt.Invoker.AddToCollection (parent, GetCorrectlyTypedValue (null, xt.ItemType, obj));
				return true;
			}
			else
				return false;
		}

		object GetCorrectlyTypedValue (XamlMember xm, XamlType xt, object value)
		{
			try {
				return DoGetCorrectlyTypedValue (xm, xt, value);
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
		// But do not immediately try to instantiate with the type, since the type might be abstract.
		object DoGetCorrectlyTypedValue (XamlMember xm, XamlType xt, object value)
		{
			if (value == null) {
				if (xt.IsContentValue (service_provider)) // it is for collection/dictionary key and item
					return null;
				else
					return xt.IsNullable ? null : xt.Invoker.CreateInstance (new object [0]);
			}
			if (xt == null)
				return value;

			// Not sure if this is really required though...
			var vt = sctx.GetXamlType (value.GetType ());
			if (vt.CanAssignTo (xt))
				return value;

			// FIXME: this could be generalized by some means, but I cannot find any.
			if (xt.UnderlyingType == typeof (XamlType) && value is string)
				value = ResolveTypeFromName ((string) value);

			// FIXME: this could be generalized by some means, but I cannot find any.
			if (xt.UnderlyingType == typeof (Type))
				value = new TypeExtension ((string) value).ProvideValue (service_provider);
			if (xt == XamlLanguage.Type && value is string)
				value = new TypeExtension ((string) value);
			
			if (IsAllowedType (xt, value))
				return value;

			var xtc = (xm != null ? xm.TypeConverter : null) ?? xt.TypeConverter;
			if (xtc != null && value != null) {
				var tc = xtc.ConverterInstance;
				if (tc != null && tc.CanConvertFrom (value.GetType ()))
					value = tc.ConvertFrom (value);
				return value;
			}

			throw new XamlObjectWriterException (String.Format ("Value '{0}' (of type {1}) is not of or convertible to type {0} (member {3})", value, value != null ? (object) value.GetType () : "(null)", xt, xm));
		}

		XamlType ResolveTypeFromName (string name)
		{
			var nsr = (IXamlNamespaceResolver) service_provider.GetService (typeof (IXamlNamespaceResolver));
			return sctx.GetXamlType (XamlTypeName.Parse (name, nsr));
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
			if (state.Type.IsAmbient)
				ambient_provider.Push (new AmbientPropertyValue (CurrentMember, obj));
		}

		internal IXamlNameResolver name_resolver {
			get { return (IXamlNameResolver) service_provider.GetService (typeof (IXamlNameResolver)); }
		}

		internal override IAmbientProvider AmbientProvider {
			get { return ambient_provider; }
		}

		void ResolvePendingReferences ()
		{
			foreach (var fixup in pending_name_references) {
				foreach (var name in fixup.Names) {
					bool isFullyInitialized;
					// FIXME: sort out relationship between name_scope and name_resolver. (unify to name_resolver, probably)
					var obj = name_scope.FindName (name) ?? name_resolver.Resolve (name, out isFullyInitialized);
					if (obj == null)
						throw new XamlObjectWriterException (String.Format ("Unresolved object reference '{0}' was found", name));
					if (!AddToCollectionIfAppropriate (fixup.ParentType, fixup.ParentMember, fixup.ParentValue, obj, null)) // FIXME: is keyObj always null?
						fixup.ParentMember.Invoker.SetValue (fixup.ParentValue, obj);
				}
			}
		}
	}
}
