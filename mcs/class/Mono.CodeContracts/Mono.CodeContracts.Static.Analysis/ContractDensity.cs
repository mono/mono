using System;

namespace Mono.CodeContrats.ContextStaticAttribute.Analysis
{
	public class ContractDensity
	{
		private ulong contracts;
    	private ulong methodInstructions;
    	private ulong contractInstructions;

		public float Densitys
	    {
	      get
	      {
	        if ((long) this.methodInstructions == 0L)
	          return 0.0f;
	        else
	          return (float) this.contractInstructions / (float) this.methodInstructions;
	      }
	    }

		public ulong Contracts
	    {
	      get
	      {
	        return this.contracts;
	      }
	    }

		public ulong MethodInstructions
	    {
	      get
	      {
	        return this.methodInstructions;
	      }
	    }

	    public ulong ContractInstructions
	    {
	      get
	      {
	        return this.contractInstructions;
	      }
	    }

	    public ContractDensity(ulong methodInstructions, ulong contractInstructions, ulong contracts)
	    {
	      this.contracts = contracts;
	      this.methodInstructions = methodInstructions;
	      this.contractInstructions = contractInstructions;
	    }

	    public void Add(ContractDensity other)
	    {
	      this.methodInstructions += other.methodInstructions;
	      this.contractInstructions += other.contractInstructions;
	      this.contracts += other.contracts;
	    }
	}
}

