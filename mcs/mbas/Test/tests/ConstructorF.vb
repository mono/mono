'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class A
	Public i as Integer = 10
	Sub New()
	End Sub	
End Class

Class AB
	Inherits A
	Public i1 as Integer = MyBase.i
	Sub New()
		if i1<>10 then
			Throw new System.Exception("Constructor not working properly")
		End if
	End Sub	
End Class

Module Test
    Public Sub Main()
      Dim a as AB= New AB()	
    End Sub
End Module

