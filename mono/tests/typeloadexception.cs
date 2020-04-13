using System;
using System.Runtime.InteropServices;

public struct Wrapper { public Object o; }
	
[ StructLayout( LayoutKind.Explicit )] public struct MyUnion {
  [ FieldOffset( 0 )] public int i;
  [ FieldOffset( 0 )] public Wrapper o;
}

public class TestTypeLoadException{

  public static int Run(){
      bool caught=false;
      try{
          Go();
      }
      catch(TypeLoadException e){
          caught=true;
      }
      if(caught){
          return 0;
      }
      else{
          return 1;
      }
  }
  public static void Go(){
    MyUnion u;
    u.i = 1;
    u.o.o = null;
    u.o.o = new object();
    u.i = 1000;
  }
}


public class Test{
  public static int test_0_typeloadexception(){
      return TestTypeLoadException.Run();
  }

  public static int Main (string[] args) {
		return TestDriver.RunTests (typeof (Test), args);
}
}
