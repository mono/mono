// NameFilter.cs
//
// Copyright 2005 John Reilly
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.


using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ICSharpCode.SharpZipLib.Core
{
	/// <summary>
	/// NameFilter is a string matching class which allows for both positive and negative
	/// matching.
	/// A filter is a sequence of independant <see cref="Regex"></see> regular expressions separated by semi-colons ';'
	/// Each expression can be prefixed by a plus '+' sign or a minus '-' sign to denote the expression
	/// is intended to include or exclude names.  If neither a plus or minus sign is found include is the default
	/// A given name is tested for inclusion before checking exclusions.  Only names matching an include spec 
	/// and not matching an exclude spec are deemed to match the filter.
	/// An empty filter matches any name.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class NameFilter
	{
		/// <summary>
		/// Construct an instance based on the filter expression passed
		/// </summary>
		/// <param name="filter">The filter expression.</param>
		public NameFilter(string filter)
		{
			this.filter = filter;
			inclusions = new ArrayList();
			exclusions = new ArrayList();
			Compile();
		}

		/// <summary>
		/// Test a string to see if it is a valid regular expression.
		/// </summary>
		/// <param name="e">The expression to test.</param>
		/// <returns>True if expression is a valid <see cref="System.Text.RegularExpressions.Regex"/> false otherwise.</returns>
		public static bool IsValidExpression(string e)
		{
			bool result = true;
			try {
				Regex exp = new Regex(e, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			}
			catch {
				result = false;
			}
			return result;
		}

		/// <summary>
		/// Test an expression to see if it is valid as a filter.
		/// </summary>
		/// <param name="toTest">The filter expression to test.</param>
		/// <returns>True if the expression is valid, false otherwise.</returns>
		public static bool IsValidFilterExpression(string toTest)
		{
			bool result = true;
		
			try
			{
				string[] items = toTest.Split(';');
				for (int i = 0; i < items.Length; ++i) {
					if (items[i] != null && items[i].Length > 0) {
						string toCompile;
			
						if (items[i][0] == '+')
							toCompile = items[i].Substring(1, items[i].Length - 1);
						else if (items[i][0] == '-')
							toCompile = items[i].Substring(1, items[i].Length - 1);
						else
							toCompile = items[i];
			
						Regex testRE = new Regex(toCompile, RegexOptions.IgnoreCase | RegexOptions.Singleline);
					}
				}
			}
			catch (Exception) {
				result = false;
		 	}
		
			return result;
		}

		/// <summary>
		/// Convert this filter to its string equivalent.
		/// </summary>
		/// <returns>The string equivalent for this filter.</returns>
		public override string ToString()
		{
			return filter;
		}
		
		/// <summary>
		/// Test a value to see if it is included by the filter.
		/// </summary>
		/// <param name="testValue">The value to test.</param>
		/// <returns>True if the value is included, false otherwise.</returns>
		public bool IsIncluded(string testValue)
		{
			bool result = false;
			if (inclusions.Count == 0)
				result = true;
			else {
				foreach (Regex r in inclusions) {
					if (r.IsMatch(testValue)) {
						result = true;
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Test a value to see if it is excluded by the filter.
		/// </summary>
		/// <param name="testValue">The value to test.</param>
		/// <returns>True if the value is excluded, false otherwise.</returns>
		public bool IsExcluded(string testValue)
		{
			bool result = false;
			foreach (Regex r in exclusions) {
				if (r.IsMatch(testValue)) {
					result = true;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Test a value to see if it matches the filter.
		/// </summary>
		/// <param name="testValue">The value to test.</param>
		/// <returns>True if the value matches, false otherwise.</returns>
		public bool IsMatch(string testValue)
		{
			return IsIncluded(testValue) == true && IsExcluded(testValue) == false;
		}

		/// <summary>
		/// Compile this filter.
		/// </summary>
		void Compile()
		{
			if (filter == null)
				return;

			string[] items = filter.Split(';');
			for (int i = 0; i < items.Length; ++i) {
				if (items[i] != null && items[i].Length > 0) {
					bool include = items[i][0] != '-';
					string toCompile;

					if (items[i][0] == '+')
						toCompile = items[i].Substring(1, items[i].Length - 1);
					else if (items[i][0] == '-')
						toCompile = items[i].Substring(1, items[i].Length - 1);
					else
						toCompile = items[i];

					// NOTE: Regular expressions can fail to compile here for a number of reasons that cause an exception
					// these are left unhandled here as the caller is responsible for ensuring all is valid.
					// several functions IsValidFilterExpression and IsValidExpression are provided for such checking
					if (include)
						inclusions.Add(new Regex(toCompile, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
					else
						exclusions.Add(new Regex(toCompile, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline));
				}
			}
		}

		#region Instance Fields
			string filter;
			ArrayList inclusions;
			ArrayList exclusions;
		#endregion
	}
}
