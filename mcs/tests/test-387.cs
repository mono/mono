public struct ObjectID {
	long l;

	public ObjectID (long l)
	{
		this.l = l;
	}

	public static implicit operator long (ObjectID p)
	{
		return p.l;
	}

	public static implicit operator ObjectID (long l)
	{
		return new ObjectID (l);
	}

	public static void Main ()
	{
		ObjectID x = new ObjectID (0);
		decimal y = x;
	}
}

