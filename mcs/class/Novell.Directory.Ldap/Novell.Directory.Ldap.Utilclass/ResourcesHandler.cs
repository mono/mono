/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.ResourcesHandler.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary>  A utility class to get strings from the ExceptionMessages and
	/// ResultCodeMessages resources.
	/// </summary>
	public class ResourcesHandler
	{
		// Cannot create an instance of this class
		private ResourcesHandler()
		{
			return ;
		}
		
		/*
		*  Initialized when the first result string is requested
		*/
		private static System.Resources.ResourceManager defaultResultCodes = null;
		
		/// <summary>  Initialized when the first Exception message string is requested</summary>
		private static System.Resources.ResourceManager defaultMessages = null;
		
		
		/// <summary> Package where resources are found</summary>
		private static System.String pkg = "Novell.Directory.Ldap.Utilclass.";
		
		/// <summary> The default Locale</summary>
		private static System.Globalization.CultureInfo defaultLocale;
		
		/// <summary> Returns a string using the MessageOrKey as a key into
		/// ExceptionMessages or, if the Key does not exist, returns the
		/// string messageOrKey.  In addition it formats the arguments into the message
		/// according to MessageFormat.
		/// 
		/// </summary>
		/// <param name="messageOrKey">   Key string for the resource.
		/// 
		/// </param>
		/// <param name="">arguments
		/// 
		/// </param>
		/// <returns> the text for the message specified by the MessageKey or the Key
		/// if it there is no message for that key.
		/// </returns>
		public static System.String getMessage(System.String messageOrKey, System.Object[] arguments)
		{
			return getMessage(messageOrKey, arguments, null);
		}
		
		/// <summary> Returns the message stored in the ExceptionMessages resource for the
		/// specified locale using messageOrKey and argments passed into the
		/// constructor.  If no string exists in the resource then this returns
		/// the string stored in message.  (This method is identical to
		/// getLdapErrorMessage(Locale locale).)
		/// 
		/// </summary>
		/// <param name="messageOrKey">   Key string for the resource.
		/// 
		/// </param>
		/// <param name="">arguments
		/// 
		/// </param>
		/// <param name="locale">         The Locale that should be used to pull message
		/// strings out of ExceptionMessages.
		/// 
		/// </param>
		/// <returns> the text for the message specified by the MessageKey or the Key
		/// if it there is no message for that key.
		/// </returns>
		public static System.String getMessage(System.String messageOrKey, System.Object[] arguments, System.Globalization.CultureInfo locale)
		{
			if (defaultMessages == null)
			{
				defaultMessages = new ResourceManager ("ExceptionMessages", Assembly.GetExecutingAssembly ());
			}
			
			if (defaultLocale == null)
				defaultLocale = Thread.CurrentThread.CurrentUICulture;

			if (locale == null)
				locale = defaultLocale;

			if (messageOrKey == null)
			{
				messageOrKey = "";
			}
			
			string pattern;
			try
			{
				pattern = defaultMessages.GetString(messageOrKey, locale);
			}
			catch (System.Resources.MissingManifestResourceException mre)
			{
				pattern = messageOrKey;
			}
			
			// Format the message if arguments were passed
			if (arguments != null)
			{
				StringBuilder strB = new StringBuilder();
				strB.AppendFormat(pattern, arguments);
				pattern = strB.ToString();
				//				MessageFormat mf = new MessageFormat(pattern);
				//				pattern=System.String.Format(locale,pattern,arguments);
//				mf.setLocale(locale);
				//this needs to be reset with the new local - i18n defect in java
//				mf.applyPattern(pattern);
//				pattern = mf.format(arguments);
			}
			return pattern;
		}
		
		/// <summary> Returns a string representing the Ldap result code from the 
		/// default ResultCodeMessages resource.
		/// 
		/// </summary>
		/// <param name="code">   the result code 
		/// 
		/// </param>
		/// <returns>        the String representing the result code.
		/// </returns>
		public static System.String getResultString(int code)
		{
			return getResultString(code, null);
		}
		
		/// <summary> Returns a string representing the Ldap result code.  The message
		/// is obtained from the locale specific ResultCodeMessage resource.
		/// 
		/// </summary>
		/// <param name="code">   the result code 
		/// 
		/// </param>
		/// <param name="locale">         The Locale that should be used to pull message
		/// strings out of ResultMessages.
		/// 
		/// </param>
		/// <returns>        the String representing the result code.
		/// </returns>
		public static System.String getResultString(int code, System.Globalization.CultureInfo locale)
		{
			if (defaultResultCodes == null)
			{
/*
				defaultResultCodes = ResourceManager.CreateFileBasedResourceManager("ResultCodeMessages", "Resources", null);*/
				defaultResultCodes = new ResourceManager ("ResultCodeMessages", Assembly.GetExecutingAssembly ());

			}

			if (defaultLocale == null)
				defaultLocale = Thread.CurrentThread.CurrentUICulture;

			if (locale == null)
				locale = defaultLocale;

			string result;
			try
			{
				result = defaultResultCodes.GetString(Convert.ToString(code), defaultLocale);
			}
			catch (ArgumentNullException mre)
			{
				result = getMessage(ExceptionMessages.UNKNOWN_RESULT, new Object[]{code}, locale);
			}
			return result;
		}

		static ResourcesHandler()
		{
			defaultLocale = Thread.CurrentThread.CurrentUICulture;
		}
	} //end class ResourcesHandler
}
