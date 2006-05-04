
Program.exe: Program.cs
	gmcs -out:$@ $< -r:ClassLib.dll -r:System.Configuration.dll -r:System.Web.dll

ClassLib.dll:: ClassLib.cs
	gmcs -target:library -out:$@ $< -r:System.Configuration.dll -r:System.Web.dll

Program.exe: ClassLib.dll
