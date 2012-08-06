namespace Mono.CodeContracts.Static.Analysis.Numerical {
    struct Counter<T> {
        public readonly T Env;
        public readonly int Count;

        public Counter (T env)
            : this(env, 0)
        {
        }

        private Counter(T env, int count)
        {
            this.Env = env;
            this.Count = count;
        }

        public Counter<T> Incremented()
        {
            return new Counter<T> (Env, Count + 1);
        } 
    }
}