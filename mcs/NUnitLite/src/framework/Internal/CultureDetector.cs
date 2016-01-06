// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;
using System.Reflection;
using System.Globalization;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// CultureDetector is a helper class used by NUnit to determine
    /// whether a test should be run based on the current culture.
    /// </summary>
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
            this.currentCulture = CultureInfo.CurrentCulture;
		}

		/// <summary>
		/// Contruct a CultureDetector for a particular culture for testing.
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
		/// <param name="cultureAttribute">The attribute to examine</param>
		/// <returns></returns>
		public bool IsCultureSupported( CultureAttribute cultureAttribute )
		{
            string include = cultureAttribute.Include;
            string exclude = cultureAttribute.Exclude;

            //try
            //{
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
            //}
            //catch( ArgumentException ex )
            //{
            //    reason = string.Format( "Invalid culture: {0}", ex.ParamName );
            //    return false; 
            //}

			return true;
		}

		/// <summary>
		/// Test to determine if the a particular culture or comma-
		/// delimited set of cultures is in use.
		/// </summary>
		/// <param name="culture">Name of the culture or comma-separated list of culture names</param>
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
