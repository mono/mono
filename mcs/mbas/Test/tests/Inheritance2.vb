'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Unhandled Exception: System.MissingMemberException: Public member 'fun' on type 'C1' not found.  

Imports System

Class C1
   	  Private Sub fun()
	  End Sub
End Class

Class C2
        Inherits C1
	  Public Sub G()
        End Sub
End Class

Module Mismatch
        Sub Main()
		Dim a As Object
		Try 
			a=New C1()
			a.fun()
			a=New C2()
			a.fun()
		      Catch e As Exception
		         Console.WriteLine(e.Message)
		End Try
	
        End Sub
End Module
