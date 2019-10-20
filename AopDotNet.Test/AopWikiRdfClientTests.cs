using AopDotNet.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace AopDotNet.Test {

    [TestClass]
    public class AopWikiRdfClientTests : TestBase {

        [TestMethod]
        public void WikiPathwaysClient_Test1() {
            var client = new AopWikiRdfClient(new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/"));
            var result = client.GetAllAopNetworks();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void WikiPathwaysClient_Test() {
            var client = new AopWikiRdfClient(new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/"));
            var ids = new[] { "3", "6", "17", "34", "220", "202" };
            foreach (var id in ids) {
                var network = client.GetAopNetwork(id);
                var feedback = network.FindFeedbackRelationships();
                var indirect = network.GetIndirectKeyEventRelationships();
                createSvg(network, $"WikiPathwaysClient_Test_{id}.svg");
            }
        }

        private static void createSvg(AopNetwork network, string fileName) {
            var visualizer = new AopNetworkGraphCreator();
            visualizer.Create(network, Path.Combine(_outputPath, fileName));
        }
    }
}
