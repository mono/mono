using System;

class Source {
	public event EventHandler ChildSourceAdded;
	public event EventHandler ChildSourceRemoved;

        Source FindSource (Source x){ return null; }

        private void AddSource(Source source, int position, object parent)
        {
            if(!FindSource(source).Equals(source)) {
                return;
            }

            object iter = null;

            source.ChildSourceAdded += delegate(object t, EventArgs e) {
                AddSource((Source)(object)e, position, iter);
            };

            source.ChildSourceRemoved += delegate(object t, EventArgs e) {
            };

        }
        public static void Main () {}

}
