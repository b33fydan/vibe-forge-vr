using Oculus.Interaction;
using UnityEngine;

namespace VibeForge.Terminal
{
    /// <summary>
    /// Parses submitted lines, records history, executes the deterministic
    /// command set, and renders results. Also supplies the live status and
    /// agent roster the commands report on.
    /// </summary>
    public sealed class FloatingTerminalController : MonoBehaviour, ITerminalContext
    {
        [SerializeField] TerminalView _view;
        [SerializeField] TerminalSummonController _summon;
        [SerializeField] OVRPassthroughLayer _passthroughLayer;

        public void Inject(
            TerminalView view,
            TerminalSummonController summon,
            OVRPassthroughLayer passthroughLayer)
        {
            _view = view;
            _summon = summon;
            _passthroughLayer = passthroughLayer;
        }

        readonly TerminalHistory _history = new TerminalHistory();

        static readonly string[] k_Header =
        {
            "VIBE FORGE AR",
            "Spatial agentic workspace proof",
            string.Empty,
        };

        void OnEnable()
        {
            _view.Submitted += Submit;
            _view.HelpRequested += () => Submit("help");
            _view.ClearRequested += () => Submit("clear");
            _view.RecenterRequested += Recenter;
            _view.CloseRequested += () => _summon.Hide();
            PointableCanvasModule.WhenSelectableHovered += OnCanvasHovered;
            PointableCanvasModule.WhenSelectableUnhovered += OnCanvasUnhovered;

            _view.ClearOutput();
            _view.AppendLines(k_Header);
            TerminalResult status = TerminalCommandExecutor.Execute(
                TerminalCommandParser.Parse("status"), this);
            _view.AppendLines(status.Lines);
        }

        void OnDisable()
        {
            _view.Submitted -= Submit;
            PointableCanvasModule.WhenSelectableHovered -= OnCanvasHovered;
            PointableCanvasModule.WhenSelectableUnhovered -= OnCanvasUnhovered;
        }

        public void Submit(string line)
        {
            TerminalCommand command = TerminalCommandParser.Parse(line);
            if (command.IsEmpty)
            {
                return;
            }

            _history.Add(command.Raw);
            TerminalResult result = TerminalCommandExecutor.Execute(command, this);
            if (result.ClearScreen)
            {
                _history.Clear();
                _view.ClearOutput();
                return;
            }

            var block = new string[result.Lines.Length + 1];
            block[0] = "> " + command.Raw;
            for (int i = 0; i < result.Lines.Length; i++)
            {
                block[i + 1] = result.Lines[i];
            }

            _view.AppendLines(block);
        }

        void Recenter()
        {
            _summon.Recenter();
        }

        void OnCanvasHovered(PointableCanvasEventArgs args)
        {
            _view.SetHover(true);
        }

        void OnCanvasUnhovered(PointableCanvasEventArgs args)
        {
            _view.SetHover(false);
        }

        string[] ITerminalContext.GetStatusLines()
        {
            bool passthrough = _passthroughLayer != null && _passthroughLayer.enabled;
            bool controller = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
            return new[]
            {
                "PASSTHROUGH   " + (passthrough ? "READY" : "OFF"),
                "CONTROLLER    " + (controller ? "TRACKED" : "MISSING"),
                "SPACE         SESSION-LOCKED",
            };
        }

        string[] ITerminalContext.GetAgentLines()
        {
            return new[]
            {
                "agents (preview roster):",
                "- forge-1   idle   spatial build",
                "- relay-2   idle   bridge standby",
            };
        }
    }
}
