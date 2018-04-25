// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {
    
    using System;
    // The IComparable interface is implemented by classes that support an
    // ordering of instances of the class. The ordering represented by
    // IComparable can be used to sort arrays and collections of objects
    // that implement the interface.
    // 
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IComparable
    {
    // Interface does not need to be marked with the serializable attribute
        // Compares this object to another object, returning an integer that
        // indicates the relationship. An implementation of this method must return
        // a value less than zero if this is less than object, zero
        // if this is equal to object, or a value greater than zero
        // if this is greater than object.
        // 
        int CompareTo(Object obj);
    }

    // Generic version of IComparable.

    public interface IComparable<in T>
    {
        // Interface does not need to be marked with the serializable attribute
        // Compares this object to another object, returning an integer that
        // indicates the relationship. An implementation of this method must return
        // a value less than zero if this is less than object, zero
        // if this is equal to object, or a value greater than zero
        // if this is greater than object.
        // 
        int CompareTo(T other);
    }
}
