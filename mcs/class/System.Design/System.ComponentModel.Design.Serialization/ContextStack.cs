// System.ComponentModel.Design.Serialization.ContextStack.cs
//
// Author:
//      Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
        public sealed class ContextStack
        {
		public ArrayList list;
		
                public ContextStack () {
                        list = new ArrayList ();
                }

                public object Current {
                        get { 
                                if (list.Count == 0) return null;
                                return list [list.Count - 1];
                        }

			set { 
 				list.Add (value);
                        }
                }

                [MonoTODO]
		public object this[Type type] {
                        get { throw new NotImplementedException ();}
                        set { throw new NotImplementedException ();}
                }

                [MonoTODO]
                public object this[int level] {
                        get { throw new NotImplementedException ();}
                        set { throw new NotImplementedException ();}
                }
	}
}
