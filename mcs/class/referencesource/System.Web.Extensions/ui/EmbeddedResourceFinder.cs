//------------------------------------------------------------------------------
// <copyright file="EmbeddedResourceFinder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
// This internal only class exists to work around a bug in
// ToolboxBitmap.GetImageFromResource method which doesn't 
// work properly is the assembly name is different from 
// the namespace. The work around is to use a type which is 
// outside the root namespace.
internal class EmbeddedResourceFinder { }