#include "demo.h"

int main () {
  DemoTest *my_test;
  
  //run a static method
  demo_test_static_method ();

  //create an object instance
  my_test = demo_test_new ();
  
  //run an instance method
  demo_test_increment (my_test);

  //run an instance method with arguments
  demo_test_add_number (my_test, 2);

  //run an instance method with arguments
  demo_test_set_title (my_test, "hello from c");
  
  //TODO: return value
  //g_printf ("returned string: %s\n", demo_test_get_title (my_test));

  //TODO: gobject-style DEMO_IS_TEST etc. macros

  return 1;
}
