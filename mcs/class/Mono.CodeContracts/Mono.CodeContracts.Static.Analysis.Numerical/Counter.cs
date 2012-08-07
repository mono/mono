namespace Mono.CodeContracts.Static.Analysis.Numerical {
        internal struct Counter<T> {
                public readonly int Count;
                public readonly T Env;

                public Counter (T env)
                        : this (env, 0)
                {
                }

                private Counter (T env, int count)
                {
                        this.Env = env;
                        this.Count = count;
                }

                public Counter<T> Incremented ()
                {
                        return new Counter<T> (this.Env, this.Count + 1);
                }

                public override string ToString ()
                {
                        return string.Format ("{0} ({1})", Env, Count);
                }
        }
}