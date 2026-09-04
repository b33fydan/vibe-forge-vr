using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VibeForge.Terminal
{
    /// <summary>
    /// Binds the terminal panel's uGUI widgets. All references are assigned
    /// by the terminal setup utility; everything here is event-driven, with
    /// no per-frame work.
    /// </summary>
    public sealed class TerminalView : MonoBehaviour
    {
        const int k_MaxLines = 300;
        const int k_VisibleLines = 17;

        [SerializeField] TextMeshProUGUI _outputText;
        [SerializeField] TMP_InputField _inputField;
        [SerializeField] Image _borderImage;
        [SerializeField] Image _titleBarImage;
        [SerializeField] Button _helpButton;
        [SerializeField] Button _clearButton;
        [SerializeField] Button _recenterButton;
        [SerializeField] Button _closeButton;

        readonly List<string> _lines = new List<string>();

        public void Inject(
            TextMeshProUGUI outputText,
            TMP_InputField inputField,
            Image borderImage,
            Image titleBarImage,
            Button helpButton,
            Button clearButton,
            Button recenterButton,
            Button runButton,
            Button closeButton)
        {
            _outputText = outputText;
            _inputField = inputField;
            _borderImage = borderImage;
            _titleBarImage = titleBarImage;
            _helpButton = helpButton;
            _clearButton = clearButton;
            _recenterButton = recenterButton;
            _closeButton = closeButton;
            runButton.onClick.AddListener(SubmitInputField);
        }

        public event System.Action<string> Submitted;
        public event System.Action HelpRequested;
        public event System.Action ClearRequested;
        public event System.Action RecenterRequested;
        public event System.Action CloseRequested;

        static readonly Color k_BorderIdle = new Color(0.85f, 0.68f, 0.25f, 0.55f);
        static readonly Color k_BorderActive = new Color(1.0f, 0.80f, 0.35f, 1.0f);
        static readonly Color k_TitleIdle = new Color(0.13f, 0.12f, 0.10f, 1.0f);
        static readonly Color k_TitleGrabbed = new Color(0.30f, 0.24f, 0.12f, 1.0f);

        void Awake()
        {
            _inputField.onSubmit.AddListener(OnInputSubmitted);
            _helpButton.onClick.AddListener(() => HelpRequested?.Invoke());
            _clearButton.onClick.AddListener(() => ClearRequested?.Invoke());
            _recenterButton.onClick.AddListener(() => RecenterRequested?.Invoke());
            _closeButton.onClick.AddListener(() => CloseRequested?.Invoke());
            SetBorderIdle();
            SetGrabbed(false);
        }

        void OnDestroy()
        {
            _inputField.onSubmit.RemoveListener(OnInputSubmitted);
            _helpButton.onClick.RemoveAllListeners();
            _clearButton.onClick.RemoveAllListeners();
            _recenterButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }

        void OnInputSubmitted(string text)
        {
            Submitted?.Invoke(text);
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        void SubmitInputField()
        {
            OnInputSubmitted(_inputField.text);
        }

        public void AppendLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                _lines.Add(line);
            }

            while (_lines.Count > k_MaxLines)
            {
                _lines.RemoveAt(0);
            }

            int first = Mathf.Max(0, _lines.Count - k_VisibleLines);
            _outputText.text = string.Join("\n", _lines.GetRange(first, _lines.Count - first).ToArray());
        }

        public void ClearOutput()
        {
            _lines.Clear();
            _outputText.text = string.Empty;
        }

        public void FocusInput()
        {
            _inputField.ActivateInputField();
        }

        public void SetHover(bool hovered)
        {
            _borderImage.color = hovered ? k_BorderActive : k_BorderIdle;
        }

        public void SetGrabbed(bool grabbed)
        {
            _titleBarImage.color = grabbed ? k_TitleGrabbed : k_TitleIdle;
            SetBorderIdle();
        }

        void SetBorderIdle()
        {
            _borderImage.color = k_BorderIdle;
        }
    }
}
