using System;

namespace Mono.CodeContracts.Static.Proving
{
	class MinimalProofObligation : ProofObligation
	{
		#region Fields
		
		private readonly BoxedExpression condition;
    	private readonly string obligationName;
		
		#endregion
		
		#region Constructor
		
		public MinimalProofObligation (APC pc, BoxedExpression condition, string obligationName, object provenance) : base(pc, provenance)
		{
			this.condition = condition;
      		this.obligationName = obligationName;
		}
		
		#endregion
		
		#region Properties
		
		public override BoxedExpression Condition
	    {
	      get
	      {
	        return this.condition;
	      }
	    }

	    public override string ObligationName
	    {
	      get
	      {
	        return this.obligationName;
	      }
	    }
		
		#endregion
	}
}

