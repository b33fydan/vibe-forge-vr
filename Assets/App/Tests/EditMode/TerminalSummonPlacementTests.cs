using NUnit.Framework;
using UnityEngine;

namespace VibeForge.Terminal.Tests
{
    public sealed class TerminalSummonPlacementTests
    {
        [Test]
        public void PlaceInFrontOfHead_PutsPanelAheadFacingHead()
        {
            var head = new GameObject("VFTestHead");
            var panel = new GameObject("VFTestPanel");
            var summonGo = new GameObject("VFTestSummon");
            try
            {
                head.transform.position = new Vector3(0f, 1.6f, 0f);
                head.transform.rotation = Quaternion.identity;
                var summon = summonGo.AddComponent<TerminalSummonController>();
                summon.Inject(panel, head.transform, null);
                summon.PlaceInFrontOfHead();
                Assert.AreEqual(
                    1.2f,
                    Vector3.Distance(head.transform.position, panel.transform.position),
                    0.001f);
                Vector3 toHead = head.transform.position - panel.transform.position;
                Vector3 panelForward = panel.transform.rotation * Vector3.forward;
                float yawError = Vector3.Angle(
                    new Vector3(toHead.x, 0f, toHead.z).normalized,
                    new Vector3(panelForward.x, 0f, panelForward.z).normalized);
                Assert.LessOrEqual(yawError, 0.5f);
            }
            finally
            {
                Object.DestroyImmediate(head);
                Object.DestroyImmediate(panel);
                Object.DestroyImmediate(summonGo);
            }
        }
    }
}
