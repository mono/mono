// cs1667.cs: Attribute 'CLSCompliant' is not valid on property or event accessors. It is valid on 'assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, param, delegate, return, type parameter' declarations only.
// Line: 7

using System;

public interface X {
  [method:CLSCompliant (false)]
  event EventHandler XEvent;
}
