'Line 11, BC30578: '#End ExternalSource' directive must be preceded by a  matching '#ExternalSource' directive

Imports System
Module ExternalSourceDirective
	Sub Main()
			#ExternalSource("/home/a.aspx", 1024)
			Console.WriteLine("In a.aspx")
			#End ExternalSource

			Console.WriteLine(“In test.aspx”)
			#End ExternalSource
	End Sub
End Module
