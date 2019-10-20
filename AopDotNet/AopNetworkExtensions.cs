using AopDotNet.Entities;
using System.Collections.Generic;
using System.Linq;

namespace AopDotNet {
    public static class AopNetworkExtensions {

        /// <summary>
        /// Gets sequential layers of the AOP network.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static IList<AopNetworkLayer> GetAopNetworkLayers(this AopNetwork aopNetwork) {
            var cyclicKers = aopNetwork.FindFeedbackRelationships();
            var kers = aopNetwork.KeyEventRelationships.Except(cyclicKers);
            var toNodesLookup = kers.ToLookup(r => r.ToNode, r => r.FromNode);
            var fromNodesLookup = kers.ToLookup(r => r.FromNode, r => r.ToNode);
            var rootNodes = aopNetwork.KeyEvents.Where(r => !toNodesLookup.Contains(r)).ToList();
            return GetAopNetworkLayersRecursive(rootNodes, toNodesLookup, fromNodesLookup, new HashSet<KeyEvent>());
        }

        /// <summary>
        /// Finds key-event relationships that feed-back to earlier key events, and therewith
        /// cause cycles.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static ICollection<KeyEventRelationship> FindFeedbackRelationships(this AopNetwork aopNetwork) {
            var toNodesLookup = aopNetwork.KeyEventRelationships.ToLookup(r => r.ToNode, r => r.FromNode);
            var fromNodesLookup = aopNetwork.KeyEventRelationships.ToLookup(r => r.FromNode);
            var rootNodes = aopNetwork.KeyEvents.Where(r => !toNodesLookup.Contains(r)).ToList();
            var result = aopNetwork.KeyEventRelationships
                .Where(r => rootNodes.Contains(r.FromNode))
                .SelectMany(r => FindFeedbackRelationshipsRecursive(r, fromNodesLookup, new HashSet<KeyEventRelationship>()))
                .Distinct()
                .ToList();
            return result;
        }

        /// <summary>
        /// Identifies key-event relationships that can also be reached indirectly (and may be
        /// non-adjecent).
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static ICollection<KeyEventRelationship> GetIndirectKeyEventRelationships(this AopNetwork aopNetwork) {
            var cyclicKers = aopNetwork.FindFeedbackRelationships();
            var kers = aopNetwork.KeyEventRelationships.Except(cyclicKers);
            var toNodesLookup = kers.ToLookup(r => r.ToNode, r => r.FromNode);
            var fromNodesLookup = kers.ToLookup(r => r.FromNode, r => r);
            var rootNodes = aopNetwork.KeyEvents.Where(r => !toNodesLookup.Contains(r)).ToList();
            var indirectRelationships = new HashSet<KeyEventRelationship>();
            foreach (var node in rootNodes) {
                _ = GetIndirectRelationshipsRecursive(node, fromNodesLookup, indirectRelationships);
            }
            return indirectRelationships;
        }

        private static IList<AopNetworkLayer> GetAopNetworkLayersRecursive(
            ICollection<KeyEvent> currentNodes,
            ILookup<KeyEvent, KeyEvent> toNodesLookup,
            ILookup<KeyEvent, KeyEvent> fromNodesLookup,
            HashSet<KeyEvent> visited
        ) {
            var layers = new List<AopNetworkLayer>();

            var biologicalOrganisations = currentNodes.Select(r => r.BiologicalOrganisation).Distinct().ToList();
            var processBiologicalOrganisationLevel = biologicalOrganisations.Min();
            var processNodes = (biologicalOrganisations.Count > 1)
                ? currentNodes.Where(r => r.BiologicalOrganisation == processBiologicalOrganisationLevel).ToList()
                : currentNodes;

            layers.Add(new AopNetworkLayer(processNodes));
            foreach (var node in processNodes) {
                visited.Add(node);
            }
            var postPonedNodes = currentNodes.Except(processNodes).ToList();
            var nextLayerNodes = processNodes
                .SelectMany(r => fromNodesLookup[r])
                .Distinct()
                .Where(r => toNodesLookup[r].All(n => r == n || visited.Contains(n)))
                .ToList();
            nextLayerNodes.AddRange(postPonedNodes);

            if (nextLayerNodes.Any()) {
                layers.AddRange(GetAopNetworkLayersRecursive(nextLayerNodes, toNodesLookup, fromNodesLookup, visited));
            }
            return layers;
        }

        private static ICollection<KeyEventRelationship> FindFeedbackRelationshipsRecursive(
            KeyEventRelationship keyEventRelationship,
            ILookup<KeyEvent, KeyEventRelationship> fromNodesLookup,
            HashSet<KeyEventRelationship> visited
        ) {
            if (visited.Contains(keyEventRelationship)) {
                return new List<KeyEventRelationship>() { keyEventRelationship };
            } else {
                visited = new HashSet<KeyEventRelationship>(visited);
                visited.Add(keyEventRelationship);
                var linkedKeyEventRelationships = fromNodesLookup.Contains(keyEventRelationship.ToNode)
                    ? fromNodesLookup[keyEventRelationship.ToNode].ToList()
                    : new List<KeyEventRelationship>();
                return linkedKeyEventRelationships
                    .SelectMany(r => FindFeedbackRelationshipsRecursive(r, fromNodesLookup, visited))
                    .Distinct()
                    .ToList();
            }
        }

        private static HashSet<KeyEvent> GetIndirectRelationshipsRecursive(
            KeyEvent keyEvent,
            ILookup<KeyEvent, KeyEventRelationship> fromNodesLookup,
            HashSet<KeyEventRelationship> indirectRelationships
        ) {
            var result = new HashSet<KeyEvent>();
            var toNodes = fromNodesLookup.Contains(keyEvent)
                ? fromNodesLookup[keyEvent]
                    .Where(r => r.ToNode != keyEvent)
                    .ToList()
                : new List<KeyEventRelationship>();

            var allIndirectToNodes = new HashSet<KeyEvent>();
            foreach (var toNode in toNodes) {
                allIndirectToNodes.UnionWith(
                    GetIndirectRelationshipsRecursive(toNode.ToNode, fromNodesLookup, indirectRelationships)
                );
            }

            indirectRelationships.UnionWith(toNodes.Where(r => allIndirectToNodes.Contains(r.ToNode)));
            allIndirectToNodes.UnionWith(toNodes.Select(r => r.ToNode));
            return allIndirectToNodes;
        }
    }
}
