//
// System.Web.Configuration.MachineKeySection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) Copyright 2005, 2010 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;

namespace System.Web.Configuration {

	public sealed class MachineKeySection : ConfigurationSection
	{
		static ConfigurationProperty decryptionProp;
		static ConfigurationProperty decryptionKeyProp;
		static ConfigurationProperty validationProp;
		static ConfigurationProperty validationKeyProp;
		static ConfigurationPropertyCollection properties;
		static MachineKeyValidationConverter converter = new MachineKeyValidationConverter ();
#if NET_4_0
		MachineKeyValidation validation;
#endif

		static MachineKeySection ()
		{
			decryptionProp = new ConfigurationProperty ("decryption", typeof (string), "Auto",
								    PropertyHelper.WhiteSpaceTrimStringConverter,
								    PropertyHelper.NonEmptyStringValidator,
								    ConfigurationPropertyOptions.None);
			decryptionKeyProp = new ConfigurationProperty ("decryptionKey", typeof (string), "AutoGenerate,IsolateApps",
								       PropertyHelper.WhiteSpaceTrimStringConverter,
								       PropertyHelper.NonEmptyStringValidator,
								       ConfigurationPropertyOptions.None);
#if NET_4_0
			validationProp = new ConfigurationProperty ("validation", typeof (string), "HMACSHA256",
								    PropertyHelper.WhiteSpaceTrimStringConverter,
								    PropertyHelper.NonEmptyStringValidator,
								    ConfigurationPropertyOptions.None);
#else
			validationProp = new ConfigurationProperty ("validation", typeof (MachineKeyValidation), 
								    MachineKeyValidation.SHA1, converter,
								    PropertyHelper.DefaultValidator,
								    ConfigurationPropertyOptions.None);
#endif
			validationKeyProp = new ConfigurationProperty ("validationKey", typeof (string), "AutoGenerate,IsolateApps",
								       PropertyHelper.WhiteSpaceTrimStringConverter,
								       PropertyHelper.NonEmptyStringValidator,
								       ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();

			properties.Add (decryptionProp);
			properties.Add (decryptionKeyProp);
			properties.Add (validationProp);
			properties.Add (validationKeyProp);

			Config.AutoGenerate (MachineKeyRegistryStorage.KeyType.Encryption);
			Config.AutoGenerate (MachineKeyRegistryStorage.KeyType.Validation);
		}

#if NET_4_0
		public MachineKeySection ()
		{
			// get DefaultValue from ValidationAlgorithm
			validation = (MachineKeyValidation) converter.ConvertFrom (null, null, ValidationAlgorithm);
		}

		[MonoTODO]
		public MachineKeyCompatibilityMode CompatibilityMode {
			get; set;
		}
#endif

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
			decryption_key = null;
			validation_key = null;
			decryption_template = null;
			validation_template = null;
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("decryption", DefaultValue = "Auto")]
		public string Decryption {
			get { return (string) base [decryptionProp];}
			set {
				decryption_template = MachineKeySectionUtils.GetDecryptionAlgorithm (value);
				base[decryptionProp] = value;
			}
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("decryptionKey", DefaultValue = "AutoGenerate,IsolateApps")]
		public string DecryptionKey {
			get { return (string) base [decryptionKeyProp];}
			set {
				base[decryptionKeyProp] = value;
//				SetDecryptionKey (value);
			}
		}

#if NET_4_0
		// property exists for backward compatibility
		public MachineKeyValidation Validation {
			get { return validation; }
			set {
				if (value == MachineKeyValidation.Custom)
					throw new ArgumentException ();
//				ValidationAlgorithm = value.ToString ();
			}
		}

