namespace Mono.CodeContracts.Static.Analysis.Numerical {
    struct Counter<TEnv> {
        public readonly TEnv Env;
        public readonly int Count;

        public Counter (TEnv env)
            : this(env, 0)
        {
        }

        private Counter(TEnv env, int count)
        {
            this.Env = env;
            this.Count = count;
        }

        public Counter<TEnv> Increment()
        {
            return new Counter<TEnv> (Env, Count + 1);
        } 
    }
}