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
