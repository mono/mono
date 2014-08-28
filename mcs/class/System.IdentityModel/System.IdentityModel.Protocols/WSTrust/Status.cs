using System;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class Status
	{
		public string Code { get; set; }
		public string Reason { get; set; }

		public Status (string code, string reason) {
			Code = code;
			Reason = reason;
		}
	}
}