REM LineNo: 20
REM ExpectedError: BC30202
REM ErrorMessage: 'Optional' expected.

REM LineNo: 21
REM ExpectedError: BC30815
REM ErrorMessage: 'name' is not declared. File I/O functionality is available in the 'Microsoft.VisualBasic' namespace.

'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.0: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified. After Optional parameter we can't use general 
'		parameter as ByVal or ByRef
'=============================================================================================

Imports System
Module OP1_0_0
	Sub F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, ByRef name As String)
		if (code <> 080 and code1 <> 091 and name="")
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
		end if
   	End Sub 
   
   Sub Main()
      Dim telephoneNo As Long = 9886066432
	Dim name As String ="Manish"
      F(telephoneNo,,name)
   End Sub 

End Module