///----------- ----------- ----------- ----------- ----------- -----------
/// <copyright file="ICustomTypeProvider.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
/// <owner>gpaperin</owner>
///----------- ----------- ----------- ----------- ----------- -----------

#if !SILVERLIGHT

using System;

namespace System.Reflection {

public interface ICustomTypeProvider {

    Type GetCustomType();
}

}  // namespace System.Reflection

#endif  // !SILVERLIGHT

// ICustomTypeProvider.cs
