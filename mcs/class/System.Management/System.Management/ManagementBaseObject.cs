//
// System.Management.ManagementBaseObject
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Management
{
#if NET_2_0
	[Serializable]
#endif
	[ToolboxItem (true)]
	public class ManagementBaseObject : Component, ICloneable, ISerializable
	{
		internal ManagementBaseObject ()
		{
		}

		[MonoTODO]
		protected ManagementBaseObject (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static explicit operator IntPtr  (ManagementBaseObject managementObject)
		{
			throw new NotImplementedException ();
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

