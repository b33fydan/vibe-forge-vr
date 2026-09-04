using System;

namespace VibeForge.Terminal
{
    /// <summary>
    /// Deterministic parser for Terminal Zero input lines. No I/O, no Unity
    /// dependencies; safe to unit test anywhere.
    /// </summary>
    public static class TerminalCommandParser
    {
        static readonly char[] k_Separators = { ' ', '\t' };

        public static TerminalCommand Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return TerminalCommand.Empty;
            }

            string[] tokens = line.Split(k_Separators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return TerminalCommand.Empty;
            }

            string name = tokens[0].ToLowerInvariant();
            string[] arguments = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, arguments, 0, arguments.Length);
            return new TerminalCommand(line, name, arguments);
        }
    }
}
