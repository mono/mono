using System;

namespace System.IdentityModel
{
	public abstract class CookieTransform
	{
		public abstract byte[] Encode (byte[] value);
	}
}