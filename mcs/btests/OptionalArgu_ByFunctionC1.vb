REM LineNo: 16
REM ExpectedError: BC30812
REM ErrorMessage: Optional parameters must specify a default value.

'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.1: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified.If we don't assign the constant value then it
'		 produce compilation error
'=============================================================================================

Imports System
Module OP1_0_0
	Function F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, Optional ByRef name As String) As Boolean
		if (code <> 080 and code1 <> 091 and name="")
			return false
		else 
			return true
		end if
   	End Function 
   
   Sub Main()
      Dim telephoneNo As Long = 9886066432
	Dim name As String ="Manish"
	Dim status as Boolean
      status =F(telephoneNo,,,name)
	if(status = false)
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
	end if
   End Sub 

End Module