using AopDotNet.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AopDotNet.Test {
    [TestClass]
    public class AopNetworkGraphCreatorTests : TestBase {

        [TestMethod]
        public void AopNetworkGraphCreator_TestCreate1() {
            var kes = new[] {
                new KeyEvent("MIE1", "Molecular initiating event 1", BiologicalOrganisation.Molecular),
                new KeyEvent("MIE2", "Molecular initiating event 2", BiologicalOrganisation.Molecular),
                new KeyEvent("KE1", "Key event 1", BiologicalOrganisation.Organelle),
                new KeyEvent("KE2", "Key event 2", BiologicalOrganisation.Organelle),
                new KeyEvent("AO", "Adverse Outcome", BiologicalOrganisation.Individual),
            }.ToDictionary(r => r.Id);
            var kers = createEdges(kes, new[] { ("MIE1", "KE1"), ("MIE2", "KE2"), ("KE1", "AO"), ("KE2", "AO") });
            var network = new AopNetwork("AOP network test 1", kes.Values, kers);
            createSvg(network, "AopNetworkGraphCreator_TestCreate1.svg");
        }

        [TestMethod]
        public void AopNetworkGraphCreator_TestCreate2() {
            var kes = new[] {
                new KeyEvent("MIE1", "Molecular initiating event 1", BiologicalOrganisation.Molecular),
                new KeyEvent("MIE2", "Molecular initiating event 2", BiologicalOrganisation.Molecular),
                new KeyEvent("KE1A", "Key event 1A", BiologicalOrganisation.Cellular),
                new KeyEvent("KE1B", "Key event 1B", BiologicalOrganisation.Organelle),
                new KeyEvent("KE2", "Key event 2", BiologicalOrganisation.Organelle),
                new KeyEvent("AO", "Adverse Outcome", BiologicalOrganisation.Individual),
            }.ToDictionary(r => r.Id);
            var kers = createEdges(kes, new[] {
                ("MIE1", "KE1A"),
                ("KE1A", "KE1B"),
                ("MIE2", "KE2"),
                ("KE1B", "AO"),
                ("KE2", "AO")
            });
            var network = new AopNetwork("AOP network test 2", kes.Values, kers);
            createSvg(network, "AopNetworkGraphCreator_TestCreate2.svg");
        }

        [TestMethod]
        public void AopNetworkGraphCreator_TestCreate3() {
            var kes = new[] {
                new KeyEvent("MIE1", "Molecular initiating event 1", BiologicalOrganisation.Molecular),
                new KeyEvent("MIE2", "Molecular initiating event 2", BiologicalOrganisation.Molecular),
                new KeyEvent("MIE3", "Molecular initiating event 3", BiologicalOrganisation.Molecular),
                new KeyEvent("MIE4", "Molecular initiating event 4", BiologicalOrganisation.Molecular),
                new KeyEvent("KE1A", "Key event 1A", BiologicalOrganisation.Cellular),
                new KeyEvent("KE1B", "Key event 1B", BiologicalOrganisation.Organelle),
                new KeyEvent("KE2", "Key event 2", BiologicalOrganisation.Organelle),
                new KeyEvent("AO", "Adverse Outcome", BiologicalOrganisation.Individual),
            }.ToDictionary(r => r.Id);
            var kers = createEdges(kes, new[] {
                ("MIE1", "KE1A"),
                ("KE1A", "KE1B"),
                ("MIE2", "KE2"),
                ("MIE3", "KE1A"),
                ("MIE3", "KE2"),
                ("MIE4", "KE2"),
                ("MIE4", "KE1B"),
                ("KE1B", "AO"),
                ("KE2", "AO")
            });
            var network = new AopNetwork("AOP network test 3", kes.Values, kers);
            createSvg(network, "AopNetworkGraphCreator_TestCreate3.svg");
        }

        [TestMethod]
        public void AopNetworkGraphCreator_TestCreateSteatosisPartial() {
            var kes = new[] {
                new KeyEvent("triglyceride-accum-liver", "Triglyceride accumulation liver", BiologicalOrganisation.Cellular),
                new KeyEvent("nucleus-distort-liver", "Nucleus distortion liver", BiologicalOrganisation.Cellular),
                new KeyEvent("mitochondrial-disrupt-liver", "Mitochondrial disruption liver", BiologicalOrganisation.Cellular),
                new KeyEvent("ER-stress-liver", "Endoplasmatic reticulum stress liver", BiologicalOrganisation.Cellular),
                new KeyEvent("cytoplasm-displ-liver", "Cytoplasm displacement liver", BiologicalOrganisation.Cellular),
                new KeyEvent("FattyCells-liver", "Fatty liver cells", BiologicalOrganisation.Tissue),
                new KeyEvent("Steatosis-liver", "Liver steatosis", BiologicalOrganisation.Organelle),
            }.ToDictionary(r => r.Id);
            var kers = createEdges(kes, new[] {
                ("triglyceride-accum-liver", "nucleus-distort-liver"),
                ("triglyceride-accum-liver", "mitochondrial-disrupt-liver"),
                ("triglyceride-accum-liver", "ER-stress-liver"),
                ("triglyceride-accum-liver", "cytoplasm-displ-liver"),
                ("nucleus-distort-liver", "FattyCells-liver"),
                ("mitochondrial-disrupt-liver", "FattyCells-liver"),
                ("ER-stress-liver", "FattyCells-liver"),
                ("cytoplasm-displ-liver", "FattyCells-liver"),
                ("FattyCells-liver", "Steatosis-liver")
            });
            var network = new AopNetwork("AOP network test steatosis (partial)", kes.Values, kers);
            createSvg(network, "AopNetworkGraphCreator_TestCreateSteatosisPartial.svg");
        }

        private static List<KeyEventRelationship> createEdges(IDictionary<string, KeyEvent> nodes, (string, string)[] edges) {
            return edges
                .Select(r => new KeyEventRelationship($"{r.Item1}>{r.Item2}", nodes[r.Item1], nodes[r.Item2]))
                .ToList();
        }

        private static void createSvg(AopNetwork network, string fileName) {
            var visualizer = new AopNetworkGraphCreator();
            visualizer.Create(network, Path.Combine(_outputPath, fileName));
        }
    }
}
