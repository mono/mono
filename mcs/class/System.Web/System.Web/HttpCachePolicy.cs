// 
// System.Web.HttpCachePolicy
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Tim Coleman (tim@timcoleman.com)
//
using System;

namespace System.Web {
	public sealed class HttpCachePolicy {

		#region Fields

		HttpCacheVaryByHeaders varyByHeaders;
		HttpCacheVaryByParams varyByParams;

		#endregion

		#region Properties

		public HttpCacheVaryByHeaders VaryByHeaders {
			get { return varyByHeaders; }
		}

		public HttpCacheVaryByParams VaryByParams {
			get { return varyByParams; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void AddValidationCallback (HttpCacheValidateHandler handler, object data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AppendCacheExtension (string extension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetCacheability (HttpCacheability cacheability)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetCacheability (HttpCacheability cacheability, string field)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetETag (string etag)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetETagFromFileDependencies ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetExpires (DateTime date)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetLastModified (DateTime date)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetLastModifiedFromFileDependencies ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetMaxAge (TimeSpan date)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetNoServerCaching ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetNoStore ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetNoTransforms ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetProxyMaxAge (TimeSpan delta)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetRevalidation (HttpCacheRevalidation revalidation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSlidingExpiration (bool slide)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetValidUntilExpires (bool validUntilExpires)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetVaryByCustom (string custom)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
