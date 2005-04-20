'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: AssemblyCopyright ("2004, 2005 Novell, Inc.")>

Module Test
Sub Main()
 	
	dim asm as System.Reflection.Assembly
	dim i as integer
	asm = System.Reflection.Assembly.GetAssembly (GetType (Test))
	dim att as object () = asm.GetCustomAttributes (false)
	for i=0 to att.Length - 1
	If att (i).ToString () <> "System.Reflection.AssemblyCopyrightAttribute" Then 
		Throw New Exception ("AssemblyCopyright Attribute was not set properly")
	End If
	next i
End Sub
End Module
