REM LineNo: 15
REM ExpectedError: BC30642
REM ErrorMessage: 'Optional' and 'ParamArray' cannot be combined.

'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: ParamArray:
'APR-1.0.0: ParamArray can be used only on the last argument of argument list. it allows us to 'pass an arbitrary list. It allows us to pass an arbitrary number of argument to the procedure 
'=============================================================================================

Option Strict Off
Imports System
Module PA_1_0_0
   Sub F(Optional ParamArray args() As Integer = {0,0,0})
      Console.Write("Array contains " & args.Length & " elements:")
      if (args.Length <> 3)
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
