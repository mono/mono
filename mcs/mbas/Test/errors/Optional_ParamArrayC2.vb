REM LineNo: 31
REM ExpectedError: BC30046
REM ErrorMessage: Method cannot have both a ParamArray and Optional parameters.

REM LineNo: 32
REM ExpectedError: BC30451
REM ErrorMessage: Name 'args' is not declared.

REM LineNo: 33
REM ExpectedError: BC30451
REM ErrorMessage: Name 'args' is not declared.

REM LineNo: 39
REM ExpectedError: BC30311
REM ErrorMessage: Value of type '1-dimensional array of Integer' cannot be converted to 'Integer'.

REM LineNo: 40
REM ExpectedError: BC30057
REM ErrorMessage: Too many arguments to 'Public Sub F([length As Integer = 3])'.

'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Optional argument:
'APR-1.0.0: ParamArray and Optional can be used together
'=============================================================================================

Option Strict Off
Imports System
Module PA_1_0_0
   Sub F(Optional ByVal length as Integer = 3 , ParamArray args() As Integer )
      Console.Write("Array contains " & args.Length & " elements:")
      if (args.Length <> length)
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
	end if 
   End Sub
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
      F(a)
      F(10, 20, 30)
      
   End Sub
End Module
'=============================================================================================
