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

using System;

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
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

