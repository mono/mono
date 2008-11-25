// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;

namespace NUnitExtension.RowTest
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class RowTestAttribute : Attribute
	{
	}
}
