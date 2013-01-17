//
// System.ComponentModel.RecommendedAsConfigurableAttribute
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	[Obsolete ("Use SettingsBindableAttribute instead of RecommendedAsConfigurableAttribute")]
	public class RecommendedAsConfigurableAttribute : Attribute {

		#region Fields

		private bool recommendedAsConfigurable;

		public static readonly RecommendedAsConfigurableAttribute Default = new RecommendedAsConfigurableAttribute (false);
		public static readonly RecommendedAsConfigurableAttribute No = new RecommendedAsConfigurableAttribute (false);
		public static readonly RecommendedAsConfigurableAttribute Yes = new RecommendedAsConfigurableAttribute (true);

		#endregion // Fields

		#region Constructors

		public RecommendedAsConfigurableAttribute (bool recommendedAsConfigurable)
		{
			this.recommendedAsConfigurable = recommendedAsConfigurable;
		}

		#endregion // Constructors

		#region Properties

		public bool RecommendedAsConfigurable {
			get { return recommendedAsConfigurable; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			if (!(obj is RecommendedAsConfigurableAttribute))
				return false;
			return ((RecommendedAsConfigurableAttribute) obj).RecommendedAsConfigurable == recommendedAsConfigurable;
		}

		public override int GetHashCode ()
		{
			return recommendedAsConfigurable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return recommendedAsConfigurable == RecommendedAsConfigurableAttribute.Default.RecommendedAsConfigurable;
		}

		#endregion // Methods
	}
}
