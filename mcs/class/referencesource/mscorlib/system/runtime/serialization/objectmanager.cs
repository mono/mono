// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: ObjectManager
**
**
** Purpose: 
**
**
============================================================*/
namespace System.Runtime.Serialization {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Security;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    using System.Security.Principal;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class ObjectManager {
        private const int DefaultInitialSize=16;
        private const int MaxArraySize=0x1000; //MUST BE A POWER OF 2!
        private const int ArrayMask = MaxArraySize-1;
        private const int MaxReferenceDepth = 100;
        
        private DeserializationEventHandler m_onDeserializationHandler;
        private SerializationEventHandler m_onDeserializedHandler;

#if !FEATURE_PAL
        private static RuntimeType TypeOfWindowsIdentity;
#endif    
        internal ObjectHolder []    m_objects;
        internal Object m_topObject = null;
        internal ObjectHolderList   m_specialFixupObjects; //This is IObjectReference, ISerializable, or has a Surrogate.
        internal long               m_fixupCount;
        internal ISurrogateSelector m_selector;
        internal StreamingContext   m_context;
        bool m_isCrossAppDomain;
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public ObjectManager(ISurrogateSelector selector, StreamingContext context) : this(selector, context, true, false) {
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal ObjectManager(ISurrogateSelector selector, StreamingContext context, bool checkSecurity, bool isCrossAppDomain) {
            if (checkSecurity) {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);          
            }
            m_objects = new ObjectHolder[DefaultInitialSize];
            m_selector = selector;
            m_context = context;
            m_isCrossAppDomain = isCrossAppDomain;
        }

    
        [System.Security.SecurityCritical]  // auto-generated
        private bool CanCallGetType(Object obj) {
#if FEATURE_REMOTING                        
            if (RemotingServices.IsTransparentProxy(obj)) {
                return false;
            }
#endif            
            return true;
        }

        internal Object TopObject {
            set {
                m_topObject = value;
            }
            get {
                return m_topObject;
            }
        }

        internal ObjectHolderList SpecialFixupObjects {
            get {
                if (m_specialFixupObjects==null) {
                    m_specialFixupObjects = new ObjectHolderList();
                }
                return m_specialFixupObjects;
            }
        }
    
        static ObjectManager() {
#if !FEATURE_PAL && FEATURE_IMPERSONATION
            TypeOfWindowsIdentity = (RuntimeType)typeof(WindowsIdentity);
#endif
        }

        /*==================================FindObject==================================
        **Action: An internal-only function to find the object with id <CODE>objectID</CODE>.
        **This function does no error checking, it assumes that all of that has been done already.
        **Returns: The ObjectHolder for <CODE>objectID</CODE> or null if it doesn't exist.
        **Arguments: objectID -- The objectID of the Object for which we're searching.
        **Exceptions: None.  This is internal only.  
        **Callers should verify that objectID is greater than 0.
        ==============================================================================*/
        internal ObjectHolder FindObjectHolder(long objectID) {
            Contract.Assert(objectID>0,"objectID>0");
    
            //The  index of the bin in which we live is rightmost n bits of the objectID.
            int index = (int)(objectID & ArrayMask);
            if (index>=m_objects.Length) {
                return null;
            }
    
            //Find the bin in which we live.
            ObjectHolder temp = m_objects[index]; 
    
            //Walk the chain in that bin.  Return the ObjectHolder if we find it, otherwise
            //return null.
            while (temp!=null) { 
                if (temp.m_id==objectID) {
                    return temp;
                }
                temp = temp.m_next;
            }
            return temp;
        }


        internal ObjectHolder FindOrCreateObjectHolder(long objectID) {
            ObjectHolder holder;
            holder = FindObjectHolder(objectID);
            if (holder==null) {
                holder = new ObjectHolder(objectID);
                AddObjectHolder(holder);
            }
            return holder;
        }

    
        /*===============================AddObjectHolder================================
        **Action: Add the provided ObjectHolder to collection of ObjectHolders. 
        **        Enlarges the collection as appropriate.
        **Returns: void
        **Arguments: holder The ObjectHolder to be added.
        **Exceptions: Internal only.  Caller should verify that <CODE>holder</CODE> is 
        **            not null.
        ==============================================================================*/
        private void AddObjectHolder(ObjectHolder holder) {
    
            Contract.Assert(holder!=null,"holder!=null");
            BCLDebug.Trace("SER", "[AddObjectHolder]Adding ObjectHolder with id: ", holder.m_id, " Current Bins: ", m_objects.Length);
            Contract.Assert(holder.m_id>=0,"holder.m_id>=0");

            //If the id that we need to place is greater than our current length, and less
            //than the maximum allowable size of the array.  We need to double the size
            //of the array.  If the array has already reached it's maximum allowable size,
            //we chain elements off of the buckets.
            if (holder.m_id>=m_objects.Length && m_objects.Length != MaxArraySize) {
                int newSize=MaxArraySize;
    
                if (holder.m_id<(MaxArraySize/2)) {
                    newSize = (m_objects.Length * 2);
    
                    //Keep doubling until we're larger than our target size.
                    //We could also do this with log operations, but that would
                    //be slower than the brute force approach.
                    while (newSize<=holder.m_id && newSize<MaxArraySize) {
                        newSize*=2;
                    }
                    
                    if (newSize>MaxArraySize) {
                        newSize=MaxArraySize;
                    }
                }
    
                BCLDebug.Trace("SER", "[AddObjectHolder]Reallocating m_objects to have ", newSize, " bins");
                ObjectHolder[] temp = new ObjectHolder[newSize];
                Array.Copy(m_objects, temp, m_objects.Length);
                m_objects = temp;
            }
                
            //Find the bin in which we live and make this new element the first element in the bin.
            int index = (int)(holder.m_id & ArrayMask);
            BCLDebug.Trace("SER", "[AddObjectHolder]Trying to put an object in bin ", index);
    
            ObjectHolder tempHolder = m_objects[index]; 
            holder.m_next = tempHolder;
            m_objects[index] = holder;
        }
    
        private bool GetCompletionInfo(FixupHolder fixup, out ObjectHolder holder, out Object member, bool bThrowIfMissing) {
    
            //Set the member id (String or MemberInfo) for the member being fixed up.
            member = fixup.m_fixupInfo;
    
            //Find the object required for the fixup.  Throw if we can't find it.
            holder = FindObjectHolder(fixup.m_id);
            BCLDebug.Trace("SER", "[ObjectManager.GetCompletionInfo]Getting fixup info for: ", fixup.m_id);

            // CompletelyFixed is our poorly named property which indicates if something requires a SerializationInfo fixup
            // or is an incomplete object reference.  We have this particular branch to handle valuetypes which implement
            // ISerializable.  In that case, we can't do any fixups on them later, so we need to delay the fixups further.
            if (!holder.CompletelyFixed) {
                if (holder.ObjectValue!=null && holder.ObjectValue is ValueType) {
                    BCLDebug.Trace("SER", "[ObjectManager.GetCompletionInfo]ValueType implementing ISerializable.  Delaying fixup.");
                    SpecialFixupObjects.Add(holder);
                    return false;
                }
            }

            if (holder==null || holder.CanObjectValueChange || holder.ObjectValue==null) {
                if (bThrowIfMissing) {
                    BCLDebug.Trace("SER", "[GetCompletionInfo]Unable to find fixup for: ", fixup.m_id);
                    BCLDebug.Trace("SER", "[GetCompletionInfo]Holder: ", ((holder==null)?"<null>":"Non Null"));
                    BCLDebug.Trace("SER", "[GetCompletionInfo]IsIncomplete: ", (holder.IsIncompleteObjectReference));
                    BCLDebug.Trace("SER", "[GetCompletionInfo]Object: ", ((holder.ObjectValue==null)?"<null>":"Non Null"));
                    if (holder==null) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NeverSeen", fixup.m_id));
                    }
                    if (holder.IsIncompleteObjectReference) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_IORIncomplete", fixup.m_id));
                    }
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectNotSupplied", fixup.m_id));
                }
                return false;
            }
            return true;
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        private void FixupSpecialObject(ObjectHolder holder) {
            ISurrogateSelector uselessSelector=null;

            Contract.Assert(holder.RequiresSerInfoFixup,"[ObjectManager.FixupSpecialObject]holder.HasSurrogate||holder.HasISerializable");
            if (holder.HasSurrogate) {
                ISerializationSurrogate surrogate = holder.Surrogate;
                Contract.Assert(surrogate!=null,"surrogate!=null");
                object returnValue = surrogate.SetObjectData(holder.ObjectValue, holder.SerializationInfo, m_context, uselessSelector);
                if (returnValue != null)
                {
                    if (!holder.CanSurrogatedObjectValueChange && returnValue != holder.ObjectValue)
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NotCyclicallyReferenceableSurrogate"), surrogate.GetType().FullName));
                    holder.SetObjectValue(returnValue, this);
                }
                holder.m_surrogate = null;
                holder.SetFlags();
            } else {
                //Set the object data 
                Contract.Assert(holder.ObjectValue is ISerializable,"holder.m_object is ISerializable");
                BCLDebug.Trace("SER","[ObjectManager.FixupSpecialObject]Fixing up ISerializable object ",holder.ObjectValue," with id ",holder.m_id);
                CompleteISerializableObject(holder.ObjectValue, holder.SerializationInfo, m_context);
            }
            //Clear anything that we know that we're not going to need.
            holder.SerializationInfo=null;
            holder.RequiresSerInfoFixup = false;

            // For value types, fixups would have been done. So the newly fixed object must be copied
            // to its container.
            if (holder.RequiresValueTypeFixup && holder.ValueTypeFixupPerformed){
                DoValueTypeFixup(null, holder, holder.ObjectValue);
            }
            DoNewlyRegisteredObjectFixups(holder);
        }
        

