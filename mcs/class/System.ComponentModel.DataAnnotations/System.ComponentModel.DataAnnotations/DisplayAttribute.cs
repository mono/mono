//
// DisplayAttribute.cs
//
// Author:
//	David Stone <david@gixug.com>
//
// Copyright (C) 2010 David Stone
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

namespace System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class DisplayAttribute : Attribute
	{
		public Type ResourceType { get; set; }

		public string Description { get; set; }
		public string GroupName { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Prompt { get; set; }

		const string property_not_set_message = "The {0} property has not been set.  Use the Get{0} method to get the value.";
		const string localization_failed_message = "Cannot retrieve property '{0}' because localization failed. Type '{1} is not public or does not contain a public static string property with the name '{2}'.";

		bool? _autoGenerateField;
		public bool AutoGenerateField {
			get {
				if (!_autoGenerateField.HasValue) {
					throw new InvalidOperationException (string.Format (property_not_set_message, "AutoGenerateField"));
				}
				
				return _autoGenerateField.Value;
			}
			set { _autoGenerateField = value; }
		}

		bool? _autoGenerateFilter;
		public bool AutoGenerateFilter {
			get {
				if (_autoGenerateFilter == null) {
					throw new InvalidOperationException (string.Format (property_not_set_message, "AutoGenerateFilter"));
				}
				
				return _autoGenerateFilter.Value;
			}
			set { _autoGenerateFilter = value; }
		}

		int? _order;
		public int? Order {
			get {
				if (_order == null) {
					throw new InvalidOperationException (string.Format (property_not_set_message, "Order"));
				}
				
				return _order.Value;
			}
			set { _order = value; }
		}

		private string GetLocalizedString (string propertyName, string key)
		{
			// If we don't have a resource or a key, go ahead and fall back on the key
			if (ResourceType == null || key == null)
				return key;
			
			var property = ResourceType.GetProperty (key);
			
			// Strings are only valid if they are public static strings
			var isValid = false;
			if (ResourceType.IsVisible && property != null && property.PropertyType == typeof(string)) {
				var getter = property.GetGetMethod ();
				
				// Gotta have a public static getter on the property
				if (getter != null && getter.IsStatic && getter.IsPublic) {
					isValid = true;
				}
			}
			
			// If it's not valid, go ahead and throw an InvalidOperationException
			if (!isValid) {
				var message = string.Format (localization_failed_message, propertyName, ResourceType.ToString (), key);
				throw new InvalidOperationException (message);
			}
			
			return (string)property.GetValue (null, null);
			
		}

		#region Consumer Methods
		public bool? GetAutoGenerateField ()
		{
			return _autoGenerateField;
		}

		public bool? GetAutoGenerateFilter ()
		{
			return _autoGenerateFilter;
		}
		
		public int? GetOrder ()
		{
			return _order;
		}

		public string GetName ()
		{
			return GetLocalizedString ("Name", Name);
		}

		public string GetShortName ()
		{
			// Short name falls back on Name if the short name isn't set
			return GetLocalizedString ("ShortName", ShortName) ?? GetName ();
		}

		public string GetDescription ()
		{
			return GetLocalizedString ("Description", Description);
		}

		public string GetPrompt ()
		{
			return GetLocalizedString ("Prompt", Prompt);
		}
		
		public string GetGroupName ()
		{
			return GetLocalizedString ("GroupName", GroupName);
		}
		
		#endregion
		
	}
#endif
}

