//
// System.Management.ManagementBaseObject
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Management
{
	public class ManagementBaseObject : Component, ICloneable, ISerializable
	{
		internal ManagementBaseObject ()
		{
		}

		[MonoTODO]
		public virtual object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CompareTo (ManagementBaseObject otherObject, ComparisonSettings settings)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetPropertyQualifierValue (string propertyName, string qualifierName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetPropertyValue (string propertyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetQualifierValue (string qualifierName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetText (TextFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetPropertyQualifierValue (string propertyName, string qualifierName, object qualifierValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetPropertyValue (string propertyName, object propertyValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetQualifierValue (string qualifierName, object qualifierValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public virtual ManagementPath ClassPath {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public object this [string propertyName] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual PropertyDataCollection Properties {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual QualifierDataCollection Qualifiers {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual PropertyDataCollection SystemProperties {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

