'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Checking For unqualifed name getting accessed

Imports A

Namespace A
	Public Module B
		Function C()	
			return 10
		End Function
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		If C()<>10 Then
			Throw New System.Exception("Not Working")
		End If
	End Sub
End Module

