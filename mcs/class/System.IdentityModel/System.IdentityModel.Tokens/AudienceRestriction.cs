using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class AudienceRestriction
	{
		public Collection<Uri> AllowedAudienceUris { get; private set; }
		public AudienceUriMode AudienceMode { get; set; }

		public AudienceRestriction () {
			AllowedAudienceUris = new Collection<Uri>();
		}

		public AudienceRestriction (AudienceUriMode audienceMode)
			: this ()
		{
			AudienceMode = audienceMode;
		}
	}
}