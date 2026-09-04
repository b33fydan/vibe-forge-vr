using UnityEngine;

namespace VibeForge.Runtime
{
    /// <summary>
    /// Forces XR eye cameras to solid-color transparent output so the
    /// passthrough underlay shows the real room. The OVRCameraRig prefab
    /// ships skybox clear flags, which would paint over passthrough.
    /// Wire the rig eye cameras in the scene; missing wiring fails loudly.
    /// </summary>
    public sealed class VFPassthroughCamera : MonoBehaviour
    {
        static readonly Color k_Transparent = new Color(0f, 0f, 0f, 0f);

        [SerializeField] Camera[] _eyeCameras;

        public void Inject(Camera[] eyeCameras)
        {
            _eyeCameras = eyeCameras;
        }

        void Awake()
        {
            if (Apply(_eyeCameras) == 0)
            {
                Debug.LogError("VF_PASSTHROUGH_FAIL no eye cameras wired on " + name);
            }
        }

        /// <summary>Applies passthrough clear state; returns cameras touched.</summary>
        public static int Apply(Camera[] eyeCameras)
        {
            if (eyeCameras == null)
            {
                return 0;
            }

            int applied = 0;
            foreach (Camera camera in eyeCameras)
            {
                if (camera == null)
                {
                    continue;
                }

                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = k_Transparent;
                applied++;
            }

            return applied;
        }
    }
}