        /*============================ResolveObjectReference============================
        **Action:Unfortunately, an ObjectReference could actually be a reference to another
        **       object reference and we don't know how far we have to tunnel until we can find the real object.  While
        **       we're still getting instances of IObjectReference back and we're still getting new objects, keep calling
        **       GetRealObject.  Once we've got the new object, take care of all of the fixups
        **       that we can do now that we've got it.
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        private bool ResolveObjectReference(ObjectHolder holder) {
            Object tempObject;
            Contract.Assert(holder.IsIncompleteObjectReference,"holder.IsIncompleteObjectReference");

            //In the pathological case, an Object implementing IObjectReference could return a reference
            //to a different object which implements IObjectReference.  This makes us vulnerable to a 
            //denial of service attack and stack overflow.  If the depthCount becomes greater than
            //MaxReferenceDepth, we'll throw a SerializationException.
            int depthCount = 0;
            
            //We wrap this in a try/catch block to handle the case where we're trying to resolve a chained
            //list of object reference (e.g. an IObjectReference can't resolve itself without some information
            //that's currently missing from the graph).  We'll catch the NullReferenceException and come back
            //and try again later.  The downside of this scheme is that if the object actually needed to throw
            //a NullReferenceException, it's being caught and turned into a SerializationException with a
            //fairly cryptic message.
            try {
                do {
                    tempObject = holder.ObjectValue;
                    BCLDebug.Trace("SER", "[ResolveObjectReference]ID: ", holder.m_id);
                    BCLDebug.Trace("SER", "[ResolveObjectReference]HasISerializable: ", holder.HasISerializable);
                    holder.SetObjectValue(((IObjectReference)(holder.ObjectValue)).GetRealObject(m_context), this);
                    //The object didn't yet have enough information to resolve the reference, so we'll
                    //return false and the graph walker should call us back again after more objects have
                    //been resolved.
                    //<

                    if (holder.ObjectValue==null) {
                        holder.SetObjectValue(tempObject, this);
                        BCLDebug.Trace("SER", "Object: ", holder.m_id, " did NOT have enough information to resolve the IObjectReference.");
                        return false;
                    }
                    if (depthCount++==MaxReferenceDepth) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_TooManyReferences"));
                    }
                } while ((holder.ObjectValue is IObjectReference) && (tempObject!=holder.ObjectValue));
            } catch (NullReferenceException) {
                BCLDebug.Trace("SER", "[ResolveObjectReference]Caught exception trying to call GetRealObject.");
                return false;
            }
    
            BCLDebug.Trace("SER", "Object: ", holder.m_id, " resolved the IObjectReference.");
            holder.IsIncompleteObjectReference=false;
            DoNewlyRegisteredObjectFixups(holder);
            return true;
        }


        /*===============================DoValueTypeFixup===============================
        **Action:
        **Returns:
        **Arguments:
        ** memberToFix -- the member in the object contained in holder being fixed up.
        ** holder -- the ObjectHolder for the object (a value type in this case) being completed.
        ** value  -- the data to set into the field.
        **Exceptions:
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        private bool DoValueTypeFixup(FieldInfo memberToFix, ObjectHolder holder, Object value) {
            TypedReference typedRef;
            FieldInfo[] fieldsTemp=new FieldInfo[4]; 
            FieldInfo[] fields=null;
            int   currentFieldIndex=0;
            int[] arrayIndex = null;
            ValueTypeFixupInfo currFixup=null;
            Object fixupObj=holder.ObjectValue;
            ObjectHolder originalHolder = holder;

            Contract.Assert(holder!=null, "[TypedReferenceBuilder.ctor]holder!=null");
            Contract.Assert(holder.RequiresValueTypeFixup, "[TypedReferenceBuilder.ctor]holder.RequiresValueTypeFixup");
            
            //In order to get a TypedReference, we need to get a list of all of the FieldInfos to 
            //create the path from our outermost containing object down to the actual field which
            //we'd like to set.  This loop is used to build up that list.
            while (holder.RequiresValueTypeFixup) {
                BCLDebug.Trace("SER", "[DoValueTypeFixup] valueType fixsite = ", holder.ObjectValue, " fixobj=",value);

                //Enlarge the array if required (this is actually fairly unlikely as it would require that we
                //be nested more than 4 deep.
                if ((currentFieldIndex + 1)>=fieldsTemp.Length) {
                    FieldInfo[] temp = new FieldInfo[fieldsTemp.Length * 2];
                    Array.Copy(fieldsTemp, temp, fieldsTemp.Length);
                    fieldsTemp = temp;
                }

                //Get the fixup information.  If we have data for our parent field, add it to our list
                //and continue the walk up to find the next outermost containing object.  We cache the 
                //object that we have.  In most cases, we could have just grabbed it after this loop finished.
                //However, if the outermost containing object is an array, we need the object one further
                //down the chain, so we have to do a lot of caching.
                currFixup = holder.ValueFixup;
                fixupObj = holder.ObjectValue;  //Save the most derived 
                if (currFixup.ParentField!=null) {
                    FieldInfo parentField = currFixup.ParentField;
                    
                    ObjectHolder tempHolder = FindObjectHolder(currFixup.ContainerID);
                    if (tempHolder.ObjectValue == null) {
                        break;
                    }
                    if (Nullable.GetUnderlyingType(parentField.FieldType) != null)
                    {
                        fieldsTemp[currentFieldIndex] = parentField.FieldType.GetField("value", BindingFlags.NonPublic|BindingFlags.Instance);
                        currentFieldIndex++;
                    }

                    fieldsTemp[currentFieldIndex] = parentField;
                    holder = tempHolder;
                    currentFieldIndex++;
                } else {
                    //If we find an index into an array, save that information.
                    Contract.Assert(currFixup.ParentIndex!=null, "[ObjectManager.DoValueTypeFixup]currFixup.ParentIndex!=null");
                    holder = FindObjectHolder(currFixup.ContainerID); //find the array to fix.
                    arrayIndex = currFixup.ParentIndex;
                    if (holder.ObjectValue==null) {
                        break;
                    }
                    break;
                }
            }

            //If the outermost container isn't an array, we need to grab it.  Otherwise, we just need to hang onto
            //the boxed object that we already grabbed.  We'll assign the boxed object back into the array as the
            //last step.
            if (!(holder.ObjectValue is Array) && holder.ObjectValue!=null) {
                fixupObj = holder.ObjectValue;
                Contract.Assert(fixupObj!=null, "[ObjectManager.DoValueTypeFixup]FixupObj!=null");
            }
            
#if false
              //We thought that the valuetype had already been placed into it's parent, but when we started
              //walking the track, we discovered a null, so that's clearly impossible.  At this point, revert
              //to just poking it into the most boxed version that we can.
              if (fixupObj==null) {
                  fixupObj = originalHolder.ObjectValue;
                  FormatterServices.SerializationSetValue(memberToFix, fixupObj, value);
                  return true;
              }
#endif

            if (currentFieldIndex!=0) {

                //MakeTypedReference requires an array of exactly the correct size that goes from the outermost object
                //in to the innermost field.  We currently have an array of arbitrary size that goes from the innermost
                //object outwards.  We create an array of the right size and do the copy.
                fields = new FieldInfo[currentFieldIndex];
                for (int i=0; i<currentFieldIndex; i++) {
                    FieldInfo fieldInfo = fieldsTemp[(currentFieldIndex - 1 - i)];
                    SerializationFieldInfo serInfo = fieldInfo as SerializationFieldInfo;
                    fields[i] = serInfo == null ? fieldInfo : serInfo.FieldInfo;
                }
                
                Contract.Assert(fixupObj!=null, "[ObjectManager.DoValueTypeFixup]fixupObj!=null");
                DumpValueTypeFixup(fixupObj, fields, memberToFix, value);
                //Make the TypedReference and use it to set the value.
                typedRef = TypedReference.MakeTypedReference(fixupObj, fields);
                if (memberToFix != null)
                    //((RuntimeFieldInfo)memberToFix).SetValueDirectImpl(value, false, typedRef);
                    ((RuntimeFieldInfo)memberToFix).SetValueDirect(typedRef, value);
                else
                    TypedReference.SetTypedReference(typedRef, value);
            } else if (memberToFix != null){
                DumpValueTypeFixup(fixupObj, null, memberToFix, value);
                FormatterServices.SerializationSetValue(memberToFix, fixupObj, value);
            }
            
            //If we have an array index, it means that our outermost container was an array.  We don't have
            //any way to build a TypedReference into an array, so we'll use the array functions to set the value.
            //<

            if (arrayIndex!=null && holder.ObjectValue!=null) {
                ((Array)(holder.ObjectValue)).SetValue(fixupObj, arrayIndex);
            }
            
            return true;
        }



        
        [Conditional("SER_LOGGING")]
        void DumpValueTypeFixup(object obj, FieldInfo[] intermediateFields, FieldInfo memberToFix, object value){
            System.Text.StringBuilder sb = new System.Text.StringBuilder("  " + obj);
            if (intermediateFields != null)
            for(int i=0;i<intermediateFields.Length;i++){
                sb.Append("."+intermediateFields[i].Name);
            }
            sb.Append("."+memberToFix.Name+"="+value);
            BCLDebug.Trace("SER", sb.ToString());
        }


        /*================================CompleteObject================================
        **Action:
        **Returns:
        **Arguments:
        **Exceptions:
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        internal void CompleteObject(ObjectHolder holder, bool bObjectFullyComplete) {
            FixupHolderList fixups=holder.m_missingElements;
            FixupHolder currentFixup;
            SerializationInfo si;
            Object fixupInfo=null;
            ObjectHolder tempObjectHolder=null;
            int fixupsPerformed=0;
            
            Contract.Assert(holder!=null,"[ObjectManager.CompleteObject]holder.m_object!=null");
            if (holder.ObjectValue==null) {
                throw new SerializationException(Environment.GetResourceString("Serialization_MissingObject", holder.m_id));
            }
    
            if (fixups==null) {
                return;
            }
            //If either one of these conditions is true, we need to update the data in the
            //SerializationInfo before calling SetObjectData.
            if (holder.HasSurrogate || holder.HasISerializable) {
                si = holder.m_serInfo;

                if (si==null) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupDiscovered"));
                }

                BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Complete object ", holder.m_id, " of SI Type: ", si.FullTypeName);
                //Walk each of the fixups and complete the name-value pair in the SerializationInfo.
                if (fixups!=null) {
                    for (int i=0; i<fixups.m_count; i++) {
                        if (fixups.m_values[i]==null) {
                            continue;
                        }
                        Contract.Assert(fixups.m_values[i].m_fixupType==FixupHolder.DelayedFixup,"fixups.m_values[i].m_fixupType==FixupHolder.DelayedFixup");
                        if (GetCompletionInfo(fixups.m_values[i], out tempObjectHolder, out fixupInfo, bObjectFullyComplete)) {
                            //Walk the SerializationInfo and find the member needing completion.  All we have to do
                            //at this point is set the member into the Object
                            BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Updating object ", holder.m_id, " with object ", tempObjectHolder.m_id);
                            Object holderValue = tempObjectHolder.ObjectValue;
                            if (CanCallGetType(holderValue)) {
                                si.UpdateValue((String)fixupInfo, holderValue, holderValue.GetType());
                            } else {
                                si.UpdateValue((String)fixupInfo, holderValue, typeof(MarshalByRefObject));
                            }
                            //Decrement our total number of fixups left to do.
                            fixupsPerformed++;
                            fixups.m_values[i]=null;
                            if (!bObjectFullyComplete) {
                                holder.DecrementFixupsRemaining(this);
                                tempObjectHolder.RemoveDependency(holder.m_id);
                            }
                        }
                    }
                }
              
            } else {
                BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Non-ISerializableObject: ", holder.m_id);
                for (int i=0; i<fixups.m_count; i++) {
                    currentFixup = fixups.m_values[i];
                    if (currentFixup==null) {
                        continue;
                    }
                    BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Getting fixup info for object: ", currentFixup.m_id);
                    if (GetCompletionInfo(currentFixup, out tempObjectHolder, out fixupInfo, bObjectFullyComplete)) {
                        BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Fixing up: ", currentFixup.m_id);
                        
                        // Check to make sure we are not both reachable from the topObject
                        // and there was a typeloadexception
                        if (tempObjectHolder.TypeLoadExceptionReachable){
                            holder.TypeLoadException = tempObjectHolder.TypeLoadException;
                            // If the holder is both reachable and typeloadexceptionreachable
                            // throw an exception with the type name
                            if (holder.Reachable)
                            {
                                throw new SerializationException(Environment.GetResourceString("Serialization_TypeLoadFailure", holder.TypeLoadException.TypeName));
                            }
                        }

                        // If the current holder is reachable, mark the dependant reachable as well
                        if (holder.Reachable)
                            tempObjectHolder.Reachable = true;


                        //There are two types of fixups that we could be doing: array or member.  
                        //Delayed Fixups should be handled by the above branch.
                        switch(currentFixup.m_fixupType) {
                        case FixupHolder.ArrayFixup:
                            Contract.Assert(holder.ObjectValue is Array,"holder.ObjectValue is Array");
                            if (holder.RequiresValueTypeFixup) {
                                throw new SerializationException(Environment.GetResourceString("Serialization_ValueTypeFixup"));
                            } else {
                                ((Array)(holder.ObjectValue)).SetValue(tempObjectHolder.ObjectValue, ((int[])fixupInfo));
                            }
                            break;
                        case FixupHolder.MemberFixup:
                            Contract.Assert(fixupInfo is MemberInfo,"fixupInfo is MemberInfo");
                            //Fixup the member directly.
                            MemberInfo tempMember = (MemberInfo)fixupInfo;
                            if (tempMember.MemberType==MemberTypes.Field) {
                                BCLDebug.Trace("SER", "[ObjectManager.CompleteObject]Fixing member: ", tempMember.Name, " in object ", holder.m_id,
                                               " with object ", tempObjectHolder.m_id);

                                // If we have a valuetype that's been boxed to an object and requires a fixup,
                                // there are two possible states:
                                // (a)The valuetype has never been fixed up into it's container.  In this case, we should
                                // just fix up the boxed valuetype.  The task of pushing that valuetype into it's container
                                // will be handled later.  This case is handled by the else clause of the following statement.
                                // (b)The valuetype has already been inserted into it's container.  In that case, we need
                                // to go through the more complicated path laid out in DoValueTypeFixup. We can tell that the
                                // valuetype has already been inserted into it's container because we set ValueTypeFixupPerformed
                                // to true when we do this.
                                if (holder.RequiresValueTypeFixup && holder.ValueTypeFixupPerformed) {
                                    if (!DoValueTypeFixup((FieldInfo)tempMember, holder, tempObjectHolder.ObjectValue)) {
                                        throw new SerializationException(Environment.GetResourceString("Serialization_PartialValueTypeFixup"));
                                    }
                                } else {
                                    FormatterServices.SerializationSetValue(tempMember, holder.ObjectValue, tempObjectHolder.ObjectValue);
                                }
                                if (tempObjectHolder.RequiresValueTypeFixup) {
                                    tempObjectHolder.ValueTypeFixupPerformed = true;
                                }
                            } else {
                                throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
                            }
                            break;
                        default:
                            throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
                        }
                        //Decrement our total number of fixups left to do.
                        fixupsPerformed++;
                        fixups.m_values[i]=null;
                        if (!bObjectFullyComplete) {
                            holder.DecrementFixupsRemaining(this);
                            tempObjectHolder.RemoveDependency(holder.m_id);
                        }
                    }
                }
            }

            m_fixupCount-=fixupsPerformed;

            if (fixups.m_count==fixupsPerformed) {
                holder.m_missingElements=null;
            }
        }

        
        /*========================DoNewlyRegisteredObjectFixups=========================
        **Action:  This is called immediately after we register a new object.  Walk that objects
        **         dependency list (if it has one) and decrement the counters on each object for
        **         the number of unsatisfiable references.  If the count reaches 0, go ahead
        **         and process the object.
        **Returns: void
        **Arguments: dependencies The list of dependent objects
        **Exceptions: None.
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        private void DoNewlyRegisteredObjectFixups(ObjectHolder holder) {
            ObjectHolder temp;
            
            if (holder.CanObjectValueChange) {
                BCLDebug.Trace("SER","[ObjectManager.DoNewlyRegisteredObjectFixups]Object is an Incomplete Object Reference.  Exiting.");
                return;
            }

            LongList dependencies = holder.DependentObjects;

            //If we don't have any dependencies, we're done.
            if (dependencies==null) {
                BCLDebug.Trace("SER", "[DoNewlyRegisteredObjectFixups]Exiting with no dependencies");
                return;
            }
            
            //Walk all of the dependencies and decrement the counter on each of uncompleted objects.
            //If one of the counters reaches 0, all of it's fields have been completed and we should
            //go take care of its fixups.
            BCLDebug.Trace("SER", "[ObjectManager.DoNewlyRegisteredObjectFixups]Object has ", dependencies.Count, " fixups registered");
            dependencies.StartEnumeration();
            while (dependencies.MoveNext()) {
                temp = FindObjectHolder(dependencies.Current);
                BCLDebug.Trace("SER", "[ObjectManager.DoNewlyRegisteredObjectFixups]Doing a fixup on object: ", temp.m_id);
                Contract.Assert(temp.DirectlyDependentObjects>0,"temp.m_missingElementsRemaining>0");
                temp.DecrementFixupsRemaining(this);
                if (((temp.DirectlyDependentObjects))==0) {
                    BCLDebug.Trace("SER", "[DoNewlyRegisteredObjectFixups]Doing fixup for object ", temp.m_id);
                    BCLDebug.Trace("SER", "[DoNewlyRegisteredObjectFixups]ObjectValue ", ((temp.ObjectValue==null)?"<null>":temp.ObjectValue));
                    // If this is null, we have the case where a fixup was registered for a child, the object 
                    // required by the fixup was provided, and the object to be fixed hasn't yet been seen.  
                    if (temp.ObjectValue!=null) {
                        CompleteObject(temp, true);
                    } else {
                        temp.MarkForCompletionWhenAvailable();
                    }
                }
            }
            BCLDebug.Trace("SER", "[ObjectManager.DoNewlyRegisteredObjectFixups]Exiting.");
        }
        
        public virtual Object GetObject(long objectID) {
            if (objectID<=0) {
                throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
            }
            Contract.EndContractBlock();
    
            //Find the bin in which we're interested.  IObjectReference's shouldn't be returned -- the graph
            //needs to link to the objects to which they refer, not to the references themselves.
            ObjectHolder holder = FindObjectHolder(objectID);
    
            BCLDebug.Trace("SER", "GetObject. objectID: ", objectID);

            if (holder==null || holder.CanObjectValueChange) { 
                BCLDebug.Trace("SER", "GetObject. holder: null or IncompleteObjectReference");
                return null;
            }
            
            BCLDebug.Trace("SER", "GetObject. holder contains: ", ((holder.ObjectValue==null)?"<null>":holder.ObjectValue));
            return holder.ObjectValue;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void RegisterObject(Object obj, long objectID) {
            RegisterObject(obj, objectID, null,0,null);
        }
    

        [System.Security.SecurityCritical]  // auto-generated_required
        public void RegisterObject(Object obj, long objectID, SerializationInfo info) {
            RegisterObject(obj, objectID, info, 0, null);
        }


        [System.Security.SecurityCritical]  // auto-generated_required
        public void RegisterObject(Object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member) {
            RegisterObject(obj, objectID, info, idOfContainingObj, member, null);
        }


        internal void RegisterString(String obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member) 
        {
            ObjectHolder temp;
            Contract.Assert(member == null || member is FieldInfo, "RegisterString - member is FieldInfo");
            Contract.Assert((FindObjectHolder(objectID) == null), "RegisterString - FindObjectHolder(objectID) == null");

            temp = new ObjectHolder(obj, objectID, info, null, idOfContainingObj, (FieldInfo)member, null);
            AddObjectHolder(temp);
            return;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public void RegisterObject(Object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member, int[] arrayIndex) {
            if (obj==null) {
                throw new ArgumentNullException("obj");
            }
            if (objectID<=0) {
                throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
            }
            Contract.EndContractBlock();

            if (member!=null && !(member is RuntimeFieldInfo) && !(member is SerializationFieldInfo)) {
                throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
            }

            ObjectHolder temp;
            ISerializationSurrogate surrogate = null;
            ISurrogateSelector useless;

            if (m_selector != null)
            {
                Type selectorType=null;
                if (CanCallGetType(obj)) {
                    selectorType = obj.GetType();
                } else {
                    selectorType = typeof(MarshalByRefObject);
                }

                BCLDebug.Trace("SER", "[ObjectManager.RegisterObject]ID: ", objectID, "\tType: ", selectorType, "\tValue: ", obj);
    
                //If we need a surrogate for this object, lets find it now.
                surrogate = m_selector.GetSurrogate(selectorType, m_context, out useless);
            }

#if FEATURE_SERIALIZATION
            //The object is interested in DeserializationEvents so lets register it.
            if (obj is IDeserializationCallback) {
                DeserializationEventHandler d = new DeserializationEventHandler(((IDeserializationCallback)obj).OnDeserialization);
                AddOnDeserialization(d);
            }
#endif

            //Formatter developers may cache and reuse arrayIndex in their code.
            //So that we don't get bitten by this, take a copy up front.
            if (arrayIndex!=null) {
              arrayIndex = (int[])arrayIndex.Clone();
            }
            
            temp = FindObjectHolder(objectID);
            //This is the first time which we've seen the object, we need to create a new holder.
            if (temp==null) { 
                BCLDebug.Trace("SER", "[ObjectManager.RegisterObject]Adding a new object holder for ", objectID, "\tValueType: ", obj.GetType());
                
                temp = new ObjectHolder(obj, objectID, info, surrogate, idOfContainingObj, (FieldInfo)member, arrayIndex);
                AddObjectHolder(temp);
                if (temp.RequiresDelayedFixup) {
                    SpecialFixupObjects.Add(temp);   
                }
                // We cannot compute whether this has any fixups required or not
                AddOnDeserialized(obj);
                return;
            } 
            
            //If the object isn't null, we've registered this before.  Not good.
            if (temp.ObjectValue!=null) {
                throw new SerializationException(Environment.GetResourceString("Serialization_RegisterTwice"));
            }
            
            //Complete the data in the ObjectHolder
            temp.UpdateData(obj, info, surrogate, idOfContainingObj, (FieldInfo)member, arrayIndex, this);

            // The following case will only be true when somebody has registered a fixup on an object before
            // registering the object itself.  I don't believe that most well-behaved formatters will do this,
            // but we need to allow it anyway.  We will walk the list of fixups which have been recorded on 
            // the new object and fix those that we can.  Because the user could still register later fixups
            // on this object, we won't call any implementations of ISerializable now.  If that's required,
            // it will have to be handled by the code in DoFixups.
            // README README: We have to do the UpdateData before 
            if (temp.DirectlyDependentObjects>0) {
                CompleteObject(temp, false);
            }

            if (temp.RequiresDelayedFixup) {
                BCLDebug.Trace("SER", "[ObjectManager.RegisterObject]Tracking incomplete objref for element: ", temp.m_id);
                SpecialFixupObjects.Add(temp);   
            } 

            if (temp.CompletelyFixed) {
                //Here's where things get tricky.  If this isn't an instance of IObjectReference, we need to walk it's fixup 
                //chain and decrement the counters on anything that has reached 0.  Once we've notified all of the dependencies,
                //we can simply clear the list of dependent objects.
                BCLDebug.Trace("SER", "[ObjectManager.RegisterObject]Calling DoNewlyRegisteredObjectFixups for element: ", temp.m_id);
                DoNewlyRegisteredObjectFixups(temp);
                temp.DependentObjects=null;
            }

            //Register the OnDeserialized methods to be invoked after deserialization is complete
            if (temp.TotalDependentObjects > 0){
                    AddOnDeserialized(obj);
            }
            else {
                    RaiseOnDeserializedEvent(obj);
            }
                    

            BCLDebug.Trace("SER", "[ObjectManager.RegisterObject]Exiting.");
        }

        /*=========================CompleteISerializableObject==========================
        **Action: Completes an object implementing ISerializable.  This will involve calling that
        **        objects constructor which takes an instance of ISerializable and a StreamingContext.
        **Returns: void.
        **Arguments: Obj     --  The object to be completed.
        **           info    --  The SerializationInfo containing all info for obj.
        **           context --  The streaming context in which the serialization is taking place.
        **Exceptions: ArgumentNullException if obj is null
        **            ArgumentException if obj does not implement ISerializable.
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        internal void CompleteISerializableObject(Object obj, SerializationInfo info, StreamingContext context) {
            if (obj==null) {
                throw new ArgumentNullException("obj");
            }

            if (!(obj is ISerializable)) {
                throw new ArgumentException(Environment.GetResourceString("Serialization_NotISer"));
            }
            Contract.EndContractBlock();

            RuntimeConstructorInfo constInfo = null;

            RuntimeType t = (RuntimeType)obj.GetType();

            try {
#if !FEATURE_PAL
                if (t == TypeOfWindowsIdentity && m_isCrossAppDomain)
                    constInfo = WindowsIdentity.GetSpecialSerializationCtor();
                else
#endif                    
                    constInfo = GetConstructor(t);
            } catch (Exception e) {
                BCLDebug.Trace("SER", "[CompleteISerializableObject]Unable to get constructor for: ", t);
                BCLDebug.Trace("SER", "[CompleteISerializableObject]Stack trace was: ", e);
                throw new SerializationException(Environment.GetResourceString("Serialization_ConstructorNotFound", t), e);
            }

            constInfo.SerializationInvoke(obj, info, context);
        }


        /*================================GetConstructor================================
        **Action:
        **Returns:
        **Arguments:
        **Exceptions:
        ==============================================================================*/
        internal static RuntimeConstructorInfo GetConstructor(RuntimeType t)
        {
            RuntimeConstructorInfo ci = t.GetSerializationCtor();

            if (ci == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_ConstructorNotFound", t.FullName));

            return ci;
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public virtual void DoFixups() {
            ObjectHolder temp;
            int fixupCount=-1;
            
            BCLDebug.Trace("SER", "[ObjectManager.DoFixups]Entering");

            //The first thing that we need to do is fixup all of the objects which implement
            //IObjectReference.  This is complicated by the fact that we need to deal with IReferenceObjects 
            //objects that have a reference to an object implementing IObjectReference.  We continually
            //walk over the list of objects until we've completed all of the object references or until
            //we can't resolve any more (which may happen if we have two objects implementing IObjectReference
            //which have a circular dependency on each other).  We don't explicitly catch the later case here,
            //it will be caught when we try to do the rest of the fixups and discover that we have some that
            //can't be completed.
            while (fixupCount!=0) {
                fixupCount=0;
                //Walk all of the IObjectReferences and ensure that they've been properly completed.
                ObjectHolderListEnumerator fixupObjectsEnum = SpecialFixupObjects.GetFixupEnumerator();
                while (fixupObjectsEnum.MoveNext()) {
                    temp = fixupObjectsEnum.Current;
                    if (temp.ObjectValue == null) {
                        BCLDebug.Trace("SER", "[ObjectManager.DoFixups]Object with id: ", temp.m_id, " not found.");
                        throw new SerializationException(Environment.GetResourceString("Serialization_ObjectNotSupplied", temp.m_id));
                    }
                    BCLDebug.Trace("SER", "[ObjectManager.DoFixups]Looking at object with id: ", temp.m_id, " which has ", 
                                   temp.TotalDependentObjects, " Total Dependent Fixups, but only ", 
                                   (temp.DependentObjects==null)?0:temp.DependentObjects.Count, 
                                   " directly dependent objects. Has it been fixed? ", temp.CompletelyFixed);
                    if (temp.TotalDependentObjects==0) {
                        if (temp.RequiresSerInfoFixup) {
                            FixupSpecialObject(temp);
                            fixupCount++;
                        } else if (!temp.IsIncompleteObjectReference) {
                            CompleteObject(temp, true);
                        }
                        
                        if (temp.IsIncompleteObjectReference && ResolveObjectReference(temp)) {
                            fixupCount++;
                        } 
                    }
                }
            }
            
            Contract.Assert(m_fixupCount>=0,"[ObjectManager.DoFixups]m_fixupCount>=0");
    
            //If our count is 0, we're done and should just return
            if (m_fixupCount==0) {
                BCLDebug.Trace("SER", "[ObjectManager.DoFixups]All fixups completed.  We don't need to walk the list.");
                if (TopObject is TypeLoadExceptionHolder)
                    throw new SerializationException(Environment.GetResourceString("Serialization_TypeLoadFailure", ((TypeLoadExceptionHolder)TopObject).TypeName));
                return;
            }
    
            //If our count isn't 0, we had at least one case where an object referenced another object twice.
            //Walk the entire list until the count is 0 or until we find an object which we can't complete.
            BCLDebug.Trace("SER", "[ObjectManager.DoFixups]Remaining object length is: ", m_objects.Length);
            for (int i=0; i<m_objects.Length; i++) {
                temp = m_objects[i];
                while (temp!=null) {
                    if (temp.TotalDependentObjects>0 /*|| temp.m_missingElements!=null*/) {
                        BCLDebug.Trace("SER", "[ObjectManager.DoFixups]Doing a delayed fixup on object ", temp.m_id);
                        CompleteObject(temp, true);
                    }
                    temp = temp.m_next;
                }
                if (m_fixupCount==0) {
                    return;
                }
            }

            // this assert can be trigered by user code that manages fixups manually
            BCLDebug.Correctness(false, "[ObjectManager.DoFixups] Fixup counting is incorrect.");
            throw new SerializationException(Environment.GetResourceString("Serialization_IncorrectNumberOfFixups"));
        }

        /*================================RegisterFixup=================================
        **Action: Do the actual grunt work of recording a fixup and registering the dependency.
        **        Create the necessary ObjectHolders and use them to do the addition.
        **Returns: void
        **Arguments: fixup -- The FixupHolder to be added.
        **           objectToBeFixed -- The id of the object requiring the fixup.
        **           objectRequired -- The id of the object required to do the fixup.
        **Exceptions: None.  This is internal-only, so all checking should have been done by this time.
        ==============================================================================*/
        private void RegisterFixup(FixupHolder fixup, long objectToBeFixed, long objectRequired) {
            //Record the fixup with the object that needs it.
            ObjectHolder ohToBeFixed = FindOrCreateObjectHolder(objectToBeFixed);
            ObjectHolder ohRequired;
            
            if (ohToBeFixed.RequiresSerInfoFixup && fixup.m_fixupType == FixupHolder.MemberFixup) {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupType"));
            }

            //Add the fixup to the list.
            ohToBeFixed.AddFixup(fixup, this);
    
            //Find the object on which we're dependent and note the dependency.
            //These dependencies will be processed when the object is supplied.
            ohRequired = FindOrCreateObjectHolder(objectRequired);
            
            ohRequired.AddDependency(objectToBeFixed);

            m_fixupCount++;
        }
    
        public virtual void RecordFixup(long objectToBeFixed, MemberInfo member, long objectRequired) {
    
            //Verify our arguments
            if (objectToBeFixed<=0 || objectRequired<=0) {
                throw new ArgumentOutOfRangeException(((objectToBeFixed<=0)?"objectToBeFixed":"objectRequired"),
                                                      Environment.GetResourceString("Serialization_IdTooSmall"));
            }

            if (member==null) {
                throw new ArgumentNullException("member");
            }
            Contract.EndContractBlock();

            if (!(member is RuntimeFieldInfo) && !(member is SerializationFieldInfo)) {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidType", member.GetType().ToString()));
            }

    
            BCLDebug.Trace("SER", "RecordFixup.  ObjectToBeFixed: ", objectToBeFixed, "\tMember: ", member.Name, "\tRequiredObject: ", objectRequired);
    
            //Create a new fixup holder
            FixupHolder fixup = new FixupHolder(objectRequired, member, FixupHolder.MemberFixup);
    
            RegisterFixup(fixup, objectToBeFixed, objectRequired);
        }
    
    
        /*==============================RecordDelayedFixup==============================
        **Action:
        **Returns:
        **Arguments:
        **Exceptions:
        ==============================================================================*/
        public virtual void RecordDelayedFixup(long objectToBeFixed, String memberName, long objectRequired) {
            //Verify our arguments
            if (objectToBeFixed<=0 || objectRequired<=0) {
                throw new ArgumentOutOfRangeException(((objectToBeFixed<=0)?"objectToBeFixed":"objectRequired"),
                                                      Environment.GetResourceString("Serialization_IdTooSmall"));
            }
    
            if (memberName==null) {
                throw new ArgumentNullException("memberName");
            }
            Contract.EndContractBlock();
    
            BCLDebug.Trace("SER", "RecordDelayedFixup.  ObjectToBeFixed: ", objectToBeFixed, "\tMember: ", memberName, "\tRequiredObject: ", objectRequired);
    
            //Create a new fixup holder
            FixupHolder fixup = new FixupHolder(objectRequired, memberName, FixupHolder.DelayedFixup);
    
            RegisterFixup(fixup, objectToBeFixed, objectRequired);
        }
    
        /*===========================RecordArrayElementFixup============================
        **Action: 
        **Returns:
        **Arguments:
        **Exceptions:
        ==============================================================================*/
        public virtual void RecordArrayElementFixup(long arrayToBeFixed, int index, long objectRequired) {
            int[] indexArray = new int[1];
            indexArray[0]=index;
    
            BCLDebug.Trace("SER", "RecordArrayElementFixup.  ObjectToBeFixed: ", arrayToBeFixed, "\tIndex: ", index, "\tRequiredObject: ", objectRequired);
    
            RecordArrayElementFixup(arrayToBeFixed, indexArray, objectRequired);
        }
    
        public virtual void RecordArrayElementFixup(long arrayToBeFixed, int[] indices, long objectRequired) {
            //Verify our arguments
            if (arrayToBeFixed<=0 || objectRequired<=0) {
                throw new ArgumentOutOfRangeException(((arrayToBeFixed<=0)?"objectToBeFixed":"objectRequired"),
                                                      Environment.GetResourceString("Serialization_IdTooSmall"));
            }
    
            if (indices==null) {
                throw new ArgumentNullException("indices");
            }
            Contract.EndContractBlock();
    
            BCLDebug.Trace("SER", "RecordArrayElementFixup.  ArrayToBeFixed: ", arrayToBeFixed, "\tRequiredObject: ", objectRequired);
            FixupHolder fixup = new FixupHolder(objectRequired, indices, FixupHolder.ArrayFixup);
            RegisterFixup(fixup, arrayToBeFixed, objectRequired);
        }
    
    
        /*==========================RaiseDeserializationEvent===========================
        **Action:Raises the deserialization event to any registered object which implements 
        **       IDeserializationCallback.  
        **Returns: void
        **Arguments: none
        **Exceptions: None
        ==============================================================================*/
        public virtual void RaiseDeserializationEvent() {
            // Invoke OnDerserialized event if applicable
            if (m_onDeserializedHandler != null) {
                m_onDeserializedHandler(m_context);
            }

            if (m_onDeserializationHandler!=null) {
                m_onDeserializationHandler(null);
            }
        }
    
        internal virtual void AddOnDeserialization(DeserializationEventHandler handler) {
            m_onDeserializationHandler = (DeserializationEventHandler)Delegate.Combine(m_onDeserializationHandler, handler);
        }
    
        internal virtual void RemoveOnDeserialization(DeserializationEventHandler handler) {
            m_onDeserializationHandler = (DeserializationEventHandler)Delegate.Remove(m_onDeserializationHandler, handler);
        }

        [System.Security.SecuritySafeCritical]
        internal virtual void AddOnDeserialized(Object obj)
        {
            SerializationEvents cache = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            m_onDeserializedHandler = cache.AddOnDeserialized(obj, m_onDeserializedHandler);
        }

        internal virtual void RaiseOnDeserializedEvent(Object obj)
        {
            SerializationEvents cache = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            cache.InvokeOnDeserialized(obj, m_context);
        }

        public void RaiseOnDeserializingEvent(Object obj)
        {
            // Run the OnDeserializing methods
            SerializationEvents cache = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            cache.InvokeOnDeserializing(obj, m_context);
        }
    }
    
    internal sealed class ObjectHolder {
        internal const int INCOMPLETE_OBJECT_REFERENCE = 0x0001;
        internal const int HAS_ISERIALIZABLE           = 0x0002;
        internal const int HAS_SURROGATE               = 0x0004;
        internal const int REQUIRES_VALUETYPE_FIXUP    = 0x0008;
        internal const int REQUIRES_DELAYED_FIXUP      = HAS_ISERIALIZABLE | HAS_SURROGATE | INCOMPLETE_OBJECT_REFERENCE;
        internal const int SER_INFO_FIXED              = 0x4000;
        internal const int VALUETYPE_FIXUP_PERFORMED   = 0x8000;
    
        private  Object m_object;
        internal long   m_id;
        private  int    m_missingElementsRemaining;
        private  int    m_missingDecendents;
        internal SerializationInfo m_serInfo;
        internal ISerializationSurrogate m_surrogate;
        internal FixupHolderList m_missingElements;
        internal LongList m_dependentObjects;
        internal ObjectHolder m_next;
        internal int  m_flags;
        private  bool m_markForFixupWhenAvailable;
        private  ValueTypeFixupInfo m_valueFixup;
        private TypeLoadExceptionHolder m_typeLoad = null;
        private bool m_reachable = false;
    
        internal ObjectHolder(long objID) 
            : this(null, objID, null, null, 0, null, null) {
        }
    
        internal ObjectHolder(Object obj, long objID, SerializationInfo info, 
                              ISerializationSurrogate surrogate, long idOfContainingObj, FieldInfo field, int[] arrayIndex) {
            Contract.Assert(objID>=0,"objID>=0");
            
            m_object=obj; //May be null;
            m_id=objID;
    
            m_flags=0;
            m_missingElementsRemaining=0;
            m_missingDecendents = 0;
            m_dependentObjects=null;
            m_next=null;
            
            m_serInfo = info;
            m_surrogate = surrogate;
            m_markForFixupWhenAvailable = false;

            if (obj is TypeLoadExceptionHolder)
            {
                m_typeLoad = (TypeLoadExceptionHolder)obj;
            }


            if (idOfContainingObj!=0 && ((field!=null && field.FieldType.IsValueType) || arrayIndex!=null)) {
                if (idOfContainingObj == objID) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_ParentChildIdentical"));
                }

                m_valueFixup = new ValueTypeFixupInfo(idOfContainingObj, field, arrayIndex);
            }

            SetFlags();
        }
    
        internal ObjectHolder(String obj, long objID, SerializationInfo info, 
                              ISerializationSurrogate surrogate, long idOfContainingObj, FieldInfo field, int[] arrayIndex) {
            Contract.Assert(objID>=0,"objID>=0");
            
            m_object=obj; //May be null;
            m_id=objID;
    
            m_flags=0;
            m_missingElementsRemaining=0;
            m_missingDecendents = 0;
            m_dependentObjects=null;
            m_next=null;
            
            m_serInfo = info;
            m_surrogate = surrogate;
            m_markForFixupWhenAvailable = false;

            if (idOfContainingObj!=0 && arrayIndex!=null) {
                m_valueFixup = new ValueTypeFixupInfo(idOfContainingObj, field, arrayIndex);
            }

            if (m_valueFixup!=null) {
                m_flags|=REQUIRES_VALUETYPE_FIXUP;
            }
        }

        private void IncrementDescendentFixups(int amount) {
            m_missingDecendents+=amount;
        }
        
        internal void DecrementFixupsRemaining(ObjectManager manager) {
            m_missingElementsRemaining--;
            
            if (RequiresValueTypeFixup) {
                UpdateDescendentDependencyChain(-1, manager);
            }
        }

        /*===============================RemoveDependency===============================
        **Action: Removes a dependency of the object represented in this holder.
        **        This is normally the result of the dependency having been filled when
        **        the object is going to be only partially completed.  If we plan to fully
        **        update the object, we do not take the work to do this.
        **Returns: void.
        **Arguments: id -- The id of the object for which to remove the dependency.
        **Exceptions: None, error handling through asserts.
        ==============================================================================*/
        internal void RemoveDependency(long id) {
            Contract.Assert(m_dependentObjects!=null, "[ObjectHolder.RemoveDependency]m_dependentObjects!=null");
            Contract.Assert(id>=0, "[ObjectHolder.RemoveDependency]id>=0");
            m_dependentObjects.RemoveElement(id);
        }

        /*===================================AddFixup===================================
        **Action: Note a fixup that has to be done before this object can be completed.
        **        Fixups are things that need to happen when other objects in the graph 
        **        are added.  Dependencies are things that need to happen when this object
        **        is added.
        **Returns: void
        **Arguments: fixup -- The fixup holder containing enough information to complete the fixup.
        **Exceptions: None.
        ==============================================================================*/
        internal void AddFixup(FixupHolder fixup, ObjectManager manager) {
            if (m_missingElements==null) {
                m_missingElements = new FixupHolderList();
            }
            m_missingElements.Add(fixup);
            m_missingElementsRemaining++;
            
            if (RequiresValueTypeFixup) {
                UpdateDescendentDependencyChain(1, manager);
            }
        }
            
        /*==========================UpdateTotalDependencyChain==========================
        **Action: Updates the total list of dependencies to account for a fixup being added
        **        or completed in a child value class.  This will update all value classes
        **        containing that child and the object which contains all of them.  
        **Returns: void
        **Arguments: amount -- the amount by which to increment (or decrement) the dependency chain.
        **           manager -- The ObjectManager used to lookup other objects up the chain.
        **Exceptions: None.  Asserts only.
        ==============================================================================*/
        private void UpdateDescendentDependencyChain(int amount, ObjectManager manager) {
            ObjectHolder holder = this;
            
            //This loop walks one more object up the chain than there are valuetypes.  This
            //is because we need to increment the TotalFixups in the holders as well.
            do {
                holder = manager.FindOrCreateObjectHolder(holder.ContainerID);
                BCLDebug.Trace("SER", "[ObjectManager.UpdateDescendentDependencyChain]Looking for holder with id: ", holder.ContainerID);
                Contract.Assert(holder!=null, "[ObjectHolder.UpdateTotalDependencyChain]holder!=null");
                holder.IncrementDescendentFixups(amount);
            } while (holder.RequiresValueTypeFixup);
        }
    
        /*================================AddDependency=================================
        **Action: Note an object which is dependent on the one which will be contained in
        **        this ObjectHolder.  Dependencies should only be added if the object hasn't
        **        yet been added.  NB: An incomplete object counts as having no object.
        **Returns: void
        **Arguments: dependentObject -- the id of the object which is dependent on this object being provided.
        **Exceptions: None.
        ==============================================================================*/
        internal void AddDependency(long dependentObject) {
            if (m_dependentObjects==null) {
                m_dependentObjects = new LongList();
            }
            m_dependentObjects.Add(dependentObject);
        }
    
       
        /*==================================UpdateData==================================
        **Action: Update the data in the object holder.  This should be called when the object
        **        is finally registered.  Presumably the ObjectHolder was created to track 
        **        some dependencies or preregistered fixups and we now need to actually record the
        **        object and other associated data.  We take this opportunity to set the flags
        **        so that we can do some faster processing in the future.
        **Returns: void
        **Arguments: obj -- The object being held by this object holder. (This should no longer be null).
        **           info --The SerializationInfo associated with this object, only required if we're doing delayed fixups.
        **           surrogate -- The surrogate handling this object.  May be null.
        **           idOfContainer -- The id of the object containing this one if this is a valuetype.
        **           member -- the MemberInfo of this object's position in it's container if this is a valuetype.
        **           manager -- the ObjectManager being used to track these ObjectHolders.
        **Exceptions: None. Asserts only.
        ==============================================================================*/
        [System.Security.SecurityCritical]  // auto-generated
        internal void UpdateData(Object obj, SerializationInfo info, ISerializationSurrogate surrogate, long idOfContainer, FieldInfo field, int[] arrayIndex, ObjectManager manager) {
            Contract.Assert(obj!=null,"obj!=null");
            Contract.Assert(m_id>0,"m_id>0");
    
            //Record the fields that we can.
            SetObjectValue(obj, manager);
            m_serInfo = info;
            m_surrogate = surrogate;

            if (idOfContainer!=0 && ((field!=null && field.FieldType.IsValueType) || arrayIndex!=null)) {
                if (idOfContainer == m_id) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_ParentChildIdentical"));
                }
                m_valueFixup = new ValueTypeFixupInfo(idOfContainer, field, arrayIndex);
            }

            SetFlags();
            
            if (RequiresValueTypeFixup) {
                UpdateDescendentDependencyChain(m_missingElementsRemaining, manager);
            }
        }
    
