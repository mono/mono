// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("NAnt")]
[assembly: AssemblyDescription("A .NET Build Tool")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("http://nant.sourceforge.net/")]
[assembly: AssemblyProduct("NAnt")]
[assembly: AssemblyCopyright("Copyright (C) 2001 Gerry Shaw")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("0.6.0.*")]

// This will not compile with Visual Studio.  If you want to build a signed
// executable use the NAnt build file.  To build under Visual Studio just
// exclude this file from the build.
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
