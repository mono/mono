'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Checking For Alias

Imports A = Thisisaverylongname

Namespace Thisisaverylongname
	Module B
		Function C()	
			return 10
		End Function
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		If A.B.C()<>10 Then
			Throw New System.Exception("Alias Not Working")
		End If
	End Sub
End Module

