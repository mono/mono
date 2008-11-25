// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;
using System.Globalization;

namespace NUnit.Core
{
	public class CultureDetector
	{
		private CultureInfo currentCulture;

		// Set whenever we fail to support a list of platforms
		private string reason = string.Empty;

		/// <summary>
		/// Default constructor uses the current culutre.
		/// </summary>
		public CultureDetector()
		{
			this.currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
		}

		/// <summary>
		/// Contruct a CultureHelper for a particular culture for testing.
		/// </summary>
		/// <param name="culture">The culture to be used</param>
		public CultureDetector( string culture )
		{
			this.currentCulture = new CultureInfo( culture );
		}

		/// <summary>
		/// Test to determine if one of a collection of culturess
		/// is being used currently.
		/// </summary>
		/// <param name="cultures"></param>
		/// <returns></returns>
		public bool IsCultureSupported( string[] cultures )
		{
			foreach( string culture in cultures )
				if ( IsCultureSupported( culture ) )
					return true;

			return false;
		}

		/// <summary>
		/// Tests to determine if the current culture is supported
		/// based on a culture attribute.
		/// </summary>
		/// <param name="platformAttribute">The attribute to examine</param>
		/// <returns></returns>
		public bool IsCultureSupported( Attribute cultureAttribute )
		{
			//Use reflection to avoid dependency on a particular framework version
			string include = (string)Reflect.GetPropertyValue( 
				cultureAttribute, "Include", 
				BindingFlags.Public | BindingFlags.Instance );

			string exclude = (string)Reflect.GetPropertyValue(
				cultureAttribute, "Exclude", 
				BindingFlags.Public | BindingFlags.Instance );

			try
			{
				if (include != null && !IsCultureSupported(include))
				{
					reason = string.Format("Only supported under culture {0}", include);
					return false;
				}

				if (exclude != null && IsCultureSupported(exclude))
				{
					reason = string.Format("Not supported under culture {0}", exclude);
					return false;
				}
			}
			catch( ArgumentException ex )
			{
				reason = string.Format( "Invalid culture: {0}", ex.ParamName );
				return false; 
			}

			return true;
		}

		/// <summary>
		/// Test to determine if the a particular culture or comma-
		/// delimited set of cultures is in use.
		/// </summary>
		/// <param name="platform">Name of the culture or comma-separated list of culture names</param>
		/// <returns>True if the culture is in use on the system</returns>
		public bool IsCultureSupported( string culture )
		{
			culture = culture.Trim();

			if ( culture.IndexOf( ',' ) >= 0 )
			{
				if ( IsCultureSupported( culture.Split( new char[] { ',' } ) ) )
					return true;
			}
			else
			{
				if( this.currentCulture.Name == culture || this.currentCulture.TwoLetterISOLanguageName == culture)
					return true;
			}

			this.reason = "Only supported under culture " + culture;
			return false;
		}

		/// <summary>
		/// Return the last failure reason. Results are not
		/// defined if called before IsSupported( Attribute )
		/// is called.
		/// </summary>
		public string Reason
		{
			get { return reason; }
		}
	}
}
