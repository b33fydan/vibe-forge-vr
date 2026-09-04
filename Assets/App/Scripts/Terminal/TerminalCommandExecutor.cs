namespace VibeForge.Terminal
{
    /// <summary>
    /// Live state the deterministic command set reports on. Implemented by
    /// the scene controller; stubbed in tests.
    /// </summary>
    public interface ITerminalContext
    {
        string[] GetStatusLines();
        string[] GetAgentLines();
    }

    /// <summary>
    /// Outcome of executing one parsed command.
    /// </summary>
    public struct TerminalResult
    {
        public string[] Lines;
        public bool ClearScreen;
    }

    /// <summary>
    /// Deterministic v0 command set: help, status, agents, clear, about.
    /// No shell, no network, no I/O. Unknown commands fail gracefully.
    /// </summary>
    public static class TerminalCommandExecutor
    {
        static readonly string[] k_Empty = new string[0];

        public static TerminalResult Execute(TerminalCommand command, ITerminalContext context)
        {
            if (command.IsEmpty)
            {
                return new TerminalResult { Lines = k_Empty, ClearScreen = false };
            }

            switch (command.Name)
            {
                case "help":
                    return new TerminalResult
                    {
                        Lines = new[]
                        {
                            "commands: help status agents clear about",
                            "type a command and press Enter (or RUN)",
                        },
                        ClearScreen = false,
                    };
                case "status":
                    return new TerminalResult
                    {
                        Lines = context != null ? context.GetStatusLines() : k_Empty,
                        ClearScreen = false,
                    };
                case "agents":
                    return new TerminalResult
                    {
                        Lines = context != null ? context.GetAgentLines() : k_Empty,
                        ClearScreen = false,
                    };
                case "about":
                    return new TerminalResult
                    {
                        Lines = new[]
                        {
                            "TERMINAL ZERO - first Vibe Forge spatial module",
                            "local proof build: no shell, no network",
                        },
                        ClearScreen = false,
                    };
                case "clear":
                    return new TerminalResult { Lines = k_Empty, ClearScreen = true };
                default:
                    return new TerminalResult
                    {
                        Lines = new[] { "command not found: " + command.Name },
                        ClearScreen = false,
                    };
            }
        }
    }
}
