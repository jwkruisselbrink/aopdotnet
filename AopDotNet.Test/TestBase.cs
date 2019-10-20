using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace AopDotNet.Test {
    [TestClass]
    public class TestBase {

        protected static string _outputPath = "../../../TestOutput";

        [TestInitialize]
        public void TestInitialize() {
            if (!Directory.Exists(_outputPath)) {
                Directory.CreateDirectory(_outputPath);
                Thread.Sleep(500);
            }
        }
    }
}
