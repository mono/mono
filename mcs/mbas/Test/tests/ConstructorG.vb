'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To check if initializing takes place according to the occurance

Imports System

Class A
	Public i as Integer = j
	Public j as Integer = 1
	Sub New()		
		if i<>0
			Throw new System.Exception("Constructor not working properly")			
		End if
	End Sub
End Class

Class AB
	Inherits A
	Public k as Integer = i
	Sub New()		
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as AB = New AB()
    End Sub
End Module

