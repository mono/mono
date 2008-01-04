//
// System.ComponentModel.Design.Serialization.CodeDomComponentSerializationService
//
// Authors:	 
//	  Ivan N. Zlatev (contact@i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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

#if NET_2_0

using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace System.ComponentModel.Design.Serialization
{
	// A ComponentSerializationService that uses a CodeDomSerializationStore 
	// to serialize Components and MemberDescriptors to CodeStatement and CodeStatementCollection
	//
	public sealed class CodeDomComponentSerializationService : ComponentSerializationService
	{
		[Serializable]
		private class CodeDomSerializationStore : SerializationStore
		{

			[Serializable]
			private class Entry
			{
				private bool _isSerialized;
				private object _serialized;
				private object _deserialized;
				private bool _isDeserialized;
				private bool _absolute;
				private string _name;

				protected Entry ()
				{
				}

				public Entry (string name)
				{
					if (name == null)
						throw new ArgumentNullException ("name");
					_name = name;
					_isDeserialized = true;
					_isSerialized = false;
					_absolute = false;
				}

				public bool IsSerialized {
					get { return _isSerialized; }
					set { _isSerialized = value; }
				}

				public object Serialized {
					get { return _serialized; }
					set { 
						_serialized = value;
						_isSerialized = true;
					}
				}

				public bool IsDeserialized {
					get { return _isDeserialized; }
					set { _isDeserialized = value; }
				}

				public object Deserialized {
					get { return _deserialized; }
					set { 
						_deserialized = value;
						_isDeserialized = true;
					}
				}

				public bool Absolute {
					get { return _absolute; }
					set { _absolute = value; }
				}

				public string Name {
					get { return _name; }
					set { _name = value; }
				}
			}

			[Serializable]
			private class MemberEntry : Entry
			{
				private MemberDescriptor _descriptor;

				protected MemberEntry ()
				{
				}

				public MemberEntry (MemberDescriptor descriptor)
				{
					if (descriptor == null)
						throw new ArgumentNullException ("descriptor");
					_descriptor = descriptor;
					base.Name = descriptor.Name;
				}

				public MemberDescriptor Descriptor {
					get { return _descriptor; }
					set { _descriptor = value; }
				}
			}

			[Serializable]
			private class ObjectEntry : Entry
			{
				private Type _type;
				[NonSerialized]
				private object _instance;
				private Dictionary<string,MemberEntry> _members;
				private bool _entireObject;

				protected ObjectEntry ()
				{
				}

				public ObjectEntry (object instance, string name) : base (name)
				{
					if (instance == null)
						throw new ArgumentNullException ("instance");
					_instance = instance;
					_type = instance.GetType ();
					_entireObject = false;
				}

				public Type Type {
					get { return _type; }
				}

				public object Instance {
					get { return _instance; }
					set { 
						_instance = value;
						if (value != null)
							_type = value.GetType ();
					}
				}

				public Dictionary<string,MemberEntry> Members {
					get { 
						if (_members == null)
							_members = new Dictionary <string, MemberEntry> ();
						return _members; 
					}
					set { _members = value; }
				}

				public bool EntireObject {
					get { return _entireObject; }
					set { _entireObject = value; }
				}
			}

			private bool _closed;
			private Dictionary <string, ObjectEntry> _objects;
			private IServiceProvider _provider;
			private ICollection _errors;

			internal CodeDomSerializationStore () : this (null)
			{
			}

			internal CodeDomSerializationStore (IServiceProvider provider)
			{
				_provider = provider;
			}

			public override void Close () 
			{
				if (!_closed) {
					Serialize (_provider);
					_closed = true;
				}
			}

			internal static CodeDomSerializationStore Load (Stream stream)
			{
				return new BinaryFormatter ().Deserialize (stream) as CodeDomSerializationStore;
			}

			public override void Save (Stream stream) 
			{
				Close ();
				new BinaryFormatter ().Serialize (stream, this);
			}

			private void Serialize (IServiceProvider provider)
			{
				if (provider == null)
					return;
				DesignerSerializationManager manager = provider.GetService (typeof (IDesignerSerializationManager)) as DesignerSerializationManager;
				if (manager == null)
					return;

				using (IDisposable session = manager.CreateSession ()) {
					foreach (ObjectEntry objectEntry in _objects.Values) {
						if (objectEntry.EntireObject) {
							CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer (objectEntry.Type, 
														typeof (CodeDomSerializer));
							if (serializer != null) {
								object serialized = null;
								if (objectEntry.Absolute)
									serialized = serializer.SerializeAbsolute (manager, objectEntry.Instance);
								else
									serialized = serializer.Serialize (manager, objectEntry.Instance);
								if (serialized != null)
									objectEntry.Serialized = serialized;
							}
						} else {
							foreach (MemberEntry memberEntry in objectEntry.Members.Values) {
								CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer (
									objectEntry.Type, typeof (CodeDomSerializer));
								if (serializer != null) {
									object serialized = null;
									if (memberEntry.Absolute) {
											serialized = serializer.SerializeMemberAbsolute (manager, 
																	 objectEntry.Instance, 
																	 memberEntry.Descriptor);
									} else {
											serialized = serializer.SerializeMember (manager,
																 objectEntry.Instance, 
																 memberEntry.Descriptor);
									}
									if (serialized != null)
										memberEntry.Serialized = serialized;
								}
							}
						}
					}
					_errors = manager.Errors;
				}
			}

			internal void AddObject (object instance, bool absolute)
			{
				if (_closed)
					throw new InvalidOperationException ("store is closed");

				if (_objects == null)
					_objects = new Dictionary <string, ObjectEntry> ();
				string objectName = GetName (instance);

				if (!_objects.ContainsKey (objectName)) {
					ObjectEntry objectEntry = new ObjectEntry (instance, objectName);
					objectEntry.Absolute = absolute;
					objectEntry.EntireObject = true;
					_objects[objectName] = objectEntry;
				}
			}

			internal void AddMember (object owner, MemberDescriptor member, bool absolute)
			{
				if (_closed)
					throw new InvalidOperationException ("store is closed");
				if (member == null)
					throw new ArgumentNullException ("member");
				if (owner == null)
					throw new ArgumentNullException ("owner");

				if (_objects == null)
					_objects = new Dictionary <string, ObjectEntry> ();
				string objectName = GetName (owner);

				if (!_objects.ContainsKey (objectName)) {
					ObjectEntry objectEntry = new ObjectEntry (owner, objectName);
					MemberEntry memberEntry = new MemberEntry (member);
					memberEntry.Absolute = absolute;
					objectEntry.Members[member.Name] = memberEntry;
					_objects[objectName] = objectEntry;
				}
			}

			private string GetName (object value)
			{
				string name = null;

				IComponent component = value as IComponent;
				if (component != null) {
					if (component.Site is INestedSite)
						name = ((INestedSite)component.Site).FullName;
					else
						name = component.Site != null ? component.Site.Name : null;
				} else if (value is MemberDescriptor) {
					name = ((MemberDescriptor) value).Name;
				} else {
					name = value.GetHashCode ().ToString ();
				}

				return name;
			}

			internal ICollection Deserialize (IServiceProvider provider, IContainer container, 
							  bool validateRecycledTypes, bool applyDefaults)
			{
				List<object> objectInstances = new List<object> ();

				if (provider == null)
					return objectInstances;
				_provider = provider;

				DesignerSerializationManager manager = provider.GetService 
					(typeof (IDesignerSerializationManager)) as DesignerSerializationManager;
				if (manager == null)
					return objectInstances;

				bool validateRecycleBackup = manager.ValidateRecycledTypes;
				IContainer containerBackup = manager.Container;

				if (container != null)
					manager.Container = container;

				using (IDisposable session = manager.CreateSession ()) {
					((IDesignerSerializationManager) manager).ResolveName += OnResolveInstance;
					foreach (ObjectEntry entry in _objects.Values)
						objectInstances.Add (DeserializeEntry (manager, entry));
					((IDesignerSerializationManager) manager).ResolveName -= OnResolveInstance;
					_errors = manager.Errors;
				}

				manager.ValidateRecycledTypes = validateRecycleBackup;
				manager.Container = containerBackup;
				return objectInstances;
			}

			private void OnResolveInstance (object sender, ResolveNameEventArgs args)
			{
				if (args.Value == null && _objects != null && _objects.ContainsKey (args.Name)) {
					IDesignerSerializationManager manager = _provider.GetService 
						(typeof (IDesignerSerializationManager)) as IDesignerSerializationManager;
					if (manager != null) {
						ObjectEntry entry = _objects[args.Name];
						if (entry.EntireObject)
							args.Value = DeserializeEntry (manager, entry);
					}
				}
			}

			private object DeserializeEntry (IDesignerSerializationManager manager, ObjectEntry objectEntry)
			{
				object deserialized = null;

				if (objectEntry.EntireObject) {
					if (objectEntry.IsDeserialized) {
						deserialized = objectEntry.Deserialized;
					} else {
						CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer (objectEntry.Type, 
															  typeof (CodeDomSerializer));
						if (serializer != null) {
							deserialized = serializer.Deserialize (manager, objectEntry.Serialized);
							objectEntry.Deserialized = deserialized;
							// check if the name of the object has changed
							// (if it e.g clashes with another name)
							string newName = manager.GetName (deserialized);
							if (newName != objectEntry.Name)
								objectEntry.Name = newName;
						}
					}
				} else {
					foreach (MemberEntry memberEntry in objectEntry.Members.Values) {
						if (!memberEntry.IsDeserialized) {
							CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer (objectEntry.Type, 
																  typeof (CodeDomSerializer));
							if (serializer != null) {
								serializer.Deserialize (manager, memberEntry.Serialized);
								memberEntry.IsDeserialized = true;
							}
						}
					}
				}

				return deserialized;
			}

			public override ICollection Errors {
				get {
					if (_errors == null)
						_errors = new object[0];
					return _errors;
				}
			}
		} // CodeDomSerializationStore

		private IServiceProvider _provider;

		public CodeDomComponentSerializationService () : this (null)
		{
		}

		public CodeDomComponentSerializationService (IServiceProvider provider)
		{
			_provider = provider;
		}

		public override SerializationStore CreateStore ()
		{
			return new CodeDomSerializationStore (_provider);
		}

		public override SerializationStore LoadStore (Stream stream)
		{
			return CodeDomSerializationStore.Load (stream);
		}

		public override ICollection Deserialize (SerializationStore store)
		{
			return this.Deserialize (store, null);
		}

		public override ICollection Deserialize (SerializationStore store, IContainer container)
		{
			return DeserializeCore (store, container, true, true);
		}

		public override void DeserializeTo (SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults)
		{
			DeserializeCore (store, container, validateRecycledTypes, applyDefaults);
		}

		private ICollection DeserializeCore (SerializationStore store, IContainer container, bool validateRecycledTypes, 
						     bool applyDefaults)
		{
			CodeDomSerializationStore codeDomStore = store as CodeDomSerializationStore;
			if (codeDomStore == null)
				throw new InvalidOperationException ("store type unsupported");
			return codeDomStore.Deserialize (_provider, container, validateRecycledTypes, applyDefaults);
		}

		public override void Serialize (SerializationStore store, object value)
		{
			SerializeCore (store, value, false);
		}

		public override void SerializeAbsolute (SerializationStore store, object value)
		{
			SerializeCore (store, value, true);
		}

		public override void SerializeMember (SerializationStore store, object owningObject, MemberDescriptor member)
		{
			SerializeMemberCore (store, owningObject, member, false);
		}

		public override void SerializeMemberAbsolute (SerializationStore store, object owningObject, MemberDescriptor member)
		{
			SerializeMemberCore (store, owningObject, member, true);
		}

		private void SerializeCore (SerializationStore store, object value, bool absolute)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (store == null)
				throw new ArgumentNullException ("store");

			CodeDomSerializationStore codeDomStore = store as CodeDomSerializationStore;
			if (store == null)
				throw new InvalidOperationException ("store type unsupported");

			codeDomStore.AddObject (value, absolute);
		}

		private void SerializeMemberCore (SerializationStore store, object owningObject, MemberDescriptor member, bool absolute)
		{
			if (member == null)
				throw new ArgumentNullException ("member");
			if (owningObject == null)
				throw new ArgumentNullException ("owningObject");
			if (store == null)
				throw new ArgumentNullException ("store");

			CodeDomSerializationStore codeDomStore = store as CodeDomSerializationStore;
			if (codeDomStore == null)
				throw new InvalidOperationException ("store type unsupported");
			codeDomStore.AddMember (owningObject, member, absolute);
		}
	}
}
#endif
