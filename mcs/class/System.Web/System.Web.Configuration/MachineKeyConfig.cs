//
// System.Web.Configuration.MachineKeyConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Configuration;
using System.Xml;
using System.Security.Cryptography;
using System.Web.Util;

namespace System.Web.Configuration
{
	class MachineKeyConfig
	{
		byte[] decryption_key;
		byte[] validation_key;
		string decryption_key_name;
		string validation_key_name;
		SymmetricAlgorithm decryption_template;
		KeyedHashAlgorithm validation_template;
		MachineKeyValidation validation_type;

		internal static MachineKeyConfig Config {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					return null;
				
				return context.GetConfig ("system.web/machineKey") as MachineKeyConfig;
			}
		}
		
		// not to be reused outside algorithm and key validation purpose
		SymmetricAlgorithm DecryptionTemplate {
			get {
				if (decryption_template == null)
					decryption_template = GetDecryptionAlgorithm ();
				return decryption_template;
			}
		}

		// not to be reused outside algorithm and key validation purpose
		KeyedHashAlgorithm ValidationTemplate {
			get {
				if (validation_template == null)
					validation_template = GetValidationAlgorithm ();
				return validation_template;
			}
		}
		
		internal MachineKeyConfig (object parent)
		{
			if (parent is MachineKeyConfig) {
				MachineKeyConfig p = (MachineKeyConfig) parent;
				validation_key = p.validation_key;
				decryption_key = p.decryption_key;
				validation_type = p.validation_type;
			}
		}

		internal byte [] GetDecryptionKey ()
		{
			if (decryption_key == null)
				SetDecryptionKey (decryption_key_name);
			return decryption_key;
		}

		internal void SetDecryptionKey (string key)
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
			return MachineKeySectionUtils.GetValidationAlgorithm (this.ValidationType);
		}

		internal SymmetricAlgorithm GetDecryptionAlgorithm ()
		{
			string name;

			if (decryption_key_name == null || decryption_key_name.StartsWith ("AutoGenerate"))
				name = "Auto";
			else
				name = decryption_key_name;
			
			return MachineKeySectionUtils.GetDecryptionAlgorithm (name);
		}

		internal MachineKeyValidation ValidationType {
			get {
				return validation_type;
			}
			set {
				validation_type = value;
			}
		}

		internal byte [] GetValidationKey ()
		{
			if (validation_key == null)
				SetValidationKey (validation_key_name);
			return validation_key;
		}		

		// key can be expended for HMAC - i.e. a small key, e.g. 32 bytes, is still accepted as valid
		// the HMAC class already deals with keys larger than what it can use (digested to right size)
		internal void SetValidationKey (string key)
		{
			validation_key_name = key;
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

