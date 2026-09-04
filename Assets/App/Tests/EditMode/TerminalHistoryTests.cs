using NUnit.Framework;

namespace VibeForge.Terminal.Tests
{
    public sealed class TerminalHistoryTests
    {
        [Test]
        public void Add_AppendsInOrder()
        {
            var history = new TerminalHistory();
            history.Add("help");
            history.Add("status");
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual("help", history.Entries[0]);
            Assert.AreEqual("status", history.Entries[1]);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Add_BlankLine_IsIgnored(string line)
        {
            var history = new TerminalHistory();
            history.Add(line);
            Assert.AreEqual(0, history.Count);
        }

        [Test]
        public void Clear_EmptiesHistory()
        {
            var history = new TerminalHistory();
            history.Add("help");
            history.Clear();
            Assert.AreEqual(0, history.Count);
        }

        [Test]
        public void Add_BeyondCapacity_DropsOldest()
        {
            var history = new TerminalHistory(capacity: 3);
            history.Add("one");
            history.Add("two");
            history.Add("three");
            history.Add("four");
            Assert.AreEqual(3, history.Count);
            CollectionAssert.AreEqual(new[] { "two", "three", "four" }, history.Entries);
        }

        [Test]
        public void Ctor_NonPositiveCapacity_FallsBackToDefault()
        {
            var history = new TerminalHistory(capacity: 0);
            for (int i = 0; i < TerminalHistory.DefaultCapacity + 1; i++)
            {
                history.Add("line " + i);
            }

            Assert.AreEqual(TerminalHistory.DefaultCapacity, history.Count);
        }
    }
}
