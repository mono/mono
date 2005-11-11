// gcs0401.cs: The new() constraint must be last
// Line: 6

class Foo<T>
	where T : new (), new ()
{
}
