using System.Diagnostics.Contracts;
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SurrogateSelector
**
**
** Purpose: A user-supplied class for doing the type to surrogate
**          mapping.
**
**
===========================================================*/
namespace System.Runtime.Serialization {

    using System.Runtime.Remoting;
    using System;
    using System.Collections;
    using System.Security.Permissions;       
[System.Runtime.InteropServices.ComVisible(true)]
    public class SurrogateSelector : ISurrogateSelector {
       
        internal SurrogateHashtable m_surrogates;
        internal ISurrogateSelector m_nextSelector;
    
        public SurrogateSelector() {
            m_surrogates = new SurrogateHashtable(32);
        }
    
        // Adds a surrogate to the list of surrogates checked.
        public virtual void AddSurrogate(Type type, StreamingContext context, ISerializationSurrogate surrogate) {
            if (type==null) {
                throw new ArgumentNullException("type");
            }
            if (surrogate==null) {
                throw new ArgumentNullException("surrogate");
            }
            Contract.EndContractBlock();
    
            SurrogateKey key = new SurrogateKey(type, context);
            m_surrogates.Add(key, surrogate);  // Hashtable does duplicate checking.
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        private static bool HasCycle(ISurrogateSelector selector) {
            ISurrogateSelector head;
            ISurrogateSelector tail;
            
            Contract.Assert(selector!=null, "[HasCycle]selector!=null");


            head = selector;
            tail = selector;

            while (head!=null) {
                head = head.GetNextSelector();
                if (head==null) {
                    return true;
                }
                if (head==tail) {
                    return false;
                }
                head = head.GetNextSelector();
                tail = tail.GetNextSelector();

                if (head==tail) {
                    return false;
                }
            }

            return true;
            
        }

        // Adds another selector to check if we don't have  match within this selector.
        // The logic is:"Add this onto the list as the first thing that you check after yourself."
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void ChainSelector(ISurrogateSelector selector) {
            ISurrogateSelector temp;
            ISurrogateSelector tempCurr;
            ISurrogateSelector tempPrev;
            ISurrogateSelector tempEnd;
            
            if (selector==null) {
                throw new ArgumentNullException("selector");
            }
            Contract.EndContractBlock();
    
            //
            // Verify that we don't try and add ourself twice.
            //
            if (selector==this) {
                throw new SerializationException(Environment.GetResourceString("Serialization_DuplicateSelector"));
            }
            
            //
            // Verify that the argument doesn't contain a cycle.
            //
            if (!HasCycle(selector)) {
                throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycleInArgument"), "selector");
            }

            //
            // Check for a cycle that would lead back to this.  We find the end of the list that we're being asked to 
            // insert for use later.
            //
            tempCurr = selector.GetNextSelector();
            tempEnd = selector;
            while (tempCurr!=null && tempCurr!=this) {
                tempEnd = tempCurr;
                tempCurr = tempCurr.GetNextSelector();
            }
            if (tempCurr==this) {
                throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
            }

            //
            // Check for a cycle later in the list which would be introduced by this insertion.
            //
            tempCurr = selector;
            tempPrev = selector;
            while(tempCurr!=null) {
                if (tempCurr==tempEnd) {
                    tempCurr = this.GetNextSelector();
                } else {
                    tempCurr = tempCurr.GetNextSelector();
                }
                if (tempCurr==null) {
                    break;
                }
                if (tempCurr==tempPrev) {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
                }

                if (tempCurr==tempEnd) {
                    tempCurr = this.GetNextSelector();
                } else {
                    tempCurr = tempCurr.GetNextSelector();
                }


                if (tempPrev==tempEnd) {
                    tempPrev = this.GetNextSelector();
                } else {                    
                    tempPrev = tempPrev.GetNextSelector();
                }
                if (tempCurr==tempPrev) {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
                }
            }

            //
            // Add the new selector and it's entire chain of selectors as the next thing that
            // we check.  
            //
            temp = m_nextSelector;
            m_nextSelector = selector;
            if (temp!=null) {
                tempEnd.ChainSelector(temp);
            }
        }
    
        // Get the next selector on the chain of selectors.
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual ISurrogateSelector GetNextSelector() {
            return m_nextSelector;
        }
    
        // Gets the surrogate for a particular type.  If this selector can't
        // provide a surrogate, it checks with all of it's children before returning null.
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector) {
            if (type==null) {
                throw new ArgumentNullException("type");
            }
            Contract.EndContractBlock();
    
            selector = this;
    
            SurrogateKey key = new SurrogateKey(type, context);
            ISerializationSurrogate temp = (ISerializationSurrogate)m_surrogates[key];
            if (temp!=null) {
                return temp;
            }
            if (m_nextSelector!=null) {
                return m_nextSelector.GetSurrogate(type, context, out selector);
            }
            return null;
        }
        
        // Removes the surrogate associated with a given type.  Does not
        // check chained surrogates.  
        public virtual void RemoveSurrogate(Type type, StreamingContext context) {
            if (type==null) {
                throw new ArgumentNullException("type");
            }
            
            Contract.EndContractBlock();
    
            SurrogateKey key = new SurrogateKey(type, context);
            m_surrogates.Remove(key);
        }
    }
    
    //<

    [Serializable]
    internal class SurrogateKey {
        internal Type m_type;
        internal StreamingContext m_context;
    
        internal SurrogateKey(Type type, StreamingContext context) {
            m_type = type;
            m_context = context;
        }
    
        public override int GetHashCode() {
            return m_type.GetHashCode();
        }
    }

    // Subclass to override KeyEquals.
    class SurrogateHashtable : Hashtable {
        internal SurrogateHashtable(int size):base(size){
            ;
        }
        // Must return true if the context to serialize for (givenContext)
        // is a subset of the context for which the serialization selector is provided (presentContext)
        // Note: This is done by overriding KeyEquals rather than overriding Equals() in the SurrogateKey
        // class because Equals() method must be commutative. 
        protected override bool KeyEquals(Object key, Object item){
            SurrogateKey givenValue = (SurrogateKey)item;
            SurrogateKey presentValue = (SurrogateKey)key;
            return presentValue.m_type == givenValue.m_type &&
                   (presentValue.m_context.m_state & givenValue.m_context.m_state) == givenValue.m_context.m_state &&
                   presentValue.m_context.Context == givenValue.m_context.Context;
        }
    }
}
