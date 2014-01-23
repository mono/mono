/******************************************************************************
* The MIT License
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
using System;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	public class ActiveDirectorySchemaClass : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public string CommonName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string Oid {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string Description {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool IsDefunct {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaClassCollection PossibleSuperiors {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlyActiveDirectorySchemaClassCollection PossibleInferiors {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaPropertyCollection MandatoryProperties {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaPropertyCollection OptionalProperties {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaClassCollection AuxiliaryClasses {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaClass SubClassOf {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public SchemaClassType Type {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Guid SchemaGuid {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySecurity DefaultObjectSecurityDescriptor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchemaClass (DirectoryContext context, string ldapDisplayName)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{

		}

		protected virtual void Dispose (bool disposing)
		{

		}

		public static ActiveDirectorySchemaClass FindByName (DirectoryContext context, string ldapDisplayName)
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection GetAllProperties ()
		{
			throw new NotImplementedException ();
		}

		public void Save ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public DirectoryEntry GetDirectoryEntry ()
		{
			throw new NotImplementedException ();
		}

	}
}
