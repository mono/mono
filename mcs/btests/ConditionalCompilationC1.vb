Imports System
Module ConditionalCompilation
	Sub Main()
		'Syntactically worng statement within #If block that satisfies the condition
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
