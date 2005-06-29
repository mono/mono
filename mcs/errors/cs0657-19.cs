// cs0657-19.cs: `field' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event, method'
// Line : 7

using System;

interface X {
  [field:NonSerialized]
  event EventHandler XEvent;
}
