/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/

namespace System.Web.Configuration {

	/// <summary>
	/// Defines the password encryption format.
	/// </summary>
	public enum FormsAuthPasswordFormat{
		Clear, 
		SHA1,
		MD5
	}

} //namespace System.Web.Configuration
