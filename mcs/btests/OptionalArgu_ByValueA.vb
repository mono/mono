'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.0: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified.
'=============================================================================================

Imports System
Module OP1_0_0
	Sub F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080)
		if (code <> 080)
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
		end if
   	End Sub 
   
   Sub Main()
      Dim telephoneNo As Long = 9886066432
      F(telephoneNo)
   End Sub 

End Module