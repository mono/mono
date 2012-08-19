namespace Mono.CodeContracts.Static.Analysis.Numerical {
        struct Counter<T> {
                public readonly int Count;
                public readonly T Env;

                public Counter (T env)
                        : this (env, 0)
                {
                }

                Counter (T env, int count)
                {
                        Env = env;
                        Count = count;
                }

                public Counter<T> Incremented ()
                {
                        return new Counter<T> (Env, Count + 1);
                }

                public override string ToString ()
                {
                        return string.Format ("{0} ({1})", Env, Count);
                }
        }
}