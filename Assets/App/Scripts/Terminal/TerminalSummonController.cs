using System.Collections;
using UnityEngine;

namespace VibeForge.Terminal
{
    /// <summary>
    /// Owns the terminal panel's presence and pose: spawn reveal, recenter,
    /// dismiss/summon, and the controller toggle shortcut. The panel never
    /// follows the head after placement.
    /// </summary>
    public sealed class TerminalSummonController : MonoBehaviour
    {
        [SerializeField] GameObject _panelRoot;
        [SerializeField] Transform _headAnchor;
        [SerializeField] CanvasGroup _revealGroup;

        public void Inject(GameObject panelRoot, Transform headAnchor, CanvasGroup revealGroup)
        {
            _panelRoot = panelRoot;
            _headAnchor = headAnchor;
            _revealGroup = revealGroup;
        }

        const float k_SpawnDistance = 1.2f;
        const float k_PitchDownDegrees = 12f;
        const float k_RevealSeconds = 0.25f;

        Coroutine _reveal;

        void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
            {
                if (_panelRoot.activeSelf)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        public void Show()
        {
            _panelRoot.SetActive(true);
            if (_reveal != null)
            {
                StopCoroutine(_reveal);
            }

            _reveal = StartCoroutine(Reveal());
        }

        public void Hide()
        {
            if (_reveal != null)
            {
                StopCoroutine(_reveal);
                _reveal = null;
            }

            _panelRoot.SetActive(false);
        }

        public void Recenter()
        {
            PlaceInFrontOfHead();
            if (!_panelRoot.activeSelf)
            {
                Show();
            }
        }

        public void PlaceInFrontOfHead()
        {
            Vector3 forward = Quaternion.Euler(k_PitchDownDegrees, 0f, 0f) * _headAnchor.forward;
            Vector3 position = _headAnchor.position + forward * k_SpawnDistance;
            _panelRoot.transform.position = position;
            Vector3 face = _headAnchor.position - position;
            face.y = 0f;
            if (face.sqrMagnitude > 0.0001f)
            {
                // Canvas front (+Z) must look at the operator.
                _panelRoot.transform.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
            }
        }

        IEnumerator Reveal()
        {
            float elapsed = 0f;
            _panelRoot.transform.localScale = Vector3.one * 0.92f;
            _revealGroup.alpha = 0f;
            while (elapsed < k_RevealSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / k_RevealSeconds);
                _panelRoot.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, t);
                _revealGroup.alpha = t;
                yield return null;
            }

            _panelRoot.transform.localScale = Vector3.one;
            _revealGroup.alpha = 1f;
            _reveal = null;
        }
    }
}
