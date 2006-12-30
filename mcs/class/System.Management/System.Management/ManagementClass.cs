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

namespace System.Management
{
	[Serializable]
	[MonoTODO ("System.Management is not implemented")]
	public class ManagementClass : ManagementObject
	{
		public ManagementClass ()
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (ManagementPath path)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (string path)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (ManagementPath path, ObjectGetOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (string path, ObjectGetOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (ManagementScope scope, ManagementPath path, ObjectGetOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass (string scope, string path, ObjectGetOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public StringCollection Derivation {
			get {
				throw CreateNotImplementedException ();
			}
		}

		public MethodDataCollection Methods {
			get {
				throw CreateNotImplementedException ();
			}
		}

		public override ManagementPath Path {
			get {
				throw CreateNotImplementedException ();
			}
			set {
				throw CreateNotImplementedException ();
			}
		}

		public override object Clone ()
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObject CreateInstance ()
		{
			throw CreateNotImplementedException ();
		}

		public ManagementClass Derive (string newClassName)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetInstances ()
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetInstances (EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public void GetInstances (ManagementOperationObserver watcher)
		{
			throw CreateNotImplementedException ();
		}

		public void GetInstances (ManagementOperationObserver watcher, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses ()
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses (string relatedClass)
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher, string relatedClass)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelatedClasses (string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelatedClasses (ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses ()
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses (string relationshipClass)
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher, string relationshipClass)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetRelationshipClasses (string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public void GetRelationshipClasses (ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public CodeTypeDeclaration GetStronglyTypedClassCode (bool includeSystemClassInClassDef, bool systemPropertyClass)
		{
			throw CreateNotImplementedException ();
		}

		public bool GetStronglyTypedClassCode (CodeLanguage lang, string filePath, string classNamespace)
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetSubclasses ()
		{
			throw CreateNotImplementedException ();
		}

		public ManagementObjectCollection GetSubclasses (EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		public void GetSubclasses (ManagementOperationObserver watcher)
		{
			throw CreateNotImplementedException ();
		}

		public void GetSubclasses (ManagementOperationObserver watcher, EnumerationOptions options)
		{
			throw CreateNotImplementedException ();
		}

		private static NotImplementedException CreateNotImplementedException ()
		{
			return new NotImplementedException ("System.Management is not implemented.");
		}

	}
}