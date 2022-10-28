using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetScript.Tests
{
    [TestClass]
    public class NetScriptTest
    {
        [TestMethod]
        public void Variables()
        {
            Interpretation.VariableCollection vars = NS.Run("var a = 1; var b = 2; var c = a + b;");

            Assert.IsTrue(vars.ContainsKey("a"));
            Assert.IsTrue(vars.ContainsKey("b"));
            Assert.IsTrue(vars.ContainsKey("c"));

            Assert.IsInstanceOfType(vars["a"], typeof(int));
            Assert.IsInstanceOfType(vars["b"], typeof(int));
            Assert.IsInstanceOfType(vars["c"], typeof(int));

            Assert.AreEqual(vars["a"], 1);
            Assert.AreEqual(vars["b"], 2);
            Assert.AreEqual(vars["c"], 3);
        }

        [TestMethod]
        public void Numerics()
        {
            Interpretation.VariableCollection vars = NS.Run("var n = null; var b = true; var by = 1b; var sby = 2sb;" +
                "var sh = 3s; var ush = 4us; var i = 5; var ui = 6ui; var l = 7l; var ul = 8ul;");

            Assert.IsTrue(vars.ContainsKey("n"));
            Assert.IsTrue(vars.ContainsKey("b"));
            Assert.IsTrue(vars.ContainsKey("by"));
            Assert.IsTrue(vars.ContainsKey("sby"));
            Assert.IsTrue(vars.ContainsKey("sh"));
            Assert.IsTrue(vars.ContainsKey("ush"));
            Assert.IsTrue(vars.ContainsKey("i"));
            Assert.IsTrue(vars.ContainsKey("ui"));
            Assert.IsTrue(vars.ContainsKey("l"));
            Assert.IsTrue(vars.ContainsKey("ul"));

            Assert.AreEqual(vars["n"], null);

            Assert.IsInstanceOfType(vars["b"], typeof(bool));
            Assert.IsInstanceOfType(vars["by"], typeof(byte));
            Assert.IsInstanceOfType(vars["sby"], typeof(sbyte));
            Assert.IsInstanceOfType(vars["sh"], typeof(short));
            Assert.IsInstanceOfType(vars["ush"], typeof(ushort));
            Assert.IsInstanceOfType(vars["i"], typeof(int));
            Assert.IsInstanceOfType(vars["ui"], typeof(uint));
            Assert.IsInstanceOfType(vars["l"], typeof(long));
            Assert.IsInstanceOfType(vars["ul"], typeof(ulong));
        }
    }
}