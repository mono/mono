// CS0657: `field' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event, method'. All attributes in this section will be ignored
// Line: 8
// Compiler options: -warnaserror

using System;

interface X {
  [field:NonSerialized]
  event EventHandler XEvent;
}
