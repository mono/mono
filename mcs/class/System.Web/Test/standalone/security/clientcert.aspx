<!-- you need an updated XSP to handle this correctly -->
<%@ Page language="c#" %>
<html>
	<head>
		<script runat="server">
		string GetValue (byte[] data)
		{
			if ((data == null) || (data.Length == 0))
				return "(empty)";
			return BitConverter.ToString (data);
		}

		void Page_Load (object sender, EventArgs e)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			HttpClientCertificate hcc = Request.ClientCertificate;
			if (hcc.IsPresent) {
				sb.Append ("Client certificate retrieved.");
				sb.AppendFormat ("\n<br>BinaryIssuer: {0}", GetValue (hcc.BinaryIssuer));
				sb.AppendFormat ("\n<br>CertEncoding: {0}", hcc.CertEncoding);
				sb.AppendFormat ("\n<br>Certificate: {0}", GetValue (hcc.Certificate));
				sb.AppendFormat ("\n<br>Cookie: {0}", hcc.Cookie);
				sb.AppendFormat ("\n<br>Flags: {0}", hcc.Flags);
				sb.AppendFormat ("\n<br>IsValid: {0}", hcc.IsValid);
				sb.AppendFormat ("\n<br>KeySize: {0}", hcc.KeySize);
				sb.AppendFormat ("\n<br>PublicKey: {0}", GetValue (hcc.PublicKey));
				sb.AppendFormat ("\n<br>SecretKeySize: {0}", hcc.SecretKeySize);
				sb.AppendFormat ("\n<br>SerialNumber: {0}", hcc.SerialNumber);
				sb.AppendFormat ("\n<br>ServerIssuer: {0}", hcc.ServerIssuer);
				sb.AppendFormat ("\n<br>ServerSubject: {0}", hcc.ServerSubject);
				sb.AppendFormat ("\n<br>Subject: {0}", hcc.Subject);
				sb.AppendFormat ("\n<br>ValidFrom: {0}", hcc.ValidFrom);
				sb.AppendFormat ("\n<br>ValidUntil: {0}", hcc.ValidUntil);

				System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate (hcc.Certificate);
				sb.AppendFormat ("\n<br>X509Certificate: {0}", cert.ToString (true).Replace ("\n", "\n<br>"));
			} else {
				sb.Append ("No client certificate");
				if (Request.IsSecureConnection) {
					sb.Append (" was sent during negotiation.");
				} else {
					sb.Append (", and this can't work unless you use HTTPS!");
				}
			}
			info.Text = sb.ToString ();
		}
		</script>
	</head>
	<body>
		<form runat="server">
			<asp:Label id="info" runat="server" />
		</form>
	</body>
</html>
