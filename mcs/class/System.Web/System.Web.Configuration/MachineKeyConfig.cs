//
// System.Web.Configuration.MachineKeyConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class MachineKeyConfig
	{
		static MachineKeyConfig machineKey;
		byte [] validationKey;
		byte [] decryptionKey;
		string validationType;

		internal MachineKeyConfig (object parent)
		{
			if (parent is MachineKeyConfig) {
				MachineKeyConfig p = (MachineKeyConfig) parent;
				validationKey = p.validationKey;
				decryptionKey = p.decryptionKey;
				validationType = p.validationType;
			}
		}

		internal byte [] ValidationKey {
			get { return validationKey; }
			set { validationKey = value; }
		}

		internal byte [] DecryptionKey {
			get { return decryptionKey; }
			set { decryptionKey = value; }
		}

		internal string ValidationType {
			get {
				if (validationType == null)
					validationType = "SHA1";

				return validationType;
			}
			set {
				if (value == null)
					return;

				validationType = value;
			}
		}

		internal static MachineKeyConfig MachineKey {
			get { return machineKey; }
			set { machineKey = value; }
		}
	}
}

