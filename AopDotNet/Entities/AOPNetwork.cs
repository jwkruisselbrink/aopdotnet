using System.Collections.Generic;
using System.Xml.Serialization;

namespace AopDotNet.Entities {
    public class AopNetwork {

        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<KeyEvent> KeyEvents { get; set; }

        public ICollection<KeyEventRelationship> KeyEventRelationships { get; set; }

        public AopNetwork() { }

        public AopNetwork(
            string name,
            ICollection<KeyEvent> keyEvents,
            ICollection<KeyEventRelationship> keyEventRelationships
        ) {
            Name = name;
            KeyEvents = keyEvents;
            KeyEventRelationships = keyEventRelationships;
        }
    }
}
