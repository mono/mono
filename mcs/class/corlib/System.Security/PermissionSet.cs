//
// System.Security.PermissionSet.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System;
using System.Collections;
using System.Security.Permissions;
using System.Security;
using System.Runtime.Serialization;

namespace System.Security
{
    ///<summary> Represents a collection that can contain different kinds of permissions and perform security      operations.</summary>
	[Serializable]
    public class PermissionSet: ISecurityEncodable, ICollection, IEnumerable, IStackWalk, IDeserializationCallback
    {
        ///<summary> Constructs a new instance of the System.Security.PermissionSet class with the   specified   value.</summary>
        ///<param name="state">A System.Security.Permissions.PermissionState value. This value is either System.Security.Permissions.PermissionState.None or System.Security.Permissions.PermissionState.Unrestricted, to specify fully restricted or fully unrestricted access. </param>
        ///<exception cref="System.ArgumentException">state is not a valid System.Security.Permissions.PermissionState value.</exception>
        public PermissionSet(PermissionState state)
        {
            if (!Enum.IsDefined(typeof(System.Security.Permissions.PermissionState), state))
            {
                throw new System.ArgumentException(); // state is not a valid System.Security.Permissions.PermissionState value.
            }
        }

        ///<summary> Constructs a new instance of the System.Security.PermissionSet class with the values of the specified System.Security.PermissionSet instance. </summary>
        ///<param name="permSet">The System.Security.PermissionSet instance with which to initialize the values of the new instance, or null to initialize an empty permission set.</param>
        ///<exception cref="System.ArgumentException">permSet is not an instance of System.Security.PermissionSet.</exception>
        public PermissionSet(PermissionSet permSet)
        {
		// LAMESPEC: This would be handled by the compiler.  No way permSet is not a PermissionSet.
	        //if (false)
		//{
		//	throw new System.ArgumentException(); // permSet is not an instance of System.Security.PermissionSet.
		//}
        }

        ///<summary> Adds the specified System.Security.IPermission object to   the current instance if that permission does not already exist in the current instance.</summary>
        ///<param name="perm">The System.Security.IPermission object to add.</param>
        ///<returns>The System.Security.IPermission is added if perm is notnull and a permission of the same type as perm does    not already exist in the current instance. If perm is null,   returns null. If a permission of the same type asperm already exists in the current instance, the union of the existing    permission and perm is added to the current instance and is returned.</returns>
        ///<exception cref="System.ArgumentException">perm is not a System.Security.IPermission object.</exception>
        public virtual IPermission AddPermission(IPermission perm)
        {
		// LAMESPEC: This would be handled by the compiler.  No way perm is not an IPermission.
		//if (false)
		//{
		//	throw new System.ArgumentException(); // perm is not a System.Security.IPermission object.
		//}
		return null;
        }

        ///<summary>Asserts that calling code can access the resources identified by the permissions contained in the current      instance through the code that calls this method, even if callers have not been      granted permission to access the resource. </summary>
        ///<exception cref="System.Security.SecurityException">The asserting code does not have sufficient permission to call this method.-or-This method was called with permissions already asserted for the current stack frame.</exception>
        public virtual void Assert()
        {
                throw new System.Security.SecurityException(); // The asserting code does not have sufficient permission to call this method.-or-This method was called with permissions already asserted for the current stack frame.
        }

        ///<summary>Returns a new System.Security.PermissionSet containing copies of the objects in the current instance.</summary>
        ///<returns>A new System.Security.PermissionSet that is value equal to the current instance.</returns>
        public virtual PermissionSet Copy()
        {
            return null;
        }

        ///<summary>Copies the permission objects in the current instance to the specified      location in the specified System.Array.</summary>
        ///<param name="array">The destination System.Array.</param>
        ///<param name="index">A System.Int32 that specifies the zero-based starting position in the array at which to begin copying.</param>
        ///<exception cref="System.ArgumentException">array has more than one dimension.</exception>
        ///<exception cref="System.IndexOutOfRangeException">index is outside the range of allowable values for array.</exception>
        ///<exception cref="System.ArgumentNullException">array is null.</exception>
        public virtual void CopyTo(Array array, int index)
        {
            if (array.Rank > 1)
            {
                throw new System.ArgumentException("Array has more than one dimension"); // array has more than one dimension.
            }
            if (index < 0 || index >= array.Length)
            {
                throw new System.IndexOutOfRangeException(); // index is outside the range of allowable values for array.
            }
            if (null == array)
            {
                throw new System.ArgumentNullException(); // array is null.
            }
        }

