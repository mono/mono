#include "demo.h"

int main () {
  DemoTest *my_test;
  
  //run a static method
  demo_test_static_method ();

  //create an object instance
  my_test = demo_test_new ();
  
  //run an instance method
  demo_test_method1 (my_test);

  //run an instance method with an unusual name
  demo_test_gtype_gtype_gtype (my_test);

  //TODO: run an instance method with arguments
  //demo_test_method2 (my_test, "hey");
}
