// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Library General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

// project created on 3/13/04 at 5:22 a
using System;
using System.Collections;
using Mono.GetOptions;

namespace Mfconsulting.General.Prj2Make.Cui
{
	class MainOpts : Options 
	{
		[Option("Output for nmake.exe", 'n')]
		public bool isNmake = false;

		[Option("Use csc instead of mcs", 'c')]
		public bool isCsc = false;
		
		[Option(1, "Converts a csproj/sln to prjx/cmbx", "csproj2prjx")]
		public bool csproj2prjx = false;
		
		public MainOpts()
		{
			ParsingMode = OptionsParsingMode.Both;
			BreakSingleDashManyLettersIntoManyOptions = false;
			EndOptionProcessingWithDoubleDash = true;

			if (System.IO.Path.DirectorySeparatorChar.CompareTo('/') == 0)
				ParsingMode = OptionsParsingMode.Linux;
		}
	}    
}
