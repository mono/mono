interface a <t> { void x (); }

interface b <t> : a <t> {}

class kv <k,v> {} // type t

interface c <k,v>: b <kv<k,v>>,  // b <t>
                   a <kv<k,v>>    // a <t>
{}

class m <k,v> : c <k,v>,
                b <kv<k,v>> // b <t>
{
        void a <kv <k,v>>.x () {} // a<t>.x ()
}

class X
{
	public static void Main ()
	{ }
}
