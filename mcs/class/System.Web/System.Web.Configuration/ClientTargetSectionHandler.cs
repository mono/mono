/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/
using System;
using System.Configuration;

namespace System.Web.Configuration
{
	/// <summary>
	/// Summary description for ClientTargetSectionHandler.
	/// </summary>
	class ClientTargetSectionHandler: NameValueSectionHandler
	{
		/// <summary>
		///		ClientTargetSectionHandler Constructor
		/// </summary>
		public ClientTargetSectionHandler(){}

		/// <summary>
		///		Gets the name of the key in the key-value pair.
		/// </summary>
		protected override string KeyAttributeName
		{
			get
			{
				return "alias";
			}
		}

		/// <summary>
		///		Gets the value for the key in the key-value pair.
		/// </summary>
		protected override string ValueAttributeName
		{
			get
			{
				return "userAgent";
			}
		}

	}
} //namespace System.Web.Configuration
