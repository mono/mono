'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.1: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified.
'=============================================================================================

Imports System
Module OP1_0_1
	Function F(ByVal telephoneNo as Long, Optional ByRef code as Integer = 080, Optional ByVal code1  As Integer = 091)
		if (code = 080 and code1 = 091)
			return false
		else 
			return true

		end if
   	End Function 
   
   Sub Main()
      Dim telephoneNo As Long = 9886066432
	 Dim code As Integer = 081
	Dim code1 As Integer = 091
	Dim status as Boolean
      status = F(telephoneNo,code,code1)
	if(status = false)
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
	end if
   End Sub 

End Module