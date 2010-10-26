using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class PropagationStack : IEnumerable<PropagationReason>
	{
		private List<MethodDefinition> stack = new List<MethodDefinition>();
		private Dictionary<MethodDefinition, PropagationReason> reasons = new Dictionary<MethodDefinition, PropagationReason>();

		public static PropagationStack Create(Dictionary<MethodDefinition, List<PropagationReason>> propagationGraph, MethodDefinition method)
		{
			var ps = new PropagationStack();
			while(true)
			{	
				ps.stack.Add(method);
				if (ps.stack.Count>100)
					Debugger.Break();
				var r = propagationGraph[method].First();
				ps.reasons[method] = r;
				if (r.MethodThatTaintedMe==null)
					return ps;
				method = r.MethodThatTaintedMe;
			}
		}

		public IEnumerator<PropagationReason> GetEnumerator()
		{
			return stack.Select(m => reasons[m]).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}