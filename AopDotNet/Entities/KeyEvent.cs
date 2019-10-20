namespace AopDotNet.Entities {

    public enum BiologicalOrganisation {
        Unknown = -1,
        Molecular = 0,
        Cellular = 1,
        Organelle = 2,
        Individual = 3,
        Population = 4
    };

    public class KeyEvent {

        private string _name;

        public string Id { get; set; }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Id;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }

        public BiologicalOrganisation BiologicalOrganisation { get; set; }

        public KeyEvent() { }

        public KeyEvent(string id, string name, BiologicalOrganisation biologicalOrganisation) {
            Id = id;
            Name = name;
            BiologicalOrganisation = biologicalOrganisation;
        }

        public override string ToString() {
            return Id;
        }
    }
}
