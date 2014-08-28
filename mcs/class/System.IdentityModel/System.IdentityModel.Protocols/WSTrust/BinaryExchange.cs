using System;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class BinaryExchange
	{
		private const string defaultEncodingTypeUrl = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

		public byte[] BinaryData { get; private set; }
		public Uri EncodingType { get; private set; }
		public Uri ValueType { get; private set; }

		public BinaryExchange (byte[] binaryData, Uri valueType)
			: this (binaryData, valueType, new Uri (defaultEncodingTypeUrl))
		{ }

		public BinaryExchange (byte[] binaryData, Uri valueType, Uri encodingType) {
			BinaryData = binaryData;
			ValueType = valueType;
			EncodingType = encodingType;
		}
	}
}