/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Utilclass;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  Encapsulates optional additional parameters or constraints to be applied to
	/// an Ldap operation.
	/// 
	/// When included with LdapConstraints or LdapSearchConstraints
	/// on an LdapConnection or with a specific operation request, it is
	/// sent to the server along with operation requests.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.ResponseControls">
	/// </seealso>
	/// <seealso cref="LdapConstraints.getControls">
	/// </seealso>
	/// <seealso cref="LdapConstraints.setControls">
	/// </seealso>
	public class LdapControl : System.ICloneable
	{
		/// <summary> Returns the identifier of the control.
		/// 
		/// </summary>
		/// <returns> The object ID of the control.
		/// </returns>
		virtual public System.String ID
		{
			get
			{
				return new System.Text.StringBuilder(control.ControlType.stringValue()).ToString();
			}
			
		}
		/// <summary> Returns whether the control is critical for the operation.
		/// 
		/// </summary>
		/// <returns> Returns true if the control must be supported for an associated
		/// operation to be executed, and false if the control is not required for
		/// the operation.
		/// </returns>
		virtual public bool Critical
		{
			get
			{
				return control.Criticality.booleanValue();
			}
			
		}
		internal static RespControlVector RegisteredControls
		{
			/* package */
			
			get
			{
				return registeredControls;
			}
			
		}
		/// <summary> Returns the RFC 2251 Control object.
		/// 
		/// </summary>
		/// <returns> An ASN.1 RFC 2251 Control.
		/// </returns>
		virtual internal RfcControl Asn1Object
		{
			/*package*/
			
			get
			{
				return control;
			}
			
		}
		
		private static RespControlVector registeredControls;
		
		private RfcControl control; // An RFC 2251 Control
		
		/// <summary> Constructs a new LdapControl object using the specified values.
		/// 
		/// </summary>
		/// <param name="oid">    The OID of the control, as a dotted string.
		/// 
		/// </param>
		/// <param name="critical">  True if the Ldap operation should be discarded if
		/// the control is not supported. False if
		/// the operation can be processed without the control.
		/// 
		/// </param>
		/// <param name="values">    The control-specific data.
		/// </param>
		[CLSCompliantAttribute(false)]
		public LdapControl(System.String oid, bool critical, sbyte[] values)
		{
			if ((System.Object) oid == null)
			{
				throw new System.ArgumentException("An OID must be specified");
			}
			if (values == null)
			{
				control = new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical));
			}
			else
			{
				control = new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical), new Asn1OctetString(values));
			}
			return ;
		}
		
		/// <summary> Create an LdapControl from an existing control.</summary>
		protected internal LdapControl(RfcControl control)
		{
			this.control = control;
			return ;
		}
		
		/// <summary> Returns a copy of the current LdapControl object.
		/// 
		/// </summary>
		/// <returns> A copy of the current LdapControl object.
		/// </returns>
		public System.Object Clone()
		{
			LdapControl cont;
			try
			{
				cont = (LdapControl) base.MemberwiseClone();
			}
			catch (System.Exception ce)
			{
				throw new System.SystemException("Internal error, cannot create clone");
			}
			sbyte[] vals = this.getValue();
			sbyte[] twin = null;
			if (vals != null)
			{
				//is this necessary?
				// Yes even though the contructor above allocates a
				// new Asn1OctetString, vals in that constuctor
				// is only copied by reference
				twin = new sbyte[vals.Length];
				for (int i = 0; i < vals.Length; i++)
				{
					twin[i] = vals[i];
				}
				cont.control = new RfcControl(new RfcLdapOID(ID), new Asn1Boolean(Critical), new Asn1OctetString(twin));
			}
			return cont;
		}
		
		/// <summary> Returns the control-specific data of the object.
		/// 
		/// </summary>
		/// <returns> The control-specific data of the object as a byte array,
		/// or null if the control has no data.
		/// </returns>
		[CLSCompliantAttribute(false)]
		public virtual sbyte[] getValue()
		{
			sbyte[] result = null;
			Asn1OctetString val = control.ControlValue;
			if (val != null)
			{
				result = val.byteValue();
			}
			return result;
		}
		
		
		/// <summary> Sets the control-specific data of the object.  This method is for
		/// use by an extension of LdapControl.
		/// </summary>
		[CLSCompliantAttribute(false)]
		protected internal virtual void  setValue(sbyte[] controlValue)
		{
			control.ControlValue = new Asn1OctetString(controlValue);
			return ;
		}
		
		/// <summary> Registers a class to be instantiated on receipt of a control with the
		/// given OID.
		/// 
		/// Any previous registration for the OID is overridden. The
		/// controlClass must be an extension of LdapControl.
		/// 
		/// </summary>
		/// <param name="oid">           The object identifier of the control.
		/// 
		/// </param>
		/// <param name="controlClass">  A class which can instantiate an LdapControl.
		/// </param>
		public static void  register(System.String oid, System.Type controlClass)
		{
			registeredControls.registerResponseControl(oid, controlClass);
			return ;
		}
		static LdapControl()
		{
			registeredControls = new RespControlVector(5, 5);
		}
	}
}
