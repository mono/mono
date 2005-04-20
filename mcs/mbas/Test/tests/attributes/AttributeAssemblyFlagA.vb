'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices


Module Test
Sub Main()
 	
	dim asm as System.Reflection.AssemblyName
	dim i as integer
	asm = System.Reflection.Assembly.GetCallingAssembly ().GetName ()
	if asm.Flags.Tostring() <> "PublicKey" then 
		Throw New Exception ("Expected Assembly Flag to be PublicKey")	
	End if	
End Sub
End Module
