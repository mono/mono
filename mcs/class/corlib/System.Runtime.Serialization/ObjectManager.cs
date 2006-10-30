//
// System.Runtime.Serialization.ObjectManager.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003 Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

namespace System.Runtime.Serialization
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
	public class ObjectManager
	{
		// All objects are chained in the same order as they have been registered
		ObjectRecord _objectRecordChain = null;
		ObjectRecord _lastObjectRecord = null;

		ArrayList _deserializedRecords = new ArrayList();
#if NET_2_0
		ArrayList _onDeserializedCallbackRecords = new ArrayList();
#endif
		Hashtable _objectRecords = new Hashtable();
		bool _finalFixup = false;

		ISurrogateSelector _selector;
		StreamingContext _context;
		int _registeredObjectsCount = 0;

		public ObjectManager(ISurrogateSelector selector, StreamingContext context)
		{
			_selector = selector;
			_context = context;
		}

		public virtual void DoFixups()
		{
			_finalFixup = true;

			try
			{
				if (_registeredObjectsCount < _objectRecords.Count)
					throw new SerializationException ("There are some fixups that refer to objects that have not been registered");


				ObjectRecord last = _lastObjectRecord;
				bool firstCicle = true;

				// Solve al pending fixups of all objects

				ObjectRecord record = _objectRecordChain;
				while (record != null)
				{
					bool ready = !(record.IsUnsolvedObjectReference && firstCicle);
					if (ready) ready = record.DoFixups (true, this, true);
					if (ready) ready = record.LoadData(this, _selector, _context);

					ObjectRecord next;

					if (ready)
					{
						if (record.OriginalObject is IDeserializationCallback)
							_deserializedRecords.Add (record);

#if NET_2_0
						SerializationCallbacks sc = SerializationCallbacks
							.GetSerializationCallbacks (record.OriginalObject.GetType ());
						if (sc.HasDeserializedCallbacks)
							_onDeserializedCallbackRecords.Add (record);
#endif
						next = record.Next;
					}
					else
					{
						// There must be an unresolved IObjectReference instance.
						// Chain the record at the end so it is solved later

						if ((record.ObjectInstance is IObjectReference) && !firstCicle)
						{
							if (record.Status == ObjectRecordStatus.ReferenceSolvingDelayed)
								throw new SerializationException ("The object with ID " + record.ObjectID + " could not be resolved");
							else
								record.Status = ObjectRecordStatus.ReferenceSolvingDelayed;
						}

						if (record != _lastObjectRecord) {
							next = record.Next;
							record.Next = null;
							_lastObjectRecord.Next = record;
							_lastObjectRecord = record;
						}
						else
							next = record;
					}

					if (record == last) firstCicle = false;
					record = next;
				}
			}
			finally
			{
				_finalFixup = false;
			}
		}

		internal ObjectRecord GetObjectRecord (long objectID)
		{
			ObjectRecord rec = (ObjectRecord)_objectRecords[objectID];
			if (rec == null)
			{
				if (_finalFixup) throw new SerializationException ("The object with Id " + objectID + " has not been registered");
				rec = new ObjectRecord();
				rec.ObjectID = objectID;
				_objectRecords[objectID] = rec;
			}
			if (!rec.IsRegistered && _finalFixup) throw new SerializationException ("The object with Id " + objectID + " has not been registered");
			return rec;
		}

		public virtual object GetObject (long objectID)
		{
			if (objectID <= 0) throw new ArgumentOutOfRangeException("objectID","The objectID parameter is less than or equal to zero");
			ObjectRecord rec = (ObjectRecord)_objectRecords[objectID];
			if (rec == null || !rec.IsRegistered) return null;
			else return rec.ObjectInstance;
		}

		public virtual void RaiseDeserializationEvent ()
		{
#if NET_2_0
			for (int i = _onDeserializedCallbackRecords.Count-1; i >= 0; i--)
			{
				ObjectRecord record = (ObjectRecord) _onDeserializedCallbackRecords [i];
				RaiseOnDeserializedEvent (record.OriginalObject);
			}
#endif
			for (int i = _deserializedRecords.Count-1; i >= 0; i--)
			{
				ObjectRecord record = (ObjectRecord) _deserializedRecords [i];
				IDeserializationCallback obj = record.OriginalObject as IDeserializationCallback;
				if (obj != null) obj.OnDeserialization (this);
			}

		}

#if NET_2_0
		public void RaiseOnDeserializingEvent (object obj)
		{
			SerializationCallbacks sc = SerializationCallbacks
				.GetSerializationCallbacks (obj.GetType ());
			sc.RaiseOnDeserializing (obj, _context);
		}

		void RaiseOnDeserializedEvent (object obj)
		{
			SerializationCallbacks sc = SerializationCallbacks
				.GetSerializationCallbacks (obj.GetType ());
			sc.RaiseOnDeserialized (obj, _context);
		}
#endif

		private void AddFixup (BaseFixupRecord record)
		{
			record.ObjectToBeFixed.ChainFixup (record, true);
			record.ObjectRequired.ChainFixup (record, false);
		}

		public virtual void RecordArrayElementFixup (long arrayToBeFixed, int index, long objectRequired)
		{
			if (arrayToBeFixed <= 0) throw new ArgumentOutOfRangeException("arrayToBeFixed","The arrayToBeFixed parameter is less than or equal to zero");
			if (objectRequired <= 0) throw new ArgumentOutOfRangeException("objectRequired","The objectRequired parameter is less than or equal to zero");
			ArrayFixupRecord record = new ArrayFixupRecord(GetObjectRecord(arrayToBeFixed), index, GetObjectRecord(objectRequired));
			AddFixup (record);
		}

		public virtual void RecordArrayElementFixup (long arrayToBeFixed, int[] indices, long objectRequired)
		{
			if (arrayToBeFixed <= 0) throw new ArgumentOutOfRangeException("arrayToBeFixed","The arrayToBeFixed parameter is less than or equal to zero");
			if (objectRequired <= 0) throw new ArgumentOutOfRangeException("objectRequired","The objectRequired parameter is less than or equal to zero");
			if (indices == null) throw new ArgumentNullException("indices");
			MultiArrayFixupRecord record = new MultiArrayFixupRecord (GetObjectRecord(arrayToBeFixed), indices, GetObjectRecord(objectRequired));
			AddFixup (record);
		}

		public virtual void RecordDelayedFixup (long objectToBeFixed, string memberName, long objectRequired)
		{
			if (objectToBeFixed <= 0) throw new ArgumentOutOfRangeException("objectToBeFixed","The objectToBeFixed parameter is less than or equal to zero");
			if (objectRequired <= 0) throw new ArgumentOutOfRangeException("objectRequired","The objectRequired parameter is less than or equal to zero");
			if (memberName == null) throw new ArgumentNullException("memberName");
			DelayedFixupRecord record = new DelayedFixupRecord (GetObjectRecord(objectToBeFixed), memberName, GetObjectRecord(objectRequired));
			AddFixup (record);
		}

		public virtual void RecordFixup (long objectToBeFixed, MemberInfo member, long objectRequired)
		{
			if (objectToBeFixed <= 0) throw new ArgumentOutOfRangeException("objectToBeFixed","The objectToBeFixed parameter is less than or equal to zero");
			if (objectRequired <= 0) throw new ArgumentOutOfRangeException("objectRequired","The objectRequired parameter is less than or equal to zero");
			if (member == null) throw new ArgumentNullException("member");
			FixupRecord record = new FixupRecord (GetObjectRecord(objectToBeFixed), member, GetObjectRecord(objectRequired));
			AddFixup (record);
		}

		private void RegisterObjectInternal (object obj, ObjectRecord record)
		{
			if (obj == null) throw new ArgumentNullException("obj");

			if (record.IsRegistered)
			{
				if (record.OriginalObject != obj) throw new SerializationException ("An object with Id " + record.ObjectID + " has already been registered");
				else return;
			}

			record.ObjectInstance = obj;
			record.OriginalObject = obj;

			if (obj is IObjectReference) record.Status = ObjectRecordStatus.ReferenceUnsolved;
			else record.Status = ObjectRecordStatus.ReferenceSolved;

			if (_selector != null) {
				record.Surrogate = _selector.GetSurrogate (
					obj.GetType(), _context, out record.SurrogateSelector);
				if (record.Surrogate != null)
					record.Status = ObjectRecordStatus.ReferenceUnsolved;
			}

			record.DoFixups (true, this, false);
			record.DoFixups (false, this, false);
			_registeredObjectsCount++;

			// Adds the object to the chain of registered objects. This chain
			// is needed to be able to to perform the final fixups in the right order

			if (_objectRecordChain == null)
			{
				_objectRecordChain = record;
				_lastObjectRecord = record;
			}
			else 
			{
				_lastObjectRecord.Next = record;
				_lastObjectRecord = record;
			}
		}


		public virtual void RegisterObject (object obj, long objectID)
		{
			if (obj == null) throw new ArgumentNullException("obj", "The obj parameter is null.");
			if (objectID <= 0) throw new ArgumentOutOfRangeException("objectID","The objectID parameter is less than or equal to zero");
			RegisterObjectInternal (obj, GetObjectRecord (objectID));
		}

		public void RegisterObject (object obj, long objectID, SerializationInfo info)
		{
			if (obj == null) throw new ArgumentNullException("obj", "The obj parameter is null.");
			if (objectID <= 0) throw new ArgumentOutOfRangeException("objectID","The objectID parameter is less than or equal to zero");
			
			ObjectRecord record = GetObjectRecord (objectID);
			record.Info = info;
			RegisterObjectInternal (obj, record);
		}

		public void RegisterObject (object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
		{
			RegisterObject (obj, objectID, info, idOfContainingObj, member, null);
		}

		public void RegisterObject( object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member, int[] arrayIndex)
		{
			if (obj == null) throw new ArgumentNullException("obj", "The obj parameter is null.");
			if (objectID <= 0) throw new ArgumentOutOfRangeException("objectID","The objectID parameter is less than or equal to zero");

			ObjectRecord record = GetObjectRecord (objectID);
			record.Info = info;
			record.IdOfContainingObj = idOfContainingObj;
			record.Member = member;
			record.ArrayIndex = arrayIndex;
			RegisterObjectInternal (obj, record);
		}
	}



	// Fixup types. There is a fixup class for each fixup type.

	// BaseFixupRecord
	// Base class for all fixups

	internal abstract class BaseFixupRecord
	{
		public BaseFixupRecord(ObjectRecord objectToBeFixed, ObjectRecord objectRequired)
		{
			ObjectToBeFixed = objectToBeFixed;
			ObjectRequired = objectRequired;
		}

		public bool DoFixup (ObjectManager manager, bool strict)
		{
			if (ObjectToBeFixed.IsRegistered && ObjectRequired.IsInstanceReady)
			{
				FixupImpl (manager);
				return true;
			}
			else if (strict)
			{
				if (!ObjectToBeFixed.IsRegistered) throw new SerializationException ("An object with ID " + ObjectToBeFixed.ObjectID + " was included in a fixup, but it has not been registered");
				else if (!ObjectRequired.IsRegistered) throw new SerializationException ("An object with ID " + ObjectRequired.ObjectID + " was included in a fixup, but it has not been registered");
				else return false;
			}
			else
				return false;
		}

		protected abstract void FixupImpl (ObjectManager manager);

		internal protected ObjectRecord ObjectToBeFixed;
		internal protected ObjectRecord ObjectRequired;

		public BaseFixupRecord NextSameContainer;
		public BaseFixupRecord NextSameRequired;
	}

	// ArrayFixupRecord
	// Fixup for assigning a value to one position of an array

	internal class ArrayFixupRecord : BaseFixupRecord
	{
		int _index;

		public ArrayFixupRecord (ObjectRecord objectToBeFixed, int index, ObjectRecord objectRequired): base (objectToBeFixed, objectRequired) {
			_index = index;
		}

		protected override void FixupImpl (ObjectManager manager) {
			Array array = (Array)ObjectToBeFixed.ObjectInstance;
			array.SetValue (ObjectRequired.ObjectInstance, _index);
		}
	}

	// MultiArrayFixupRecord
	// Fixup for assigning a value to several positions of an array

	internal class MultiArrayFixupRecord : BaseFixupRecord
	{
		int[] _indices;

		public MultiArrayFixupRecord (ObjectRecord objectToBeFixed, int[] indices, ObjectRecord objectRequired): base (objectToBeFixed, objectRequired) {
			_indices = indices;
		}

		protected override void FixupImpl (ObjectManager manager) {
			ObjectToBeFixed.SetArrayValue (manager, ObjectRequired.ObjectInstance, _indices);
		}
	}

	// FixupRecord
	// Fixup for assigning a value to a member of an object

	internal class FixupRecord: BaseFixupRecord
	{
		public MemberInfo _member;

		public FixupRecord (ObjectRecord objectToBeFixed, MemberInfo member, ObjectRecord objectRequired): base (objectToBeFixed, objectRequired) {
			_member = member;
		}

		protected override void FixupImpl (ObjectManager manager) {
			ObjectToBeFixed.SetMemberValue (manager, _member, ObjectRequired.ObjectInstance);
		}
	}

	// DelayedFixupRecord
	// Fixup for assigning a value to a SerializationInfo of an object

	internal class DelayedFixupRecord: BaseFixupRecord
	{
		public string _memberName;

		public DelayedFixupRecord (ObjectRecord objectToBeFixed, string memberName, ObjectRecord objectRequired): base (objectToBeFixed, objectRequired) {
			_memberName = memberName;
		}

		protected override void FixupImpl (ObjectManager manager) {
			ObjectToBeFixed.SetMemberValue (manager, _memberName, ObjectRequired.ObjectInstance);
		}
	}

	// Object Record

	internal enum ObjectRecordStatus: byte { Unregistered, ReferenceUnsolved, ReferenceSolvingDelayed, ReferenceSolved }

	internal class ObjectRecord
	{
		public ObjectRecordStatus Status = ObjectRecordStatus.Unregistered;
		public object OriginalObject;
		public object ObjectInstance;
		public long ObjectID;
		public SerializationInfo Info;
		public long IdOfContainingObj;
		public ISerializationSurrogate Surrogate;
		public ISurrogateSelector SurrogateSelector;
		public MemberInfo Member;
		public int[] ArrayIndex;
		public BaseFixupRecord FixupChainAsContainer;
		public BaseFixupRecord FixupChainAsRequired;
		public ObjectRecord Next;

		public void SetMemberValue (ObjectManager manager, MemberInfo member, object value)
		{
			if (member is FieldInfo)
				((FieldInfo)member).SetValue (ObjectInstance, value);
			else if (member is PropertyInfo)
				((PropertyInfo)member).SetValue (ObjectInstance, value, null);
			else throw new SerializationException ("Cannot perform fixup");

			if (Member != null)
			{
				ObjectRecord containerRecord = manager.GetObjectRecord (IdOfContainingObj);
				if (containerRecord.IsRegistered)
					containerRecord.SetMemberValue (manager, Member, ObjectInstance);
			}
			else if (ArrayIndex != null)
			{
				ObjectRecord containerRecord = manager.GetObjectRecord (IdOfContainingObj);
				if (containerRecord.IsRegistered)
					containerRecord.SetArrayValue (manager, ObjectInstance, ArrayIndex);
			}
		}
		public void SetArrayValue (ObjectManager manager, object value, int[] indices)
		{
			((Array)ObjectInstance).SetValue (value, indices);
		}

		public void SetMemberValue (ObjectManager manager, string memberName, object value)
		{
			if (Info == null) throw new SerializationException ("Cannot perform fixup");
			Info.AddValue (memberName, value, value.GetType());
		}

		public bool IsInstanceReady
		{
			// Returns true if this object is ready to be assigned to a parent object.
			get
			{
				if (!IsRegistered) return false;
				if (IsUnsolvedObjectReference) return false;

				// Embedded value objects cannot be assigned to their containers until fully completed
				if (ObjectInstance.GetType ().IsValueType && (HasPendingFixups || Info != null))
					return false;

				return true;
			}
		}

		public bool IsUnsolvedObjectReference
		{
			get  {
				return Status != ObjectRecordStatus.ReferenceSolved;
			}
		}

		public bool IsRegistered
		{
			get {
				return Status != ObjectRecordStatus.Unregistered;
			}
		}

		public bool DoFixups (bool asContainer, ObjectManager manager, bool strict)
		{
			BaseFixupRecord prevFixup = null;
			BaseFixupRecord fixup = asContainer ? FixupChainAsContainer : FixupChainAsRequired;
			bool allFixed = true;

			while (fixup != null)
			{
				if (fixup.DoFixup (manager, strict))
				{
					UnchainFixup (fixup, prevFixup, asContainer);
					if (asContainer) fixup.ObjectRequired.RemoveFixup (fixup, false);
					else fixup.ObjectToBeFixed.RemoveFixup (fixup, true);
				}
				else
				{
					prevFixup = fixup;
					allFixed = false;
				}

				fixup = asContainer ? fixup.NextSameContainer : fixup.NextSameRequired;
			}
			return allFixed;
		}

		public void RemoveFixup (BaseFixupRecord fixupToRemove, bool asContainer)
		{
			BaseFixupRecord prevFixup = null;
			BaseFixupRecord fixup = asContainer ? FixupChainAsContainer : FixupChainAsRequired;
			while (fixup != null)
			{
				if (fixup == fixupToRemove) 
				{
					UnchainFixup (fixup, prevFixup, asContainer);
					return;
				}
				prevFixup = fixup;
				fixup = asContainer ? fixup.NextSameContainer : fixup.NextSameRequired;
			}
		}

		private void UnchainFixup (BaseFixupRecord fixup, BaseFixupRecord prevFixup, bool asContainer)
		{
			if (prevFixup == null) {
				if (asContainer) FixupChainAsContainer = fixup.NextSameContainer;
				else FixupChainAsRequired = fixup.NextSameRequired;
			}
			else {
				if (asContainer) prevFixup.NextSameContainer = fixup.NextSameContainer;
				else prevFixup.NextSameRequired = fixup.NextSameRequired;
			}
		}

		public void ChainFixup (BaseFixupRecord fixup, bool asContainer)
		{
			if (asContainer) 
			{
				fixup.NextSameContainer = FixupChainAsContainer;
				FixupChainAsContainer = fixup;
			}
			else 
			{
				fixup.NextSameRequired = FixupChainAsRequired;
				FixupChainAsRequired = fixup;
			}
		}

		public bool LoadData (ObjectManager manager, ISurrogateSelector selector, StreamingContext context)
		{
			if (Info != null)
			{
				if (Surrogate != null) {
					object new_obj = Surrogate.SetObjectData (ObjectInstance, Info, context, SurrogateSelector);
					if (new_obj != null)
						ObjectInstance = new_obj;
					Status = ObjectRecordStatus.ReferenceSolved;
				} else if (ObjectInstance is ISerializable) {
					object[] pars = new object[] {Info, context};
					ConstructorInfo con = ObjectInstance.GetType().GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof (SerializationInfo), typeof (StreamingContext) }, null );
					if (con == null) throw new SerializationException ("The constructor to deserialize an object of type " + ObjectInstance.GetType().FullName + " was not found.");
					con.Invoke (ObjectInstance, pars);
				} else {
					throw new SerializationException ("No surrogate selector was found for type " + ObjectInstance.GetType().FullName);
				}

				Info = null;
			}

			if (ObjectInstance is IObjectReference && Status != ObjectRecordStatus.ReferenceSolved)
			{
				try {
					ObjectInstance = ((IObjectReference)ObjectInstance).GetRealObject(context);
					int n = 100;
					while (ObjectInstance is IObjectReference && n > 0) {
						object ob = ((IObjectReference)ObjectInstance).GetRealObject (context);
						if (ob == ObjectInstance)
							break;
						ObjectInstance = ob;
						n--;
					}
					if (n == 0)
						throw new SerializationException ("The implementation of the IObjectReference interface returns too many nested references to other objects that implement IObjectReference.");
					
					Status = ObjectRecordStatus.ReferenceSolved;
				}
				catch (NullReferenceException) {
					// Give a second chance
					return false;
				}
			}

			if (Member != null)
			{
				// If this object is a value object embedded in another object, the parent
				// object must be updated

				ObjectRecord containerRecord = manager.GetObjectRecord (IdOfContainingObj);
				containerRecord.SetMemberValue (manager, Member, ObjectInstance);
			}
			else if (ArrayIndex != null)
			{
				ObjectRecord containerRecord = manager.GetObjectRecord (IdOfContainingObj);
				containerRecord.SetArrayValue (manager, ObjectInstance, ArrayIndex);
			}

			return true;
		}

		public bool HasPendingFixups
		{
			get { return FixupChainAsContainer != null; }
		}
	}
}
