'Line 30, BC30580: ExternalSource Directives may not be nested

Imports System
Module ExternalDirectives
	Sub Main()
		#ExternalSource("/home/test.aspx",30)
            	#ExternalSource("/home/test.aspx",30)
		Console.WriteLine("In test.aspx")
		#End ExternalSource
		#End ExternalSource
	End Sub
End Module
