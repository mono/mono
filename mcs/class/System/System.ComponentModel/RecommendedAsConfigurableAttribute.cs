//
// System.ComponentModel.RecommendedAsConfigurableAttribute
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.Property)]
	public class RecommendedAsConfigurableAttribute : Attribute {

		#region Fields

		bool recommendedAsConfigurable;

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

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException (); 
		}

		public override bool IsDefaultAttribute ()
		{
			return (!recommendedAsConfigurable);
		}
		#endregion // Methods
	}
}
