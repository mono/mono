Class C1
        'property that do not override any other
        'property should not be declared 'notoverridable'

        Public NotOverridable Property p() as Integer
		GET
		END GET

		SET (val as Integer)
		End SET
	End Property

End Class
Module OverrideC2
        Sub Main()
        End Sub
End Module
