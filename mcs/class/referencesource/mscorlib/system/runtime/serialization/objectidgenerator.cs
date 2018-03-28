// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: ObjectIDGenerator
**
**
** Purpose: ObjectIDGenerator is a simple id generator that keeps track of 
** objects with a hashtable.
**
**
===========================================================*/
namespace System.Runtime.Serialization {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Diagnostics.Contracts;

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class ObjectIDGenerator {
    
        private const int numbins = 4;
    
        internal int m_currentCount;
        internal int m_currentSize;
        internal long []m_ids;
        internal Object []m_objs;
        
        // Table of prime numbers to use as hash table sizes. Each entry is the
        // smallest prime number larger than twice the previous entry.
        private static readonly int[] sizes = {
            5, 11, 29, 47, 97, 197, 397, 797, 1597, 3203, 6421, 12853, 25717, 51437,
            102877, 205759, 411527, 823117, 1646237, 3292489, 6584983};
    
        // Constructs a new ObjectID generator, initializing all of the necessary variables.
        public ObjectIDGenerator() {
            m_currentCount=1;
            m_currentSize = sizes[0];
            m_ids = new long[m_currentSize*numbins];
            m_objs = new Object[m_currentSize*numbins];
        }
    
        // Determines where element obj lives, or should live, 
        // within the table. It calculates the hashcode and searches all of the
        // bins where the given object could live.  If it's not found within the bin, 
        // we rehash and go look for it in another bin.  If we find the object, we
        // set found to true and return it's position.  If we can't find the object,
        // we set found to false and return the position where the object should be inserted.
        //
        private int FindElement(Object obj, out bool found) {
            //<


            int hashcode = RuntimeHelpers.GetHashCode(obj);
            int hashIncrement = (1+((hashcode&0x7FFFFFFF)%(m_currentSize-2)));        
            do {
                int pos = ((hashcode&0x7FFFFFFF)%m_currentSize)*numbins;

                for (int i=pos; i<pos+numbins; i++) {
                    if (m_objs[i]==null) {
                        found=false;
                        return i;
                    }
                    if (m_objs[i]==obj) {
                        found=true;
                        return i;
                    }
                }
                hashcode+=hashIncrement;
                //the seemingly infinite loop must be revisited later. Currently it is assumed that
                //always the array will be expanded (Rehash) when it is half full
            }while(true);
        }
    
    
        // Gets the id for a particular object, generating one if needed.  GetID calls
        // FindElement to find out where the object lives or should live.  If we didn't
        // find the element, we generate an object id for it and insert the pair into the
        // table.  We return an Int64 for the object id.  The out parameter firstTime
        // is set to true if this is the first time that we have seen this object.
        //
        public virtual long GetId(Object obj, out bool firstTime) {
            bool found;
            long foundID;
            
            if (obj==null) {
                throw new ArgumentNullException("obj", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            Contract.EndContractBlock();

            int pos = FindElement(obj, out found);
    
            //We pull out foundID so that rehashing doesn't cause us to lose track of the id that we just found.
            if (!found) {
#if FEATURE_REMOTING                
                BCLDebug.Trace("SER", "[ObjectIDGenerator.GetID] Adding objectid: ", (m_currentCount), " and hash code: ", 
                               RuntimeHelpers.GetHashCode(obj), " at pos: ", pos, " Type: ", obj, "IsTransparentProxy: ",
                               System.Runtime.Remoting.RemotingServices.IsTransparentProxy(obj));
#endif    
                //We didn't actually find the object, so we should need to insert it into
                //the array and assign it an object id.
                m_objs[pos]=obj;
                m_ids[pos]=m_currentCount++;
                foundID=m_ids[pos]; 
                if (m_currentCount > (m_currentSize*numbins)/2) {
                    Rehash();
                }
            } else {
                BCLDebug.Trace("SER", "[ObjectIDGenerator.GetID] Found objectid: ", (m_ids[pos]), " with hashcode: ", RuntimeHelpers.GetHashCode(obj), " Type: ", obj);
                foundID = m_ids[pos];
            }
            firstTime = !found;
    
            return foundID;
        }
    
        // Checks to see if obj has already been assigned an id.  If it has,
        // we return that id, otherwise we return 0.
        //
        public virtual long HasId(Object obj, out bool firstTime) {
            bool found;
    
            if (obj==null) {
                throw new ArgumentNullException("obj", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            Contract.EndContractBlock();
    
            int pos = FindElement(obj, out found);
            if (found) {
                firstTime = false;
                return m_ids[pos];
            }
            firstTime=true;
            return 0;
        }
    
        // Rehashes the table by finding the next larger size in the list provided,
        // allocating two new arrays of that size and rehashing all of the elements in
        // the old arrays into the new ones.  Expensive but necessary.
        //
        private void Rehash() {
            int i,pos;
            long [] newIds;
            long [] oldIds;
            Object[] newObjs;
            Object[] oldObjs;
            bool found;
            int currSize;
    
            for (i=0, currSize=m_currentSize; i<sizes.Length && sizes[i]<=currSize; i++);
            if (i==sizes.Length) {
                //We just walked off the end of the array, what would you like to do now?
                //We're pretty much hosed at this point, so just keep going.
                throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
            }
            m_currentSize = sizes[i];
    
            newIds = new long[m_currentSize*numbins];
            newObjs = new Object[m_currentSize*numbins];
    
            oldIds = m_ids;
            oldObjs = m_objs;
    
            m_ids = newIds;
            m_objs = newObjs;
            
            for (int j=0; j<oldObjs.Length; j++) {
                if (oldObjs[j]!=null) {
                    pos = FindElement(oldObjs[j], out found);
                    m_objs[pos]=oldObjs[j];
                    m_ids[pos] = oldIds[j];
                }
            }
        }
    }




}

