'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<Assembly: AssemblyCulture("")>

Module Test
Sub Main()
 	
	dim asm as System.Reflection.AssemblyName
	asm = System.Reflection.Assembly.GetCallingAssembly ().GetName ()
	if asm.cultureinfo.Tostring() <> ""
		Throw New Exception ("Expected to be null")
	End if
End Sub
End Module