        internal void MarkForCompletionWhenAvailable() {
            m_markForFixupWhenAvailable = true;
        }


        /*===================================SetFlags===================================
        **Action: An internal-only routine to set the flags based upon the data contained in 
        **        the ObjectHolder
        **Returns: Void
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        internal void SetFlags() {
            if (m_object is IObjectReference) {
                m_flags|=INCOMPLETE_OBJECT_REFERENCE;
            }
    
            m_flags &= ~(HAS_ISERIALIZABLE | HAS_SURROGATE);
            if (m_surrogate!=null) 
                m_flags|=HAS_SURROGATE;
            else if (m_object is ISerializable)
                m_flags|=HAS_ISERIALIZABLE;
            
            if (m_valueFixup!=null) {
                m_flags|=REQUIRES_VALUETYPE_FIXUP;
            }
        }
    
        internal bool IsIncompleteObjectReference {
            get { return (m_flags & (INCOMPLETE_OBJECT_REFERENCE /*| HAS_SURROGATE*/)) != 0; }
            set { 
                if (value) {
                    m_flags|=INCOMPLETE_OBJECT_REFERENCE;
                } else {
                    m_flags&=~INCOMPLETE_OBJECT_REFERENCE;
                }
            }
        }
    
        internal bool RequiresDelayedFixup {
            get { return (m_flags & REQUIRES_DELAYED_FIXUP)!=0; }
        }

        internal bool RequiresValueTypeFixup {
            get { return (m_flags & REQUIRES_VALUETYPE_FIXUP)!=0; }
        }

        // ValueTypes which require fixups are initially handed to the ObjectManager
        // as boxed objects.  When they're still boxed objects, we should just do fixups
        // on them like we would any other object.  As soon as they're pushed into their
        // containing object we set ValueTypeFixupPerformed to true and have to go through
        // a more complicated path to set fixed up valuetype objects.
        // We check whether or not there are any dependent objects. 
        internal bool ValueTypeFixupPerformed {
            get { 
                BCLDebug.Trace("SER", "[ObjectManager.ValueTypeFixupPerformed]Flags: ", m_flags & VALUETYPE_FIXUP_PERFORMED);
                BCLDebug.Trace("SER", "[ObjectManager.ValueTypeFixupPerformed]DependentObjects: ", (m_dependentObjects==null)?"<null>":m_dependentObjects.Count.ToString());
                return ( ((m_flags & VALUETYPE_FIXUP_PERFORMED)!=0)|| 
                        (m_object!=null && ((m_dependentObjects==null) || m_dependentObjects.Count==0))); }
            set { 
                if (value) {
                    m_flags|=VALUETYPE_FIXUP_PERFORMED;
                }
            }
        }
                

        internal bool HasISerializable {
            get {
                return (m_flags & HAS_ISERIALIZABLE)!=0;
            }
        }                
    
        internal bool HasSurrogate {
            get { return (m_flags & HAS_SURROGATE)!=0; }
        }


        internal bool CanSurrogatedObjectValueChange
        {
            get
            {
                return (m_surrogate == null || m_surrogate.GetType() != typeof(SurrogateForCyclicalReference));
            }
        }
        
        internal bool CanObjectValueChange
        {
            get
            {
                if (IsIncompleteObjectReference)
                    return true;
                if (HasSurrogate)
                    return CanSurrogatedObjectValueChange;
                return false;
            }
        }

        internal int DirectlyDependentObjects {
            get {
                return m_missingElementsRemaining;
            }
        }


        internal int TotalDependentObjects {
            get {
                return m_missingElementsRemaining + m_missingDecendents;
            }
        }

        internal bool Reachable {
            get { return m_reachable; }
            set { m_reachable = value; }
        }
            
        internal bool TypeLoadExceptionReachable {
            get { return m_typeLoad != null; }
        }

        internal TypeLoadExceptionHolder TypeLoadException {
            get { return m_typeLoad; }
            set { m_typeLoad = value; }
        }
            

        internal Object ObjectValue {
            get {
                return m_object;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void SetObjectValue(Object obj, ObjectManager manager) {
            m_object = obj;
            if (obj == manager.TopObject)
                m_reachable = true;
            if (obj is TypeLoadExceptionHolder)
                m_typeLoad = (TypeLoadExceptionHolder)obj;
            
            if (m_markForFixupWhenAvailable) {
                manager.CompleteObject(this, true);
            }
        }
        
        internal SerializationInfo SerializationInfo {
            get {
                return m_serInfo;
            }
 
            set {
                m_serInfo = value;
            }
        }

        internal ISerializationSurrogate Surrogate {
            get {
                return m_surrogate;
            }
        }
        
        internal LongList DependentObjects {
            get {
                return m_dependentObjects;
            } 
            set {
                m_dependentObjects = value;
            }
        }

        internal bool RequiresSerInfoFixup {
            get {
                if (((m_flags & HAS_SURROGATE)==0) && ((m_flags & HAS_ISERIALIZABLE)==0)) {
                    return false;
                }

                return (m_flags & SER_INFO_FIXED)==0;
            }
            set {
                if (!value) {
                    m_flags|=SER_INFO_FIXED;
                } else {
                    m_flags&=~SER_INFO_FIXED;
                }
            }
        }                

        internal ValueTypeFixupInfo ValueFixup {
            get {
                return m_valueFixup;
            }
        }

        internal bool CompletelyFixed {
            get {
                return (!RequiresSerInfoFixup && !IsIncompleteObjectReference);
            }
        }
        
        internal long ContainerID {
            get {
                if (m_valueFixup!=null) {
                    return m_valueFixup.ContainerID;
                }
                return 0;
            }
        }
    }    
    
    [Serializable]
    internal class FixupHolder {
        internal const int ArrayFixup=0x1;
        internal const int MemberFixup=0x2;
        internal const int DelayedFixup=0x4;
    
        internal long   m_id;
        internal Object m_fixupInfo; //This is either an array index, a String, or a MemberInfo
        internal int    m_fixupType;
        
        internal FixupHolder(long id, Object fixupInfo, int fixupType) {
            Contract.Assert(id>0,"id>0");
            Contract.Assert(fixupInfo!=null,"fixupInfo!=null");
            Contract.Assert(fixupType==ArrayFixup || fixupType == MemberFixup || fixupType==DelayedFixup,"fixupType==ArrayFixup || fixupType == MemberFixup || fixupType==DelayedFixup");
            
            m_id = id;
            m_fixupInfo = fixupInfo;
            m_fixupType = fixupType;
        }
    }
    
    [Serializable]
    internal class FixupHolderList {
        internal const int InitialSize = 2;

        internal FixupHolder[] m_values;
        internal int m_count;
    
        internal FixupHolderList() : this(InitialSize) {
        }
    
        internal FixupHolderList(int startingSize) {
            m_count=0;
            m_values = new FixupHolder[startingSize];
        }
    
        internal virtual void Add(long id, Object fixupInfo) {
            if (m_count==m_values.Length) {
                EnlargeArray();
            }
            m_values[m_count].m_id=id;
            m_values[m_count++].m_fixupInfo = fixupInfo;
        }
    
        internal virtual void Add(FixupHolder fixup) {
            if (m_count==m_values.Length) {
                EnlargeArray();
            }
            m_values[m_count++]=fixup;
        }        
        
        private void EnlargeArray() {
            int newLength = m_values.Length*2;
            if (newLength<0) {
                if (newLength==Int32.MaxValue) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
                }
                newLength=Int32.MaxValue;
            }
    
            FixupHolder[] temp = new FixupHolder[newLength];
            Array.Copy(m_values, temp, m_count);
            m_values=temp;
        }
    
    
    }
    
    [Serializable]
    internal class LongList {
        private const int InitialSize = 2;

        private long [] m_values;
        private int m_count; //The total number of valid items still in the list;
        private int m_totalItems; //The total number of allocated entries.  
                                  //This includes space for items which have been marked as deleted.
        private int m_currentItem; //Used when doing an enumeration over the list.
    
        //
        // An m_currentItem of -1 indicates that the enumeration hasn't been started.
        // An m_values[xx] of -1 indicates that the item has been deleted.
        //
        internal LongList() : this(InitialSize) {
        }
    
        internal LongList(int startingSize) {
            m_count=0;
            m_totalItems = 0;
            m_values = new long[startingSize];
        }
        
        internal void Add(long value) {
            if (m_totalItems==m_values.Length) {
                EnlargeArray();
            }
            m_values[m_totalItems++]=value;
            m_count++;
        }
    
        internal int Count {
            get {
                return m_count;
            }
        }

        internal void StartEnumeration() {
            m_currentItem = -1;
        }

        internal bool MoveNext() {
            while (++m_currentItem < m_totalItems && m_values[m_currentItem]==-1) {
            }
            if (m_currentItem==m_totalItems) {
                return false;
            }
            return true;
        }

        internal long Current {
            get {
                Contract.Assert(m_currentItem!=-1, "[LongList.Current]m_currentItem!=-1");
                Contract.Assert(m_values[m_currentItem]!=-1, "[LongList.Current]m_values[m_currentItem]!=-1");
                return m_values[m_currentItem];
            }
        }

        internal bool RemoveElement(long value) {
            int i;
            for (i=0; i<m_totalItems; i++) {
                if (m_values[i]==value) 
                    break;
            }
            if (i==m_totalItems) {
                return false;
            }
            m_values[i] = -1;
            return true;
        }
             
        private void EnlargeArray() {
            BCLDebug.Trace("SER", "[LongList.EnlargeArray]Enlarging array of size ", m_values.Length);
            int newLength = m_values.Length*2;
            if (newLength<0) {
                if (newLength==Int32.MaxValue) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
                }
                newLength=Int32.MaxValue;
            }
    
            long[] temp = new long[newLength];
            Array.Copy(m_values, temp, m_count);
            m_values = temp;
        }
    }
    
    internal class ObjectHolderList {
        internal const int DefaultInitialSize = 8;
        
        internal ObjectHolder[] m_values;
        internal int m_count;
    
        internal ObjectHolderList() 
            : this(DefaultInitialSize) {
        }
    
        internal ObjectHolderList(int startingSize) {
            Contract.Assert(startingSize>0 && startingSize<0x1000,"startingSize>0 && startingSize<0x1000");
        
            m_count =0;
            m_values = new ObjectHolder[startingSize];
        }
    
        internal virtual void Add(ObjectHolder value) {
            if (m_count==m_values.Length) {
                EnlargeArray();
            }
            m_values[m_count++]=value;
        }
    

        internal ObjectHolderListEnumerator GetFixupEnumerator() {
            return new ObjectHolderListEnumerator(this, true);
        }

        private void EnlargeArray() {
            BCLDebug.Trace("SER", "[ObjectHolderList.EnlargeArray]Enlarging array of size ", m_values.Length);
            int newLength = m_values.Length*2;
            if (newLength<0) {
                if (newLength==Int32.MaxValue) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
                }
                newLength=Int32.MaxValue;
            }
    
            ObjectHolder[] temp = new ObjectHolder[newLength];
            Array.Copy(m_values, temp, m_count);
            m_values = temp;
        }

        internal int Version {
            get {
                return m_count;
            }
        }

        internal int Count {
            get {
                return m_count;
            }
        }
    }

    internal class ObjectHolderListEnumerator {
        bool m_isFixupEnumerator;
        ObjectHolderList m_list;
        int m_startingVersion;
        int m_currPos;

        internal ObjectHolderListEnumerator(ObjectHolderList list, bool isFixupEnumerator) {
            Contract.Assert(list!=null, "[ObjectHolderListEnumerator.ctor]list!=null");
            m_list = list;
            m_startingVersion = m_list.Version;
            m_currPos=-1;
            m_isFixupEnumerator = isFixupEnumerator;
        }

        internal bool MoveNext() {
            Contract.Assert(m_startingVersion==m_list.Version, "[ObjectHolderListEnumerator.MoveNext]m_startingVersion==m_list.Version");
            if (m_isFixupEnumerator) {
                while (++m_currPos < m_list.Count && m_list.m_values[m_currPos].CompletelyFixed) {
                }
                if (m_currPos==m_list.Count) {
                    return false;
                }
                return true;
            } else {
                m_currPos++;
                if (m_currPos==m_list.Count) {
                    return false;
                }
                return true;
            }
        }

        internal ObjectHolder Current {
            get {
                Contract.Assert(m_currPos!=-1, "[ObjectHolderListEnumerator.Current]m_currPos!=-1");
                Contract.Assert(m_currPos<m_list.Count, "[ObjectHolderListEnumerator.Current]m_currPos<m_list.Count");
                Contract.Assert(m_startingVersion==m_list.Version, "[ObjectHolderListEnumerator.Current]m_startingVersion==m_list.Version");
                return m_list.m_values[m_currPos];
            }
        }
    }

    internal class TypeLoadExceptionHolder {
        string m_typeName;
        internal TypeLoadExceptionHolder (string typeName)
        {
            m_typeName = typeName;
        }

        internal string TypeName
        {
            get { return m_typeName;}
        }
           
    }

    
}
