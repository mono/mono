//
// System.Management.ManagementClass
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Gert Driesen
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
using System.CodeDom;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Management
{
	[Serializable]
	[MonoTODO ("System.Management is not implemented")]
	public class ManagementClass : ManagementObject
	{
		public ManagementClass ()
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (ManagementPath path)
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (string path)
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (ManagementPath path, ObjectGetOptions options)
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (string path, ObjectGetOptions options)
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (ManagementScope scope, ManagementPath path, ObjectGetOptions options)
		{
			throw new NotImplementedException ();
		}

		public ManagementClass (string scope, string path, ObjectGetOptions options)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		protected ManagementClass (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

		public StringCollection Derivation {
			get {
				throw new NotImplementedException ();
			}
		}

		public MethodDataCollection Methods {
			get {
				throw new NotImplementedException ();
			}
		}

		public override ManagementPath Path {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		public ManagementObject CreateInstance ()
		{
			throw new NotImplementedException ();
		}

		public ManagementClass Derive (string newClassName)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetInstances ()
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetInstances (EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public void GetInstances (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		public void GetInstances (ManagementOperationObserver watcher, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses ()
		{
			throw new NotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses (string relatedClass)
		{
			throw new NotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher, string relatedClass)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses (string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses ()
		{
			throw new NotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses (string relationshipClass)
		{
			throw new NotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher, string relationshipClass)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses (string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public CodeTypeDeclaration GetStronglyTypedClassCode (bool includeSystemClassInClassDef, bool systemPropertyClass)
		{
			throw new NotImplementedException ();
		}

		public bool GetStronglyTypedClassCode (CodeLanguage lang, string filePath, string classNamespace)
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetSubclasses ()
		{
			throw new NotImplementedException ();
		}

		public ManagementObjectCollection GetSubclasses (EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}

		public void GetSubclasses (ManagementOperationObserver watcher)
		{
			throw new NotImplementedException ();
		}

		public void GetSubclasses (ManagementOperationObserver watcher, EnumerationOptions options)
		{
			throw new NotImplementedException ();
		}
	}
}