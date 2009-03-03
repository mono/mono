// Compiler options: -warnaserror

using System;
using System.Reflection;
using System.ComponentModel;

[assembly: CLSCompliant(true)]
[assembly: AssemblyTitle("")]

public class CLSCLass_6 {
    private object disposedEvent = new object ();
    public EventHandlerList event_handlers;    
        
    public event Delegate Disposed {
        add { event_handlers.AddHandler (disposedEvent, value); }
	remove { event_handlers.RemoveHandler (disposedEvent, value); }
    }
}

public delegate CLSDelegate Delegate ();
    
[Serializable]
public class CLSDelegate {
}

#pragma warning disable 3019
internal class CLSClass_5 {
        [CLSCompliant (true)]
        public uint Test () {
                return 1;
        }
}
#pragma warning restore 3019

[CLSCompliant (true)]
public class CLSClass_4 {
        [CLSCompliant (false)]
        public uint Test () {
                return 1;
        }
}

public class CLSClass_3 {
        [CLSCompliant (false)]
        public uint Test_3 () {
                return 6;
        }
}

[CLSCompliant(false)]
public class CLSClass_2 {
        public sbyte XX {
            get { return -1; }
        }
}

class CLSClass_1 {
        public UInt32 Valid() {
                return 5;
        }
}
    
[CLSCompliant(true)]
public class CLSClass {
    
        private class C1 {
#pragma warning disable 3019            
            [CLSCompliant(true)]
            public class C11 {
                protected ulong Foo3() {
                    return 1;
                }
            }
#pragma warning restore 3019
            protected long Foo2() {
                return 1;
            }
        }

	[CLSCompliant(false)]
	protected internal class CLSClass_2 {
        	public sbyte XX {
	            get { return -1; }
        	}
	}

#pragma warning disable 3019, 169
		[CLSCompliant(true)]
        private ulong Valid() {
                return 1;
        }
#pragma warning restore 3019, 169
        
        [CLSCompliant(true)]
        public byte XX {
            get { return 5; }
        }

//        protected internal sbyte FooProtectedInternal() {
//                return -4;
//       }
        
        internal UInt32 FooInternal() {
                return 1;
        }        

#pragma warning disable 169
        private ulong Foo() {
                return 1;
        }
#pragma warning restore 169

        
        public static void Main() {}
}
