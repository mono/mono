using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Security.Tokens
{
	internal class DerivedKeySecurityToken : SecurityToken
	{
		string algorithm;
		SecurityKeyIdentifierClause reference;
		SecurityToken resolved_token; // store resolved one.
		int? generation, offset, length;
		// properties
		string id, name, label;
		byte [] nonce;
		ReadOnlyCollection<SecurityKey> keys;
		ReferenceList reflist;

		public DerivedKeySecurityToken (string id, string algorithm,
			SecurityKeyIdentifierClause reference,
			SymmetricSecurityKey referencedKey,
			string name,
			int? generation,
			int? offset,
			int? length,
			string label,
			byte [] nonce)
		{
			algorithm = algorithm ?? SecurityAlgorithms.Psha1KeyDerivation;

			this.id = id;
			this.algorithm = algorithm;
			this.reference = reference;
			this.generation = generation;
			this.offset = offset;
			this.length = length;
			this.nonce = nonce;
			this.name = name;
			this.label = label;

			SecurityKey key = new InMemorySymmetricSecurityKey (
				referencedKey.GenerateDerivedKey (
					algorithm,
					Encoding.UTF8.GetBytes (label ?? Constants.WsscDefaultLabel),
					nonce,
					(length ?? 32) * 8,
					offset ?? 0));
			keys = new ReadOnlyCollection<SecurityKey> (
				new SecurityKey [] {key});
		}

		public override string Id {
			get { return id; }
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { return keys; }
		}

		public override DateTime ValidFrom {
			get { return resolved_token.ValidFrom; }
		}

		public override DateTime ValidTo {
			get { return resolved_token.ValidTo; }
		}

		internal ReferenceList ReferenceList {
			get { return reflist; }
			set { reflist = value; }
		}

		public SecurityKeyIdentifierClause TokenReference {
			get { return reference; }
		}

		public int? Generation {
			get { return generation; }
		}

		public int? Length {
			get { return length; }
		}

		public int? Offset {
			get { return offset; }
		}

		public string Label {
			get { return label; }
		}

		public byte [] Nonce {
			get { return nonce; }
		}

		public string Name {
			get { return name; }
		}

		public override bool MatchesKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			LocalIdKeyIdentifierClause l = keyIdentifierClause
				as LocalIdKeyIdentifierClause;
			return l != null && l.LocalId == Id;
		}

		public override SecurityKey ResolveKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			return MatchesKeyIdentifierClause (keyIdentifierClause) ?
				keys [0] : null;
		}
	}
}
