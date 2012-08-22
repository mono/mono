using System.Collections.Generic;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Inference.Interface;

namespace Mono.CodeContracts.Static.Proving
{	
	abstract class ProofObligation
	{
		#region Fields
		
		private static uint nextID;
		public readonly uint ID;
		private bool hasCodeFix;
		public readonly object Provenance;
		public readonly APC PC;
		public List<ICodeFix> codeFixes;
		
		#endregion
		
		#region Construsctors
		
		static ProofObligation ()
		{
		}
		
		public ProofObligation (APC pc, object provenance)
		{
			this.PC = pc;
			this.codeFixes = new List<ICodeFix> ();
			this.hasCodeFix = false;
			this.ID = ProofObligation.nextID++;
			this.Provenance = provenance;
		}
		
		#endregion
		
		#region Properties
		
		public static int ProofObligationsWithCodeFix { get; private set; }
		
		public virtual bool IsEmpty
	    {
	      get
	      {
	        return false;
	      }
	    }
		
		public virtual BoxedExpression ConditionForPreconditionInference 
		{
			get 
			{
				return this.Condition;
			}
		}

		public virtual APC PCForValidation 
		{
			get 
			{
				return this.PC;
			}
		}
		
		public bool HasCodeFix 
		{
			get 
			{
				return this.CodeFixCount != 0;
			}
		}
		
		public int CodeFixCount 
		{
			get 
			{
				return this.codeFixes.Count;
			}
		}
		
		#endregion
		
		#region Methods
		
		private void ObjectInvariant ()
		{
		}
		
		#endregion
	}
}

