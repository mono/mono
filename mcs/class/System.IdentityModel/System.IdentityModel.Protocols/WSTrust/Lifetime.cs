using System;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class Lifetime
	{
		public DateTime? Created { get; set; }
		public DateTime? Expires { get; set; }

		public Lifetime (DateTime created, DateTime expires)
			: this ((DateTime?)created, (DateTime?)expires)
		{ }

		public Lifetime (DateTime? created, DateTime? expires) {
			if (created.HasValue) { Created = created.Value.ToUniversalTime (); }
			if (expires.HasValue) { Expires = expires.Value.ToUniversalTime (); }
		}
	}
}