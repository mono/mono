// 
// System.Web.HttpCachePolicy
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
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

using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web {

	class CacheabilityUpdatedEventArgs : EventArgs {

		public readonly HttpCacheability Cacheability;

		public CacheabilityUpdatedEventArgs (HttpCacheability cacheability)
		{
			Cacheability = cacheability;
		}
	}
	
	internal delegate void CacheabilityUpdatedCallback (object sender, CacheabilityUpdatedEventArgs args);
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpCachePolicy {

		internal HttpCachePolicy ()
		{
		}

#region Fields

		HttpCacheVaryByHeaders vary_by_headers = new HttpCacheVaryByHeaders ();
		HttpCacheVaryByParams vary_by_params = new HttpCacheVaryByParams ();
		ArrayList validation_callbacks;
		StringBuilder cache_extension;
		internal HttpCacheability Cacheability;
		string etag;
		bool etag_from_file_dependencies;

		//
		// Used externally
		//
		internal bool have_expire_date;
		internal DateTime expire_date;
		internal bool have_last_modified;
		internal DateTime last_modified;
		
		//bool LastModifiedFromFileDependencies;
		HttpCacheRevalidation revalidation;
		string vary_by_custom;
		bool HaveMaxAge;
		TimeSpan MaxAge;
		bool HaveProxyMaxAge;
		TimeSpan ProxyMaxAge;
		ArrayList fields;
		bool sliding_expiration;
		int duration;
		bool allow_response_in_browser_history;
                
#endregion

                internal event CacheabilityUpdatedCallback CacheabilityUpdated;
                
#region Properties
                
		public HttpCacheVaryByHeaders VaryByHeaders {
			get { return vary_by_headers; }
		}

		public HttpCacheVaryByParams VaryByParams {
			get { return vary_by_params; }
		}

		internal int Duration {
			get { return duration; }
			set { duration = value; }
		}

		internal bool Sliding {
			get { return sliding_expiration; }
		}
		
                internal DateTime Expires {
                        get { return expire_date; }
                }

#endregion // Properties

#region Methods

		internal int ExpireMinutes ()
		{
			if (!have_expire_date)
				return 0;
			
			return (expire_date - DateTime.Now).Minutes;
		}
		
		public void AddValidationCallback (HttpCacheValidateHandler handler, object data)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			if (validation_callbacks == null)
				validation_callbacks = new ArrayList ();

			validation_callbacks.Add (new Pair (handler, data));
		}

		public void AppendCacheExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (cache_extension == null)
				cache_extension = new StringBuilder (extension);
			else
				cache_extension.Append (", " + extension);
		}

		//
		// This one now allows the full range of Cacheabilities.
		//
		public void SetCacheability (HttpCacheability cacheability)
		{
			if (cacheability < HttpCacheability.NoCache || cacheability > HttpCacheability.ServerAndPrivate)
				throw new ArgumentOutOfRangeException ("cacheability");

			if (Cacheability > 0 && cacheability > Cacheability)
				return;
			
			Cacheability = cacheability;

			if (CacheabilityUpdated != null)
				CacheabilityUpdated (this, new CacheabilityUpdatedEventArgs (cacheability));
		}

		public void SetCacheability (HttpCacheability cacheability, string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			if (cacheability != HttpCacheability.NoCache && cacheability != HttpCacheability.Private)
				throw new ArgumentException ("Must be NoCache or Private", "cacheability");

			if (fields == null)
				fields = new ArrayList ();

			fields.Add (new Pair (cacheability, field));
		}

		public void SetETag (string etag)
		{
			if (etag == null)
				throw new ArgumentNullException ("etag");

			if (this.etag != null)
				throw new InvalidOperationException ("The ETag header has already been set");

			if (etag_from_file_dependencies)
				throw new InvalidOperationException ("SetEtagFromFileDependencies has already been called");

			this.etag = etag;
		}

		public void SetETagFromFileDependencies ()
		{
			if (this.etag != null)
				throw new InvalidOperationException ("The ETag header has already been set");

			etag_from_file_dependencies = true;
		}

		public void SetExpires (DateTime date)
		{
			if (have_expire_date && date > expire_date)
				return;

			have_expire_date = true;
			expire_date = date;
		}

		public void SetLastModified (DateTime date)
		{
			if (date > DateTime.Now)
				throw new ArgumentOutOfRangeException ("date");

			if (have_last_modified && date < last_modified)
				return;

			have_last_modified = true;
			last_modified = date;
		}

		[MonoTODO]
		public void SetLastModifiedFromFileDependencies ()
		{
			throw new NotImplementedException (); 
		}

		public void SetMaxAge (TimeSpan date)
		{
			if (date < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("date");
			
			if (HaveMaxAge && MaxAge < date)
				return;

			MaxAge = date;
			HaveMaxAge = true;
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

		public void SetProxyMaxAge (TimeSpan delta)
		{
			if (delta < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("delta");

			if (HaveProxyMaxAge && ProxyMaxAge < delta)
				return;

			ProxyMaxAge = delta;
		}

		public void SetRevalidation (HttpCacheRevalidation revalidation)
		{
			if (revalidation < HttpCacheRevalidation.AllCaches ||
			    revalidation > HttpCacheRevalidation.None)
				throw new ArgumentOutOfRangeException ("revalidation");

			if (this.revalidation > revalidation)
				this.revalidation = revalidation;
		}

		public void SetSlidingExpiration (bool slide)
		{
			sliding_expiration = slide;
		}

		[MonoTODO]
		public void SetValidUntilExpires (bool validUntilExpires)
		{
			throw new NotImplementedException (); 
		}

		public void SetVaryByCustom (string custom)
		{
			if (custom == null)
				throw new ArgumentNullException ("custom");

			if (vary_by_custom != null)
				throw new InvalidOperationException ("VaryByCustom has already been set.");

			vary_by_custom = custom;
		}

		internal string GetVaryByCustom ()
		{
			return vary_by_custom;
		}

		public void SetAllowResponseInBrowserHistory (bool allow)
		{
			if (Cacheability == HttpCacheability.NoCache || Cacheability == HttpCacheability.ServerAndNoCache) 
				allow_response_in_browser_history = allow;
		}

		internal void SetHeaders (HttpResponse response, ArrayList headers)
		{
			string cc, expires;
			if (Cacheability > HttpCacheability.NoCache) {
				string c = Cacheability.ToString ().ToLower (CultureInfo.InvariantCulture);
				
				if (MaxAge.TotalSeconds != 0)
					cc = String.Format ("{0}, max-age={1}", c, (long) MaxAge.TotalSeconds);
				else
					cc = c;
				
				expires = TimeUtil.ToUtcTimeString (expire_date);
				headers.Add (new UnknownResponseHeader ("Expires", expires));
			} else {
				cc = "no-cache";
				response.CacheControl = cc;
				if (!allow_response_in_browser_history) {
					expires = "-1";
					headers.Add (new UnknownResponseHeader ("Expires", expires));
				}
			}
			
			headers.Add (new UnknownResponseHeader ("Cache-Control", cc));
						
			if (etag != null)
				headers.Add (new UnknownResponseHeader ("ETag", etag));

			if (have_last_modified)
				headers.Add (new UnknownResponseHeader ("Last-Modified",
							     TimeUtil.ToUtcTimeString (last_modified)));

			if (!vary_by_params.IgnoreParams) {
				BaseResponseHeader vb = vary_by_params.GetResponseHeader ();
				if (vb != null)
					headers.Add (vb);
			}

		}
#if NET_2_0
		[MonoTODO]
		public void SetOmitVaryStar (bool omit)
		{
			throw new NotImplementedException (); 
		}
#endif
		
#endregion // Methods
	}
}

