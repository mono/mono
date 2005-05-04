using C = System.Console;

class Test  {
    
    public static void Main (string[] args) {
      switch (1) {
        default:
          switch (2) {
            default:
              int flag = 1;       //makes the next if computable -- essential!
	      if (flag == 1) {
		C.WriteLine("**** This one is expected");
                break;  //break-2
	      }  
              else  goto lbl;
          }
	  break;  //break-1  This point is REACHABLE through break-2, 
	          // contrary to the warning from compiler!
        
        lbl:
          C.WriteLine("**** THIS SHOULD NOT APPEAR, since break-1 was supposed to fire ***");
          break;
        
      }
    }
}
