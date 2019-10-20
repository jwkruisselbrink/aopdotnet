namespace AopDotNet.Entities {
    public class KeyEventRelationship {
        public string Name { get; set; }
        public KeyEvent FromNode { get; set; }
        public KeyEvent ToNode { get; set; }

        public KeyEventRelationship() { }

        public KeyEventRelationship(string name) {
            Name = name;
        }

        public KeyEventRelationship(string name, KeyEvent fromNode, KeyEvent toNode) {
            Name = name;
            FromNode = fromNode;
            ToNode = toNode;
        }

        public override string ToString() {
            return Name;
        }
    }
}
