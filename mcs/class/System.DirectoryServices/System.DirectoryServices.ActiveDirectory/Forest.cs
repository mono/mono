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
	public class Forest : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlySiteCollection Sites {
			get {
				throw new NotImplementedException ();
			}
		}

		public DomainCollection Domains {
			get {
				throw new NotImplementedException ();
			}
		}

		public GlobalCatalogCollection GlobalCatalogs {
			get {
				throw new NotImplementedException ();
			}
		}

		public ApplicationPartitionCollection ApplicationPartitions {
			get {
				throw new NotImplementedException ();
			}
		}

		public ForestMode ForestMode {
			get {
				throw new NotImplementedException ();
			}
		}

		public Domain RootDomain {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchema Schema {
			get {
				throw new NotImplementedException ();
			}
		}

		public DomainController SchemaRoleOwner {
			get {
				throw new NotImplementedException ();
			}
		}

		public DomainController NamingRoleOwner {
			get {
				throw new NotImplementedException ();
			}
		}

		public void Dispose ()
		{
			this.Dispose (true);
		}

		protected void Dispose (bool disposing)
		{

		}

		public static Forest GetForest (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public void RaiseForestFunctionality (ForestMode forestMode)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalog FindGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalog FindGlobalCatalog (string siteName)
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalog FindGlobalCatalog (LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalog FindGlobalCatalog (string siteName, LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalogCollection FindAllGlobalCatalogs ()
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalogCollection FindAllGlobalCatalogs (string siteName)
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs ()
		{
			throw new NotImplementedException ();
		}

		public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs (string siteName)
		{
			throw new NotImplementedException ();
		}

		public TrustRelationshipInformationCollection GetAllTrustRelationships ()
		{
			throw new NotImplementedException ();
		}

		public ForestTrustRelationshipInformation GetTrustRelationship (string targetForestName)
		{
			throw new NotImplementedException ();
		}

		public bool GetSelectiveAuthenticationStatus (string targetForestName)
		{
			throw new NotImplementedException ();
		}

		public void SetSelectiveAuthenticationStatus (string targetForestName, bool enable)
		{
			throw new NotImplementedException ();
		}

		public bool GetSidFilteringStatus (string targetForestName)
		{
			throw new NotImplementedException ();
		}

		public void SetSidFilteringStatus (string targetForestName, bool enable)
		{
			throw new NotImplementedException ();
		}

		public void DeleteLocalSideOfTrustRelationship (string targetForestName)
		{
			throw new NotImplementedException ();
		}

		public void DeleteTrustRelationship (Forest targetForest)
		{
			throw new NotImplementedException ();
		}

		public void VerifyOutboundTrustRelationship (string targetForestName)
		{
			throw new NotImplementedException ();
		}

		public void VerifyTrustRelationship (Forest targetForest, TrustDirection direction)
		{
			throw new NotImplementedException ();
		}

		public void CreateLocalSideOfTrustRelationship (string targetForestName, TrustDirection direction, string trustPassword)
		{
			throw new NotImplementedException ();
		}

		public void CreateTrustRelationship (Forest targetForest, TrustDirection direction)
		{
			throw new NotImplementedException ();
		}

		public void UpdateLocalSideOfTrustRelationship (string targetForestName, string newTrustPassword)
		{
			throw new NotImplementedException ();
		}

		public void UpdateLocalSideOfTrustRelationship (string targetForestName, TrustDirection newTrustDirection, string newTrustPassword)
		{
			throw new NotImplementedException ();
		}

		public void UpdateTrustRelationship (Forest targetForest, TrustDirection newTrustDirection)
		{
			throw new NotImplementedException ();
		}

		public void RepairTrustRelationship (Forest targetForest)
		{
			throw new NotImplementedException ();
		}

		public static Forest GetCurrentForest ()
		{
			throw new NotImplementedException ();
		}
	}
}
