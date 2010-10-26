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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpCachePolicy
	{
		internal HttpCachePolicy ()
		{
		}

		HttpCacheVaryByContentEncodings vary_by_content_encodings = new HttpCacheVaryByContentEncodings ();
		HttpCacheVaryByHeaders vary_by_headers = new HttpCacheVaryByHeaders ();
		HttpCacheVaryByParams vary_by_params = new HttpCacheVaryByParams ();
		ArrayList validation_callbacks;
		StringBuilder cache_extension;
		internal HttpCacheability Cacheability;
		string etag;
		bool etag_from_file_dependencies;
		bool last_modified_from_file_dependencies;

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
		bool allow_server_caching = true;
		bool set_no_store;
		bool set_no_transform;
		bool valid_until_expires;
		bool omit_vary_star;
		
		public HttpCacheVaryByContentEncodings VaryByContentEncodings {
			get { return vary_by_content_encodings; }
		}

		public HttpCacheVaryByHeaders VaryByHeaders {
			get { return vary_by_headers; }
		}

		public HttpCacheVaryByParams VaryByParams {
			get { return vary_by_params; }
		}

		internal bool AllowServerCaching {
			get { return allow_server_caching; }
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

		internal ArrayList ValidationCallbacks {
			get { return validation_callbacks; }
		}

		internal bool OmitVaryStar {
			get { return omit_vary_star; }
		}

		internal bool ValidUntilExpires {
			get { return valid_until_expires; }
		}

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

		public void SetLastModifiedFromFileDependencies ()
		{
			last_modified_from_file_dependencies = true;
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

		public void SetNoServerCaching ()
		{
			allow_server_caching = false;
		}

		public void SetNoStore ()
		{
			set_no_store = true;
		}

		public void SetNoTransforms ()
		{
			set_no_transform = true;
		}

		public void SetProxyMaxAge (TimeSpan delta)
		{
			if (delta < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("delta");

			if (HaveProxyMaxAge && ProxyMaxAge < delta)
				return;

			ProxyMaxAge = delta;
			HaveProxyMaxAge = true;
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

		public void SetValidUntilExpires (bool validUntilExpires)
		{
			valid_until_expires = validUntilExpires;
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

		internal void SetHeaders (HttpResponse response, NameValueCollection headers)
		{
			bool noCache = false;
			string cc = null;

			switch (Cacheability) {
			case HttpCacheability.Public:
				cc = "public";
				break;

			case HttpCacheability.NoCache:
			case HttpCacheability.ServerAndNoCache:
				noCache = true;
				cc = "no-cache";
				break;

			case HttpCacheability.Private:
			case HttpCacheability.ServerAndPrivate:
			default:
				cc = "private";
				break;
			}

			if (noCache) {
				response.CacheControl = cc;
				if (!allow_response_in_browser_history) {
					headers.Add ("Expires", "-1");
					headers.Add ("Pragma", "no-cache");
				}
			} else {
				if (HaveMaxAge)
					cc = String.Concat (cc, ", max-age=", ((long) MaxAge.TotalSeconds).ToString ());

				if (have_expire_date) {
					string expires = TimeUtil.ToUtcTimeString (expire_date);
					headers.Add ("Expires", expires);
				}
			}

			if (set_no_store)
				cc = String.Concat (cc, ", no-store");
			if (set_no_transform)
				cc = String.Concat (cc, ", no-transform");
			if (cache_extension != null && cache_extension.Length > 0) {
				if (!Strng.IsNullOrEmpty (cc))
					cc = String.Concat (cc, ", ");
				cc = String.Concat (cc, cache_extension.ToString ());
			}
			
			headers.Add ("Cache-Control", cc);

			if (last_modified_from_file_dependencies || etag_from_file_dependencies)
				HeadersFromFileDependencies (response);

			if (etag != null)
				headers.Add ("ETag", etag);

			if (have_last_modified)
				headers.Add ("Last-Modified", TimeUtil.ToUtcTimeString (last_modified));

			if (!vary_by_params.IgnoreParams) {
				string vb = vary_by_params.GetResponseHeaderValue ();
				if (vb != null)
					headers.Add ("Vary", vb);
			}
		}

		void HeadersFromFileDependencies (HttpResponse response)
		{
			string [] fileDeps = response.FileDependencies;

			if (fileDeps == null || fileDeps.Length == 0)
				return;

			bool doEtag = etag != null && etag_from_file_dependencies;
			if (!doEtag && !last_modified_from_file_dependencies)
				return;

			DateTime latest_mod = DateTime.MinValue, mod;
			StringBuilder etagsb = new StringBuilder ();
			
			foreach (string f in fileDeps) {
				if (!File.Exists (f))
					continue;
				try {
					mod = File.GetLastWriteTime (f);
				} catch {
					// ignore
					continue;
				}

				if (last_modified_from_file_dependencies && mod > latest_mod)
					latest_mod = mod;
				if (doEtag)
					etagsb.AppendFormat ("{0}", mod.Ticks.ToString ("x"));
			}

			if (last_modified_from_file_dependencies && latest_mod > DateTime.MinValue) {
				last_modified = latest_mod;
				have_last_modified = true;
			}

			if (doEtag && etagsb.Length > 0)
				etag = etagsb.ToString ();
		}

		public void SetOmitVaryStar (bool omit)
		{
			omit_vary_star = omit;
		}
	}
}
