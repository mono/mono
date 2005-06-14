using System;
using System.Reflection;

[assembly: CLSCompliant (true)]

public class CLSClass {
        [CLSCompliant(false)]
        public CLSClass(ulong l) {}
        internal CLSClass(uint i) {}
            
        [CLSCompliant(false)]
        public ulong X_0 {
            set {}
            }
            
        [CLSCompliant(false)]
        protected ulong this[ulong i] {
            set {}
        }
        
        [CLSCompliant(false)]
        public ulong X_1;
            
        internal ulong X_2;

        public static void Main() {
	}
}
public class InnerTypeClasss {
    public struct Struct {
    }
    
    public Struct Method () {
        return new Struct ();
    }
}