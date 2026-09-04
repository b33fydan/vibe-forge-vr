using NUnit.Framework;
using UnityEngine;

namespace VibeForge.Runtime.Tests
{
    public sealed class VFPassthroughCameraTests
    {
        [Test]
        public void Apply_SetsSolidTransparentBackground()
        {
            var go = new GameObject("VFTestCamera");
            try
            {
                Camera camera = go.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Skybox;
                int applied = VFPassthroughCamera.Apply(new[] { camera });
                Assert.AreEqual(1, applied);
                Assert.AreEqual(CameraClearFlags.SolidColor, camera.clearFlags);
                Assert.AreEqual(new Color(0f, 0f, 0f, 0f), camera.backgroundColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Apply_NullOrEmpty_TouchesNothing()
        {
            Assert.AreEqual(0, VFPassthroughCamera.Apply(null));
            Assert.AreEqual(0, VFPassthroughCamera.Apply(new Camera[0]));
            Assert.AreEqual(0, VFPassthroughCamera.Apply(new Camera[] { null }));
        }

        [Test]
        public void Apply_SkipsNullEntries()
        {
            var go = new GameObject("VFTestCamera");
            try
            {
                Camera camera = go.AddComponent<Camera>();
                int applied = VFPassthroughCamera.Apply(new Camera[] { null, camera });
                Assert.AreEqual(1, applied);
                Assert.AreEqual(CameraClearFlags.SolidColor, camera.clearFlags);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