        ///<summary>Forces a System.Security.SecurityException if all callers do    not have the permissions specified by the objects   contained in the current instance.</summary>
        ///<exception cref="System.Security.SecurityException">A caller does not have the permission specified by the current instance.</exception>
        public virtual void Demand()
        {
                throw new System.Security.SecurityException(); // A caller does not have the permission specified by the current instance.
        }

        ///<summary>Denies access to the resources secured by the objects contained in the current instance through the      code that calls this method.</summary>
        ///<exception cref="System.Security.SecurityException">A previous call to Deny has already restricted the permissions for the current stack frame.</exception>
        public virtual void Deny()
        {
                throw new System.Security.SecurityException(); // A previous call to Deny has already restricted the permissions for the current stack frame.
        }

        ///<summary>Reconstructs the state of a System.Security.PermissionSet object using the specified XML   encoding.</summary>
        ///<param name="et">A System.Security.SecurityElement instance containing the XML encoding to use to reconstruct the state of a System.Security.PermissionSet object.</param>
        ///<exception cref="System.ArgumentNullException">et is null.</exception>
        ///<exception cref="System.ArgumentException">et does not contain an XML encoding for a System.Security.PermissionSet instance.An error occurred while reconstructing et.</exception>
        public virtual void FromXml(SecurityElement et)
        {
            if (null == et)
            {
                throw new System.ArgumentNullException("et"); // et is null.
            }
            if (true)
            {
                throw new System.ArgumentException("et does not contain an XML encoding for a System.Security.PermissionSet instance."); // et does not contain an XML encoding for a System.Security.PermissionSet instance.An error occurred while reconstructing et.
            }
        }

        ///<summary> Returns an enumerator used to iterate      over the permissions in the current instance.</summary>
        ///<returns>A System.Collections.IEnumerator object   for the permissions of the   set.</returns>
        public virtual IEnumerator GetEnumerator()
        {
            return null;
        }

        ///<summary> Determines whether the current instance is a subset of      the specified object.</summary>
        ///<param name="target">A System.Security.PermissionSet instance that is to be tested for the subset relationship. </param>
        ///<returns>true if the current instance is a subset of target;   otherwise, false. If the current instance is unrestricted, andtarget is not, returns false. If target is    unrestricted, returns true.</returns>
        public virtual bool IsSubsetOf(PermissionSet target)
        {
            return false;
        }

        ///<summary> Specifies that only the resources described by the current      instance can be accessed by calling code, even if the code has      been granted permission to access other resources.</summary>
        ///<exception cref="System.Security.SecurityException">A previous call to PermitOnly has already set the permissions for the current stack frame.</exception>
        public virtual void PermitOnly()
        {
            if (true)
            {
                throw new System.Security.SecurityException(); // A previous call to PermitOnly has already set the permissions for the current stack frame.
            }
        }

        ///<summary> Returns a System.String representation of the state   of the current instance.</summary>
        ///<returns>A System.Stringcontaining the XML representation of the state of the current instance.</returns>
        public override string ToString()
        {
            return null;
        }

        ///<summary>Returns the XML encoding of the current instance.</summary>
        ///<returns>A System.Security.SecurityElement containing an XML encoding of the state of the   current instance.</returns>
        public virtual SecurityElement ToXml()
        {
            return null;
        }

        ///<summary> Returns a System.Security.PermissionSet object that is the union of the current instance and   the specified object.</summary>
        ///<param name="other">A System.Security.PermissionSet instance to be combined with the current instance.</param>
        ///<returns> A new System.Security.PermissionSet instance that represents the   union of the current instance and other. If the current   instance or other is unrestricted, returns a System.Security.PermissionSet   instance that is unrestricted.</returns>
        public virtual PermissionSet Union(PermissionSet other)
        {
            return null;
        }

        ///<summary>Implemented to support the System.Collections.ICollection interface. [Note: For more information, see System.Collections.ICollection.Count.]</summary>
        int ICollection.Count 
        {
            get
            {
                return 0;
            }
        }

        ///<summary>Implemented to support the System.Collections.ICollection interface. [Note: For more information, see System.Collections.ICollection.IsSynchronized.]</summary>
        bool ICollection.IsSynchronized 
        {
            get
            {
                return false;
            }
        }

        ///<summary>Implemented to support the System.Collections.ICollection interface. [Note: For more information, see System.Collections.ICollection.SyncRoot.]</summary>
        object ICollection.SyncRoot 
        {
            get
            {
                return null;
            }
        }

	void IDeserializationCallback.OnDeserialization(object sender){}

    }
}
