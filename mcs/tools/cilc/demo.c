#include <glib.h>
#include <glib/gprintf.h>
#include "demo.h"

int main () {
  DemoTest *my_test;
	//gchar *tmp;
	int num;
	DemoDrink drink;
	//GEnumClass *enum_class;
  
  //run a static method
  demo_test_static_method ();

  //create an object instance
  my_test = demo_test_new ();
  
  //run an instance method
  demo_test_increment (my_test);

  //run an instance method with arguments
  demo_test_add_number (my_test, 2);

  //run an instance method with arguments
  demo_test_echo (my_test, "hello from c");

  //run a property set accessor
  demo_test_set_title (my_test, "set property from c");
  
	//run a property set accessor
  //tmp = demo_test_get_title (my_test);
	//g_print (tmp);
  num = demo_test_get_value (my_test);
	g_printf ("The counter's value is %d\n", num);
  
	drink = demo_test_pick_drink ();
	//enum_class = g_type_class_peek (demo_drink_get_type ());
	//g_enum_get_value (enum_class, drink);
	//g_printf ("%d\n", drink);
  
  //TODO: return value
  //g_printf ("returned string: %s\n", demo_test_get_title (my_test));

  return 0;
}
