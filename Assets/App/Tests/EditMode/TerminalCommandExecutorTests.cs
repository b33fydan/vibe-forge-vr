using NUnit.Framework;

namespace VibeForge.Terminal.Tests
{
    sealed class StubContext : ITerminalContext
    {
        public string[] GetStatusLines()
        {
            return new[] { "STATUS-1", "STATUS-2" };
        }

        public string[] GetAgentLines()
        {
            return new[] { "AGENT-1" };
        }
    }

    public sealed class TerminalCommandExecutorTests
    {
        readonly StubContext _context = new StubContext();

        TerminalResult Run(string line)
        {
            return TerminalCommandExecutor.Execute(TerminalCommandParser.Parse(line), _context);
        }

        [Test]
        public void Execute_Empty_ReturnsNoLines()
        {
            TerminalResult result = Run("   ");
            Assert.IsFalse(result.ClearScreen);
            Assert.AreEqual(0, result.Lines.Length);
        }

        [Test]
        public void Execute_Help_ListsCommands()
        {
            TerminalResult result = Run("help");
            Assert.IsFalse(result.ClearScreen);
            Assert.GreaterOrEqual(result.Lines.Length, 1);
            StringAssert.Contains("help", result.Lines[0]);
        }

        [Test]
        public void Execute_Status_ForwardsContextLines()
        {
            TerminalResult result = Run("STATUS");
            CollectionAssert.AreEqual(new[] { "STATUS-1", "STATUS-2" }, result.Lines);
        }

        [Test]
        public void Execute_Agents_ForwardsContextLines()
        {
            TerminalResult result = Run("agents");
            CollectionAssert.AreEqual(new[] { "AGENT-1" }, result.Lines);
        }

        [Test]
        public void Execute_About_DescribesProof()
        {
            TerminalResult result = Run("about");
            Assert.GreaterOrEqual(result.Lines.Length, 1);
        }

        [Test]
        public void Execute_Clear_RequestsClearScreen()
        {
            TerminalResult result = Run("clear");
            Assert.IsTrue(result.ClearScreen);
        }

        [Test]
        public void Execute_Unknown_ReturnsNotFound()
        {
            TerminalResult result = Run("frobnicate");
            Assert.IsFalse(result.ClearScreen);
            Assert.AreEqual(1, result.Lines.Length);
            StringAssert.Contains("frobnicate", result.Lines[0]);
        }
    }
}
