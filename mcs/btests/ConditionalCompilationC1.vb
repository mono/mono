REM LineNo: 17
REM ExpectedError: BC30648
REM ErrorMessage: String constants must end with a double quote.

REM LineNo: 22
REM ExpectedError: BC30037
REM ErrorMessage: Character is not valid.

'Line 9,  BC30648: String constants must end with a double quote.
'Line 14, BC30037: Character is not valid.

Imports System
Module ConditionalCompilation
	Sub Main()
		'Syntactically wrong statement within #If block that satisfies the condition
		#If True
			Console.WriteLine("Hello)
		#End If

		'Lexically invalid statement within #If block that satisfies condition
		#If True
			@#$^**
		#End If
		
		'Lexically invalid statement within #If block that does not satisfies condition
		#If False
			DD@##$
		#End If
		
	End Sub
End Module
