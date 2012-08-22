using System;

namespace Mono.CodeContracts.Static.Analysis
{
	abstract class PathElement
	{
		#region Path Element Properties
		
		public virtual bool IsBool
		{
			get{return false;}
		}
		
		public virtual string IsModel
		{
			get{return false;}
		}
		
		public virtual bool IsAdressOf
		{
			get;set;
		}
		
		public virtual string CastTo
		{
			get{return "";}
		}
		
		public virtual string IsGettingUnit
		{
			get{return false;}
		}
		
		public virtual string IsParam
		{
			get{return false;}
		}
		
		public virtual string IsParamRef
		{
			get{return false;}
		}
		
		public virtual string IsDeref
		{
			get{return false;}
		}
		
		public virtual string GotDeref
		{
			get{return false;}
		}
		
		public virtual string IsMethodCaller
		{
			get{return false;}
		}
		
		public virtual string IsManagedPointer
		{
			get{return false;}
		}
		
		public virtual string IsUnmanagedPointer
		{
			get{return false;}
		}
		
		public virtual string IsStatic
		{
			get{return false;}
		}
		
		#endregion
		
		#region Path Element Methods
		
		public abstract PathElement CahngeLocation(object source,object destination);
		
		public virtual bool CheckField<Field>(out Field f)
	    {
	      f = default (Field);
	      return false;
	    }

    	public abstract bool CheckResults<Type>(out Type type);

    	public abstract override string ToString();
		
		#endregion 
	}
}

