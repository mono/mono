//
// Requires -unsafe to build
//
using System;

public class Driver {
  public static void Main () {
    float [] floats = new float[1];
    floats[0] = 1.0f;
    unsafe {
      fixed (float *fp = &floats[0]) {
	Console.WriteLine ("foo" + fp);
      }
    }
  }
}
