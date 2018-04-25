// CS1922: A field or property `GCEventTypeMatcher' cannot be initialized with a collection object initializer because type `GCEventTypeMatcher' does not implement `System.Collections.IEnumerable' interface
// Line: 11

using System;

public enum GCEventType {
	NURSERY_START
}

public class GCEventTypeMatcher {
	private static GCEventTypeMatcher[] matcher = { new GCEventTypeMatcher () { NURSERY_START, s => TRUE } };
}
