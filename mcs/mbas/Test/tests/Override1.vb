'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Unhandled Exception: System.MissingMemberException: Public member 'i' on type 'C2' not found.

Imports System

Class C1        
        Overridable Function fun(j as Integer)
	  End Function
End Class

Class C2
	Inherits C1  
	Overrides Function fun(j as Integer)	  
			i=j
			return i
	 End Function
	 Dim i as Integer
End Class

Module InheritanceM
        Sub Main()            
		Dim a as Object = new C2()
		Try 
			a.fun(a.i)	
		Catch e as Exception 
				System.Console.WriteLine(e.Message)
		End Try
        End Sub
End Module
