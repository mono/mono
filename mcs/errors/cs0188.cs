// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 6

struct Sample {
        public Sample(string text) {
            Initialize();
            this.text = text;
        }

        void Initialize() {
        }
        
        string text;
}