using System.IdentityModel.Tokens;

namespace System.IdentityModel.Configuration
{
	public sealed class IdentityModelCaches
	{
		public SessionSecurityTokenCache SessionSecurityTokenCache { get; set; }
		public TokenReplayCache TokenReplayCache { get; set; }
	}
}