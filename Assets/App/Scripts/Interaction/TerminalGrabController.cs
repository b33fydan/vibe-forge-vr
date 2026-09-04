using Oculus.Interaction;
using UnityEngine;

namespace VibeForge.Terminal
{
    /// <summary>
    /// Reflects title-bar grab state on the terminal visuals. Movement itself
    /// is handled by the Interaction SDK distance-grab interactable.
    /// </summary>
    public sealed class TerminalGrabController : MonoBehaviour
    {
        [SerializeField] DistanceGrabInteractable _grabInteractable;
        [SerializeField] TerminalView _view;

        public void Inject(DistanceGrabInteractable grabInteractable, TerminalView view)
        {
            _grabInteractable = grabInteractable;
            _view = view;
        }

        void OnEnable()
        {
            _grabInteractable.WhenStateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            _grabInteractable.WhenStateChanged -= OnStateChanged;
        }

        void OnStateChanged(InteractableStateChangeArgs args)
        {
            _view.SetGrabbed(args.NewState == InteractableState.Select);
        }
    }
}
