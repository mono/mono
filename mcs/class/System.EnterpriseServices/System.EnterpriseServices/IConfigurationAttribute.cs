// System.EnterpriseServices.IConfigurationAttribute.cs
//
// Author:  Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2004 Novell, Inc.
//

using System.Collections;

namespace System.EnterpriseServices
{
	internal interface IConfigurationAttribute
	{
		bool AfterSaveChanges (Hashtable info);
		bool Apply (Hashtable info);
		bool IsValidTarget (string s);
	}
}
