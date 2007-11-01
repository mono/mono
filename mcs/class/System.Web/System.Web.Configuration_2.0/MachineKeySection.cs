//
// System.Web.Configuration.MachineKeySection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class MachineKeySection : ConfigurationSection
	{
		static ConfigurationProperty decryptionProp;
		static ConfigurationProperty decryptionKeyProp;
		static ConfigurationProperty validationProp;
		static ConfigurationProperty validationKeyProp;
		static ConfigurationPropertyCollection properties;

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
			validationProp = new ConfigurationProperty ("validation", typeof (MachineKeyValidation), MachineKeyValidation.SHA1,
								    new MachineKeyValidationConverter (),
								    PropertyHelper.DefaultValidator,
								    ConfigurationPropertyOptions.None);
			validationKeyProp = new ConfigurationProperty ("validationKey", typeof (string), "AutoGenerate,IsolateApps",
								       PropertyHelper.WhiteSpaceTrimStringConverter,
								       PropertyHelper.NonEmptyStringValidator,
								       ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();

			properties.Add (decryptionProp);
			properties.Add (decryptionKeyProp);
			properties.Add (validationProp);
			properties.Add (validationKeyProp);

			MachineKeySectionUtils.AutoGenKeys ();
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("decryption", DefaultValue = "Auto")]
		public string Decryption {
			get { return (string) base [decryptionProp];}
			set { base[decryptionProp] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("decryptionKey", DefaultValue = "AutoGenerate,IsolateApps")]
		public string DecryptionKey {
			get { return (string) base [decryptionKeyProp];}
			set {
				base[decryptionKeyProp] = value;
				MachineKeySectionUtils.SetDecryptionKey (value);
			}
		}

		[TypeConverter (typeof (MachineKeyValidationConverter))]
		[ConfigurationProperty ("validation", DefaultValue = "SHA1")]
		public MachineKeyValidation Validation {
			get { return (MachineKeyValidation) base [validationProp];}
			set { base[validationProp] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("validationKey", DefaultValue = "AutoGenerate,IsolateApps")]
		public string ValidationKey {
			get { return (string) base [validationKeyProp];}
			set {
				base[validationKeyProp] = value;
				MachineKeySectionUtils.SetValidationKey (value);
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
