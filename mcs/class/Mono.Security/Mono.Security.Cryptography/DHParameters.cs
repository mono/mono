//
// DHParameters.cs: Defines a structure that holds the parameters of the Diffie-Hellman algorithm
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//

using System;

namespace Mono.Security.Cryptography {
	/// <summary>
	/// Represents the parameters of the Diffie-Hellman algorithm.
	/// </summary>
	[Serializable]
	public struct DHParameters {
		/// <summary>
		/// Represents the public <b>P</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		public byte[] P;
		/// <summary>
		/// Represents the public <b>G</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		public byte[] G;
		/// <summary>
		/// Represents the private <b>X</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		[NonSerialized]
		public byte[] X;
	}
}