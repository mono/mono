//
// System.Security.CodeAccessPermission.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Security {

	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk
	{
        ///<summary>Constructs a new instance of the System.Security.CodeAccessPermission class.</summary>
        protected CodeAccessPermission(){}

        ///<summary> Asserts that calling code can access the resource identified by the current instance through the code that      calls this method, even if callers have not been granted permission to      access the resource. </summary>
        ///<exception cref="System.Security.SecurityException">The calling code does not have System.Security.Permissions.SecurityPermissionFlag.Assertion. </exception>
        public void Assert()
        {
            if (false)
            {
                throw new System.Security.SecurityException(); // The calling code does not have System.Security.Permissions.SecurityPermissionFlag.Assertion. 
            }
        }

        ///<summary> Returns a System.Security.CodeAccessPermission containing the same values as the current instance.</summary>
        ///<returns> A new System.Security.CodeAccessPermission instance that is value equal to the current instance.</returns>
        public abstract IPermission Copy();

        ///<summary>Forces a System.Security.SecurityException if all callers do not have the permission specified by the current instance.</summary>
        ///<exception cref="System.Security.SecurityException"> A caller does not have the permission specified by the current instance. A caller has called System.Security.CodeAccessPermission.Deny for the resource protected by the current instance. </exception>
        public void Demand()
        {
            if (false)
            {
                throw new System.Security.SecurityException(); //  A caller does not have the permission specified by the current instance. A caller has called System.Security.CodeAccessPermission.Deny for the resource protected by the current instance. 
            }
        }

        ///<summary> Denies access to the resources specified by the current instance through the code that calls this method.</summary>
        public void Deny(){}

        ///<summary> Reconstructs the state of a System.Security.CodeAccessPermission object using the specified XML encoding.</summary>
        ///<param name="elem">A System.Security.SecurityElement instance containing the XML encoding to use to reconstruct the state of a System.Security.CodeAccessPermission object.</param>
        ///<exception cref="System.ArgumentException">elem does not contain the XML encoding for a instance of the same type as the current instance.The version number of elem is not valid.</exception>
        public abstract void FromXml(SecurityElement elem);

        ///<summary> Returns a System.Security.CodeAccessPermission object that is the intersection of the current instance and the specified object.</summary>
        ///<param name="target">A System.Security.CodeAccessPermission instance to intersect with the current instance.</param>
        ///<returns> A new System.Security.CodeAccessPermission instance that represents the intersection of the current instance andtarget. If the intersection is empty or target is null, returns null. If the  current instance is unrestricted, returns a copy of target. Iftarget is unrestricted, returns a copy of the current instance.</returns>
        ///<exception cref="System.ArgumentException">target is not null and is not a System.Security.CodeAccessPermission object.</exception>
        public abstract IPermission Intersect(IPermission target);

        ///<summary>Determines whether the current instance is a subset of the specified      object.</summary>
        ///<param name="target">A System.Security.CodeAccessPermission instance that is to be tested for the subset relationship.</param>
        ///<returns>true if the current instance is a   subset of target; otherwise, false. If the current   instance is unrestricted, and target is not, returnsfalse. If target is unrestricted, returns true.</returns>
        ///<exception cref="System.ArgumentException">target is not null and is not of type System.Security.CodeAccessPermission.</exception>
        public abstract bool IsSubsetOf(IPermission target);

        ///<summary> Returns the XML representation of the state of the current instance.</summary>
        ///<returns> A System.String containing the XML  representation of the state of the current instance.</returns>
        public override string ToString()
        {
            return null;
        }

        ///<summary> Returns the XML encoding of the current instance.</summary>
        ///<returns>A System.Security.SecurityElement containing an XML encoding of the state of the current instance.</returns>
        public abstract SecurityElement ToXml();

        ///<summary> Returns a System.Security.CodeAccessPermission object that is the union of the current instance and the specified object.</summary>
        ///<param name="other">A System.Security.IPermission object of the same type as the current instance to be combined with the current instance.</param>
        ///<returns>If other is null, returns a copy of the current  instance using the System.Security.IPermission.Copy method.</returns>
        ///<exception cref="System.ArgumentException">other is not of type System.Security.CodeAccessPermission.</exception>
        ///<exception cref="System.NotSupportedException">other is not null.</exception>
        public virtual IPermission Union(IPermission other)
        {
            if (!(other is System.Security.CodeAccessPermission))
            {
                throw new System.ArgumentException(); // other is not of type System.Security.CodeAccessPermission.
            }
            if (null != other)
            {
                throw new System.NotSupportedException(); // other is not null.
            }
            return null;
        }
}
