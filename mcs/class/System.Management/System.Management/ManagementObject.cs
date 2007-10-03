//
// System.Management.ManagementObject
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
	public class ManagementObject : ManagementBaseObject
	{
		[MonoTODO]
		public ManagementObject ()
		{
		}

		[MonoTODO]
		public ManagementObject (ManagementPath path)
		{
		}

		[MonoTODO]
		public ManagementObject (string path)
		{
		}

		[MonoTODO]
		public ManagementObject (ManagementPath path, ObjectGetOptions options)
		{
		}

		[MonoTODO]
		public ManagementObject (string path, ObjectGetOptions options)
		{
		}

		[MonoTODO]
		public ManagementObject (ManagementScope scope, ManagementPath path, ObjectGetOptions options)
		{
		}

		[MonoTODO]
		public ManagementObject (string scopeString, string pathString, ObjectGetOptions options)
		{
		}

		[MonoTODO]
#if NET_2_0
		protected
#else
		public
#endif
		ManagementObject (SerializationInfo info, StreamingContext context)
		{
		}

#if NET_2_0
		[MonoTODO]
		protected override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath CopyTo (ManagementPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath CopyTo (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ManagementOperationObserver watcher, ManagementPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ManagementOperationObserver watcher, string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath CopyTo (ManagementPath path, PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath CopyTo (string path, PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ManagementOperationObserver watcher, ManagementPath path, PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ManagementOperationObserver watcher, string path, PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete (DeleteOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete (ManagementOperationObserver watcher, DeleteOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Get ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Get (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementBaseObject GetMethodParameters (string methodName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelated ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelated (string relatedClass)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelated (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelated (ManagementOperationObserver watcher, string relatedClass)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelated (string relatedClass,
							      string relationshipClass,
							      string relationshipQualifier,
							      string relatedQualifier,
							      string relatedRole,
							      string thisRole,
							      bool classDefinitionsOnly,
							      EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelated (ManagementOperationObserver watcher,
					string relatedClass,
					string relationshipClass,
					string relationshipQualifier,
					string relatedQualifier,
					string relatedRole,
					string thisRole,
					bool classDefinitionsOnly,
					EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelationships ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelationships (string relationshipClass)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelationships (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelationships (ManagementOperationObserver watcher, string relationshipClass)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectCollection GetRelationships (string relationshipClass,
								    string relationshipQualifier,
								    string thisRole,
								    bool classDefinitionsOnly,
								    EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetRelationships (ManagementOperationObserver watcher,
					      string relationshipClass,
					      string relationshipQualifier,
					      string thisRole,
					      bool classDefinitionsOnly,
					      EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object InvokeMethod (string methodName, object [] args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementBaseObject InvokeMethod (string methodName,
							  ManagementBaseObject inParameters,
							  InvokeMethodOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InvokeMethod (ManagementOperationObserver watcher, string methodName, object [] args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InvokeMethod (ManagementOperationObserver watcher,
					  string methodName,
					  ManagementBaseObject inParameters,
					  InvokeMethodOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath Put ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Put (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementPath Put (PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Put (ManagementOperationObserver watcher, PutOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public override ManagementPath ClassPath {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public ObjectGetOptions Options {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual ManagementPath Path {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public ManagementScope Scope {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

