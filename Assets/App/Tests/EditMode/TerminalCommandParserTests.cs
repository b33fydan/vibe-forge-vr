using NUnit.Framework;

namespace VibeForge.Terminal.Tests
{
    public sealed class TerminalCommandParserTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t \t")]
        public void Parse_BlankInput_ReturnsEmpty(string line)
        {
            TerminalCommand command = TerminalCommandParser.Parse(line);
            Assert.IsTrue(command.IsEmpty);
        }

        [Test]
        public void Parse_SimpleCommand_ReturnsNameWithoutArguments()
        {
            TerminalCommand command = TerminalCommandParser.Parse("help");
            Assert.IsFalse(command.IsEmpty);
            Assert.AreEqual("help", command.Name);
            Assert.AreEqual(0, command.Arguments.Length);
            Assert.AreEqual("help", command.Raw);
        }

        [Test]
        public void Parse_PaddedCommand_TrimsAndKeepsRaw()
        {
            TerminalCommand command = TerminalCommandParser.Parse("  STATUS  ");
            Assert.AreEqual("status", command.Name);
            Assert.AreEqual("  STATUS  ", command.Raw);
        }

        [Test]
        public void Parse_MixedCase_MatchesLowercase()
        {
            Assert.AreEqual("help", TerminalCommandParser.Parse("HeLp").Name);
        }

        [Test]
        public void Parse_CommandWithArguments_SplitsAndPreservesCase()
        {
            TerminalCommand command = TerminalCommandParser.Parse("about Extra Args");
            Assert.AreEqual("about", command.Name);
            CollectionAssert.AreEqual(new[] { "Extra", "Args" }, command.Arguments);
        }

        [Test]
        public void Parse_UnknownCommand_PreservesNameForNotFoundResponse()
        {
            TerminalCommand command = TerminalCommandParser.Parse("frobnicate -x");
            Assert.AreEqual("frobnicate", command.Name);
            CollectionAssert.AreEqual(new[] { "-x" }, command.Arguments);
        }

        [Test]
        public void Parse_TabSeparated_SplitsLikeSpaces()
        {
            TerminalCommand command = TerminalCommandParser.Parse("agents\tguild");
            Assert.AreEqual("agents", command.Name);
            CollectionAssert.AreEqual(new[] { "guild" }, command.Arguments);
        }
    }
}
