REM LineNo: 24
REM ExpectedError: BC30202
REM ErrorMessage: 'Optional' expected.

REM LineNo: 25
REM ExpectedError: BC30815
REM ErrorMessage: 'name' is not declared. File I/O functionality is available in the 'Microsoft.VisualBasic' namespace.

REM LineNo: 36
REM ExpectedError: BC30057
REM ErrorMessage: Too many arguments to 'Public Function F(telephoneNo As Long, [code As Integer = 80], [code1 As Integer = 91]) As Boolean'.

'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.2: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified. After Optional parameter we can't use general 
'		parameter as ByVal or ByRef
'=============================================================================================

Imports System
Module OP1_0_2
	Function F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, ByRef name As String) As Boolean
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
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_2")
	end if
   End Sub 

End Module