		[StringValidator (MinLength = 1)]
		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[ConfigurationProperty ("validation", DefaultValue = "HMACSHA256")]
		public string ValidationAlgorithm {
			get { return (string) base [validationProp];}
			set {
				if (value == null)
					return;

				if (value.StartsWith ("alg:"))
					validation = MachineKeyValidation.Custom;
				else
					validation = (MachineKeyValidation) converter.ConvertFrom (null, null, value);

				base[validationProp] = value;
			}
		}
#else
		[TypeConverter (typeof (MachineKeyValidationConverter))]
		[ConfigurationProperty ("validation", DefaultValue = "SHA1")]
		public MachineKeyValidation Validation {
			get { return (MachineKeyValidation) base [validationProp];}
			set { base[validationProp] = value; }
		}
#endif

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("validationKey", DefaultValue = "AutoGenerate,IsolateApps")]
		public string ValidationKey {
			get { return (string) base [validationKeyProp];}
			set {
				base[validationKeyProp] = value;
//				SetValidationKey (value);
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


		internal static MachineKeySection Config {
			get { return WebConfigurationManager.GetSection ("system.web/machineKey") as MachineKeySection; }
		}

		private byte[] decryption_key;
		private byte[] validation_key;
		private SymmetricAlgorithm decryption_template;
		private KeyedHashAlgorithm validation_template;

		internal SymmetricAlgorithm GetDecryptionAlgorithm ()
		{
			// code location to help with unit testing the code
			return MachineKeySectionUtils.GetDecryptionAlgorithm (Decryption);
		}

		// not to be reused outside algorithm and key validation purpose
		private SymmetricAlgorithm DecryptionTemplate {
			get {
				if (decryption_template == null)
					decryption_template = GetDecryptionAlgorithm ();
				return decryption_template;
			}
		}

		internal byte [] GetDecryptionKey ()
		{
			if (decryption_key == null)
				SetDecryptionKey (DecryptionKey);
			return decryption_key;
		}

		void SetDecryptionKey (string key)
		{
			if ((key == null) || key.StartsWith ("AutoGenerate")) {
				decryption_key = AutoGenerate (MachineKeyRegistryStorage.KeyType.Encryption);
			} else {
				try {
					decryption_key = MachineKeySectionUtils.GetBytes (key, key.Length);
					DecryptionTemplate.Key = decryption_key;
				}
				catch {
					decryption_key = null;
					throw new ArgumentException ("Invalid key length");
				}
			}
		}

		internal KeyedHashAlgorithm GetValidationAlgorithm ()
		{
			// code location to help with unit testing the code
			return MachineKeySectionUtils.GetValidationAlgorithm (this);
		}

		// not to be reused outside algorithm and key validation purpose
		private KeyedHashAlgorithm ValidationTemplate {
			get {
				if (validation_template == null)
					validation_template = GetValidationAlgorithm ();
				return validation_template;
			}
		}

		internal byte [] GetValidationKey ()
		{
			if (validation_key == null)
				SetValidationKey (ValidationKey);
			return validation_key;
		}

		// key can be expended for HMAC - i.e. a small key, e.g. 32 bytes, is still accepted as valid
		// the HMAC class already deals with keys larger than what it can use (digested to right size)
		void SetValidationKey (string key)
		{
			if ((key == null) || key.StartsWith ("AutoGenerate")) {
				validation_key = AutoGenerate (MachineKeyRegistryStorage.KeyType.Validation);
			} else {
				try {
					validation_key = MachineKeySectionUtils.GetBytes (key, key.Length);
					ValidationTemplate.Key = validation_key;
				}
				catch (CryptographicException) {
					// second chance, use the key length that the HMAC really wants
					try {
						byte[] expanded_key = new byte [ValidationTemplate.Key.Length];
						Array.Copy (validation_key, 0, expanded_key, 0, validation_key.Length);
						ValidationTemplate.Key = expanded_key;
						validation_key = expanded_key;
					}
					catch {
						validation_key = null;
						throw new ArgumentException ("Invalid key length");
					}
				}
			}
		}

		byte[] AutoGenerate (MachineKeyRegistryStorage.KeyType type)
		{
			byte[] key = null;
#if TARGET_J2EE
			{
#else
			try {
				key = MachineKeyRegistryStorage.Retrieve (type);

				// ensure the stored key is usable with the selection algorithm
				if (type == MachineKeyRegistryStorage.KeyType.Encryption)
					DecryptionTemplate.Key = key;
				else if (type == MachineKeyRegistryStorage.KeyType.Validation)
					ValidationTemplate.Key = key;
			} catch (Exception) {
				key = null;
			}
#endif
			// some algorithms have special needs for key (e.g. length, parity, weak keys...) 
			// so we better ask them to provide a default key (than to generate/use bad ones)
			if (key == null) {
				if (type == MachineKeyRegistryStorage.KeyType.Encryption)
					key = DecryptionTemplate.Key;
				else if (type == MachineKeyRegistryStorage.KeyType.Validation)
					key = ValidationTemplate.Key;
#if !TARGET_J2EE
				MachineKeyRegistryStorage.Store (key, type);
#endif
			}
			return key;
		}
	}
}

#endif
