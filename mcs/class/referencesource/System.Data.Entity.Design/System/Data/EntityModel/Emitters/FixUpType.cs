//---------------------------------------------------------------------
// <copyright file="FixUpType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner srimand
//---------------------------------------------------------------------

namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Types of fix ups the client code generator can perform on the generated code
    /// </summary>
    // these values are used as indexes in the m_fixMethods list.
    internal enum FixUpType
    {
        /// <summary>Mark an override method as final (sealed)</summary>
        MarkOverrideMethodAsSealed = 1,
        /// <summary>Mark a property setter as internal</summary>
        MarkPropertySetAsInternal = 2,
        /// <summary>Mark a class as static</summary>
        MarkClassAsStatic = 3,
        /// <summary>Mark a property getter as private</summary>
        MarkPropertyGetAsPrivate = 4,
        /// <summary>Mark a property getter as internal</summary>
        MarkPropertyGetAsInternal = 5,
        /// <summary>Mark a property getter as public</summary>
        MarkPropertyGetAsPublic = 6,
        /// <summary>Mark a property setter as private</summary>
        MarkPropertySetAsPrivate = 7,
        /// <summary>Mark a property setter as public</summary>
        MarkPropertySetAsPublic = 8,
        /// <summary>Mark a method as partial</summary>
        MarkAbstractMethodAsPartial = 9,
        /// <summary>Mark a property getter as protected</summary>
        MarkPropertyGetAsProtected = 10,
        /// <summary>Mark a property setter as protected</summary>
        MarkPropertySetAsProtected = 11,
    }
}
