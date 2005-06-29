// cs1667-5.cs: Attribute `System.CLSCompliantAttribute' is not valid on property or event accessors. It is valid on `assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, parameter, delegate, return' declarations only
// Line: 7

using System;

public interface X {
  [method:CLSCompliant (false)]
  event EventHandler XEvent;
}
