'BC30578: '#End ExternalSource' directive must be preceded by a  matching '#ExternalSource' directive

Imports System
Module ExternalSourceDirective
	Sub Main()
			Console.WriteLine(“In test.aspx”)
			#End ExternalSource
	End Sub
End Module
