//
// System.Resources.NeutralResourcesLanguageAttribute.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

namespace System.Resources
{
	   [AttributeUsage (AttributeTargets.Assembly)]
	   public sealed class NeutralResourcesLanguageAttribute : Attribute
	   {

			 string culture;

			 // Constructors
			 public NeutralResourcesLanguageAttribute (string cultureName)
			 {
				    culture = cultureName;
			 }
			 public string CultureName
			 {
				    get { return culture; }
			 }
	   }
}
