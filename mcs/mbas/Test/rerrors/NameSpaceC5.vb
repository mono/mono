'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 21
REM ExpectedError: BC30560
REM ErrorMessage: 'Console' is ambiguous in the namespace 'System'

'Thus it can be understood that Shadowing is not possible... even for existing namespaces...

Namespace System
	Public Module Console
		Public Sub WriteLine(s as String)
		End Sub
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		System.Console.WriteLine("HelloWorld")
	End Sub
End Module

