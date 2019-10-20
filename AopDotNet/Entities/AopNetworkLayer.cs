using System.Collections.Generic;
using System.Linq;

namespace AopDotNet.Entities {
    public class AopNetworkLayer {

        public ICollection<KeyEvent> KeyEvents { get; set; }

        public AopNetworkLayer() { }

        public AopNetworkLayer(ICollection<KeyEvent> nodes) {
            KeyEvents = nodes;
        }

        public BiologicalOrganisation BiologicalOrganisation() {
            return KeyEvents.Select(r => r.BiologicalOrganisation).Distinct().Single();
        }

        public override string ToString() {
            return string.Join(", ", KeyEvents.Select(r => r.Id));
        }
    }
}
