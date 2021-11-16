namespace History {
    public class Fact<T> : IFact  {
        public string Name { get; }
        public T Value { get; set; }

        public Fact(string name) {
            Name = name;
        }
    }
}
