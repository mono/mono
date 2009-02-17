// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Mono.Cecil;

namespace Moonlight.SecurityModel {

	static public class CecilRocks {

		private const string SecurityCritical = "System.Security.SecurityCriticalAttribute";
		private const string SecuritySafeCritical = "System.Security.SecuritySafeCriticalAttribute";

		public static bool HasAttribute (this CustomAttributeCollection self, string attribute)
		{
			foreach (CustomAttribute ca in self) {
				if (ca.Constructor.DeclaringType.FullName == attribute)
					return true;
			}
			return false;
		}

		public static bool HasAttribute (this MethodDefinition self, string attribute)
		{
			if (!self.HasCustomAttributes)
				return false;

			foreach (CustomAttribute ca in self.CustomAttributes) {
				if (ca.Constructor.DeclaringType.FullName == attribute)
					return true;
			}
			return false;
		}

		public static bool HasAttribute (this TypeDefinition self, string attribute)
		{
			if (!self.HasCustomAttributes)
				return false;

			foreach (CustomAttribute ca in self.CustomAttributes) {
				if (ca.Constructor.DeclaringType.FullName == attribute)
					return true;
			}
			return false;
		}

		public static bool IsSecurityCritical (this CustomAttributeCollection self)
		{
			return HasAttribute (self, SecurityCritical);
		}

		public static bool IsSecurityCritical (this MethodDefinition self)
		{
			if (!self.HasCustomAttributes)
				return IsSecurityCritical (self.DeclaringType);
			return HasAttribute (self.CustomAttributes, SecurityCritical);
		}

		public static bool IsSecurityCritical (this TypeDefinition self)
		{
			bool result = false;
			if (self.HasCustomAttributes)
				result = HasAttribute (self.CustomAttributes, SecurityCritical);

			if (result)
				return true;
			else if (self.IsNested)
				return self.DeclaringType.IsSecurityCritical ();
			else
				return false;
		}

		public static bool IsSecuritySafeCritical (this CustomAttributeCollection self)
		{
			return HasAttribute (self, SecuritySafeCritical);
		}

		public static bool IsSecuritySafeCritical (this MethodDefinition self)
		{
			if (!self.HasCustomAttributes)
				return IsSecuritySafeCritical (self.DeclaringType);
			return HasAttribute (self.CustomAttributes, SecuritySafeCritical);
		}

		public static bool IsSecuritySafeCritical (this TypeDefinition self)
		{
			bool result = false;
			if (self.HasCustomAttributes)
				result = HasAttribute (self.CustomAttributes, SecuritySafeCritical);

			if (result)
				return true;
			else if (self.IsNested)
				return self.DeclaringType.IsSecuritySafeCritical ();
			else
				return false;
		}

		public static bool IsVisible (this TypeDefinition self)
		{
			while (self.IsNested) {
				if (self.IsNestedPrivate || self.IsNestedAssembly)
					return false;
				// Nested classes are always inside the same assembly, so the cast is ok
				self = (self.DeclaringType as TypeDefinition);
			}
			return self.IsPublic;
		}

		public static bool IsVisible (this MethodDefinition self)
		{
			if (self.IsPrivate || self.IsAssembly)
				return false;
			return (self.DeclaringType as TypeDefinition).IsVisible ();
		}
	}
}
