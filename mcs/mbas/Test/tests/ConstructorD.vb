'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To Prove Constructors can be overloaded

Imports System

Class A
	Sub New()		
	End Sub
	Sub New(I as Integer)		
	End Sub
	Sub New(I as Integer, J as Integer)		
	End Sub
	Shared Sub New()
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as A = New A(10)
    End Sub
End Module

