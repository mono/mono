'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

MustInherit Class aa 
	  Delegate Function SD(i as Integer) AS Integer
        Mustoverride Function f(i as Integer) As Integer
End Class

Class ab
	  Inherits aa	
	  Delegate Function SD(i as Integer) AS Integer
        overrides Function f(i as Integer) As Integer
	  End function
End Class

Module M
        Sub Main()
		    dim a as ab = new ab()	
        End Sub
End Module
