# Vibe Forge AR - Step-by-Step Build Guide

## Goal

Build a standalone mixed-reality application for Meta Quest 3 that launches into color passthrough, finds real surfaces, previews a placement point, places a virtual object with the right controller trigger, and lets the user grab and move it.

On Quest, this is technically mixed reality (MR): virtual content is composited over the real room through passthrough. We will use "AR" in the product name and "MR" when referring to Meta's SDK features.

## First playable: the Vibe Forge AR vertical slice

The first version will do exactly this:

1. Launch on a Quest 3 as a standalone Android APK.
2. Show color passthrough.
3. Track the right Touch controller.
4. Cast a ray into the live physical environment.
5. Show a placement marker only when a valid surface is detected.
6. Place one simple forge object when the right index trigger is pressed.
7. Let the player grab, move, and release the object.
8. Keep placed content spatially stable for the current session.
9. Produce useful Unity build logs and Quest device logs.

This deliberately excludes persistent anchors, hand tracking, multiplayer, object recognition, generative AI, and polished game systems. Those become much easier after the core spatial loop works.

## Current workspace truth - July 11, 2026

- Repository: `/Volumes/beefybackup/vibe forge vr`
- Branch: `main`, tracking `origin/main`
- Remote: `https://github.com/b33fydan/vibe-forge-vr.git`
- Current commit: `97104bb` (`first commit`)
- Unity installed: `6000.5.0f1`
- Unity Android Build Support: not installed for the current editor
- Android SDK, NDK, and OpenJDK Unity modules: not installed
- `adb`: not currently available on the shell path
- Node.js: `v22.22.3`, which exceeds Meta's Node 18+ prerequisite
- Standalone Codex CLI: installed wrapper is present but its native executable is missing; the Codex desktop app remains our active coding environment
- No Unity project exists yet
- The source research document is preserved as `VR Game Learning with Codex.md`
- The identical lowercase duplicate has been reconciled into that canonical source
- AppleDouble `._*` sidecars are ignored and pruned because this repository is on an external volume

## Technical baseline

| Layer | Choice for this project |
| --- | --- |
| Hardware target | Meta Quest 3 |
| Runtime | Standalone Quest Android APK |
| Unity | Unity 6.3 LTS, current `6000.3.x` patch |
| Rendering | Universal Render Pipeline via Universal 3D template |
| XR provider | Unity OpenXR Plugin |
| Meta foundation | Meta XR Core SDK |
| Interaction | Meta XR Interaction SDK |
| Environment understanding | Meta XR MR Utility Kit (MRUK) |
| Initial input | Touch controllers |
| Editor simulation | Meta XR Simulator on Apple Silicon macOS |
| Source control | Existing GitHub repository, milestone commits |

Unity 6.5 may be compatible with Meta's general minimum requirements, but Meta's current AI-assisted workflow is written and tested against Unity 6.3. We will install 6.3 alongside 6.5 to reduce package and tutorial drift during the first build. We will not remove 6.5.

## Phase 1 - Make the repository safe for Unity

### Step 1: Add repository hygiene

We will add a Unity-aware `.gitignore` that excludes generated folders while preserving `.meta` files. It will also ignore macOS `.DS_Store` and AppleDouble `._*` files.

Generated paths to ignore include:

```text
Library/
Temp/
Obj/
Logs/
UserSettings/
Builds/
Artifacts/
MemoryCaptures/
```

We will retain the two research Markdown files until we deliberately choose one canonical copy. No untracked source document will be deleted casually.

Acceptance check:

- `git status` contains no AppleDouble sidecars.
- The research document remains available.
- The repository has a clean checkpoint before Unity project creation.

### Step 2: Add durable project instructions

We will create a root `AGENTS.md` that fixes the project rules:

- Runtime scripts live under `Assets/App/Scripts`.
- Editor-only scripts live under `Assets/App/Editor`.
- Tests live under `Assets/App/Tests`.
- Installed package manifests are the API source of truth.
- We do not guess Meta XR, Interaction SDK, or MRUK API names.
- We do not hand-edit scene or prefab YAML unless explicitly necessary.
- We do not casually regenerate `.meta` files.
- We do not change Unity, XR provider, or package versions during a feature task.
- Every feature ends with compile, test, build, diff, and device-check evidence.

Acceptance check:

- A future Codex session can inspect `AGENTS.md` and understand the platform, folder rules, and definition of done without relying on chat history.

## Phase 2 - Complete the Mac development toolchain

### Step 3: Install Unity 6.3 LTS with Android modules

In Unity Hub, install a current Unity `6000.3.x` editor and select:

- Android Build Support
- Android SDK and NDK Tools
- OpenJDK

Keep Unity `6000.5.0f1` installed, but create the project with 6.3.

Acceptance check:

- Unity Hub shows Unity 6.3 and all three Android components.
- The Unity 6.3 editor launches successfully.

### Step 4: Prepare the Quest 3

We will:

1. Create or join a Meta developer organization.
2. Complete developer-account verification.
3. Enable Developer Mode for the headset in the Meta Horizon mobile app.
4. Connect the Quest 3 with a USB-C data cable.
5. Put on the headset and approve USB debugging.
6. Select "Always allow from this computer."

On macOS, the Windows Oculus ADB driver is not required. We will use the Android platform tools bundled with Unity or install a shell-accessible `adb` if needed.

Acceptance check:

```bash
adb devices
```

The headset must appear with status `device`, not `unauthorized`.

## Phase 3 - Create the Quest-ready Unity foundation

### Step 5: Create the Unity project

Create a Universal 3D project named `VibeForgeAR` with Unity 6.3 LTS. The Unity project should become the product surface in this repository, with these committed roots:

```text
Assets/
Packages/
ProjectSettings/
```

Inside `Assets`, create:

```text
Assets/App/
  Art/
  Editor/
  Materials/
  Prefabs/
  Scenes/
  Scripts/
    Interaction/
    Placement/
    Runtime/
  Tests/
    EditMode/
```

Save the first scene as:

```text
Assets/App/Scenes/MainMR.unity
```

Acceptance check:

- The project opens in the intended 6.3 editor.
- The default URP scene renders without console errors.
- Unity-generated `.meta` files are tracked.
- `Library`, `Temp`, and other generated folders remain ignored.

### Step 6: Configure Quest and OpenXR

In Unity:

1. Open `File > Build Profiles`.
2. Enable or switch to the Meta Quest profile.
3. Install the Unity OpenXR Plugin if prompted.
4. In `Edit > Project Settings > XR Plug-in Management`, enable OpenXR for Meta Quest/Android.
5. Enable the Meta XR feature group under Android OpenXR settings.
6. Install Meta XR Core SDK.
7. Install Meta XR Interaction SDK.
8. Install Meta XR MR Utility Kit.
9. Run `Meta > Tools > Project Setup Tool` for the Android target.
10. Apply required fixes, then review recommended fixes before applying them.

We will record the exact resolved package versions from `Packages/manifest.json` and `Packages/packages-lock.json`. We will never use floating `latest` versions in project instructions.

Acceptance check:

- OpenXR is the active Quest XR provider.
- The Oculus XR Plugin is not added.
- The Meta Project Setup Tool reports no required fixes.
- Unity has zero compilation errors.

### Step 7: Create a minimal XR scene

Using Meta XR Building Blocks, we will add:

- Passthrough
- A single Meta XR camera rig
- Touch controller tracking
- The installed Interaction SDK's controller/ray interaction setup

We will delete or disable the ordinary Unity Main Camera if the Building Block creates an XR camera rig. There must be exactly one active camera rig.

Acceptance check:

- The hierarchy contains one XR camera rig.
- Passthrough support is configured.
- Both controllers track in the Meta XR Simulator.
- No duplicate EventSystem, camera, or input stack exists.

## Phase 4 - Prove deployment before writing gameplay

### Step 8: Build an empty Quest APK

Before placement code, build and run:

```text
Builds/Quest/VibeForgeAR-empty.apk
```

Use a Development Build and install it on the connected Quest 3.

Acceptance check:

- The APK builds successfully.
- It installs and launches on the headset.
- Passthrough appears rather than a black screen.
- Controller poses are visible or otherwise verifiably tracked.
- Required permissions appear and can be granted.

We do not add gameplay until this passes. This keeps Android, signing, OpenXR, and headset-connectivity failures separate from application-code failures.

## Phase 5 - Build the first AR interaction

### Step 9: Configure MRUK environment understanding

We will add an `MRSystems` GameObject and configure the installed MRUK environment-raycast component. We will also configure the camera rig's scene and passthrough support, required startup permissions, and Android manifest.

The exact component and API names will come from the installed package version, not from memory or a stale tutorial.

Acceptance check:

- MRUK initializes without exceptions.
- Unsupported/not-ready states are visible in concise state-transition logs.
- A missing ray hit is treated as a normal invalid-placement state.

### Step 10: Create the placement preview

Create a lightweight `PlacementPreview` prefab. It will:

- Appear only on a valid environment hit.
- Hide when MRUK is unsupported, not ready, out of depth-camera view, or has no valid hit.
- Align using an explicit surface-normal policy.
- Avoid allocations and repeated component lookups every frame.

Acceptance check:

- The preview follows simulated surfaces in the editor where supported.
- Invalid hits never leave a stale marker visible.
- Pure placement/orientation logic has EditMode tests.

### Step 11: Create the first placeable forge object

Create:

```text
Assets/App/Prefabs/ForgeBlock.prefab
```

Initial properties:

- Approximate scale: 15 cm
- Collider enabled
- Rigidbody enabled
- Gravity initially disabled
- Grabbable using the installed Meta XR Interaction SDK architecture

We will inspect a current Building Block or installed package sample before configuring grab components.

Acceptance check:

- The prefab has no missing references.
- It can be grabbed, moved, released, and grabbed again.
- It uses the same input/interaction architecture as the scene.

### Step 12: Implement placement

Add a small placement controller with serialized references. It will:

1. Read the right controller's origin and forward direction.
2. Use a configurable maximum distance, initially eight meters.
3. Check environment-raycast support and readiness.
4. Update the preview only for a valid hit.
5. Place exactly one forge object per trigger press.
6. Reject placement when there is no valid hit.
7. Apply the documented surface-alignment policy.
8. Use MRUK world locking or a per-object spatial anchor according to the installed SDK's supported strategy.
9. Log state changes, not every frame.

Acceptance check:

- One trigger press creates one object.
- No object appears when the preview is invalid.
- The object remains visually stable during ordinary head movement.
- Placement validation and orientation helpers have EditMode tests.

## Phase 6 - Test in layers

### Step 13: Editor checks

Verify:

- Zero compilation errors.
- Exactly one XR camera rig.
- All serialized references assigned.
- Main scene enabled in the build profile.
- Required Quest project settings satisfied.
- EditMode tests pass.

Editor Play Mode proves scene and script health; it does not prove live depth placement.

### Step 14: Meta XR Simulator checks on this Mac

Use Meta XR Simulator to test:

- Head and controller poses
- Controller ray direction
- Trigger-edge handling
- Preview show/hide state
- Spawn count
- Grab/release flow
- Exceptions and scene wiring

Simulator success is logic evidence, not proof of physical-room depth behavior.

### Step 15: Physical Quest 3 acceptance test

Build:

```text
Builds/Quest/VibeForgeAR-dev.apk
```

The MVP is accepted only when:

- The app launches into color passthrough.
- Scene/passthrough permissions behave correctly.
- The right controller tracks.
- The preview appears on visible real surfaces.
- The preview disappears on invalid rays.
- Each trigger press creates exactly one object.
- The object lands on the intended surface.
- The object does not visibly drift during normal movement.
- The object can be grabbed and released.
- Device logs contain no unhandled exception.
- We explicitly understand that objects do not persist after relaunch yet.

## Phase 7 - Make builds and debugging repeatable

### Step 16: Add an automated Quest development build

Create an editor-only build entry point and a macOS shell wrapper:

```text
Assets/App/Editor/QuestBuild.cs
Tools/build-quest.sh
```

The script will:

- Invoke the pinned Unity 6.3 editor.
- Run in batch mode.
- Build enabled production scenes.
- Output `Builds/Quest/VibeForgeAR-dev.apk`.
- Write `Artifacts/quest-build.log`.
- Return a nonzero exit code on failure.
- Never change XR provider or package versions.

Acceptance check:

- A clean terminal command produces the APK or a clearly failing exit code.
- The build log identifies the Unity version, output path, and first actionable error.

### Step 17: Capture Quest logs

Use `adb logcat` to save a reproduction log under `Artifacts/`. We will analyze build logs, device logs, the Unity Editor log, and the current Git diff together.

Findings will be separated into:

- C# or Unity exceptions
- Android permissions
- OpenXR initialization
- Meta XR/MRUK readiness
- Missing scene or prefab references
- Performance symptoms
- Unrelated warning noise

## Phase 8 - Review and checkpoint the MVP

### Step 18: Review the complete change

Review for:

- Unity lifecycle mistakes
- Per-frame allocations
- Null scene references
- Incorrect installed-SDK assumptions
- Editor-only APIs leaking into runtime assemblies
- Accidental scene YAML or package changes
- Permission/privacy mistakes
- Spatial-anchor lifecycle mistakes

Then run:

```bash
git status
git diff --stat
git diff
```

Commit only after the compile, test, build, and device evidence is recorded.

## What comes after the first playable

Add features in this order:

1. Placement fit checks and collision rejection.
2. Semantic understanding of floors, walls, tables, and other room surfaces.
3. Persistent anchors across sessions.
4. Real-world depth occlusion.
5. Hand tracking with controller fallback.
6. A real forge gameplay loop: select, place, combine, transform, and save creations.
7. Optional passthrough-camera computer vision or AI features, with explicit privacy design.
8. Shared anchors or multiplayer only after single-user coordinates are stable.
9. Standalone performance profiling and production polish.

## Milestone map

| Milestone | Proof required |
| --- | --- |
| M0 - Repository foundation | Clean Git state, ignores, durable instructions |
| M1 - Toolchain | Unity 6.3 Android modules installed, Quest visible to ADB |
| M2 - Quest shell | Empty passthrough APK launches on device |
| M3 - Spatial preview | Valid real surfaces drive the placement marker |
| M4 - Place and grab | Forge object places once, stays stable, and is grabbable |
| M5 - Engineering loop | Tests, repeatable APK build, logs, and reviewed diff |
| M6 - Product loop | First creative forge mechanic built on the stable MR base |

## Immediate next action

**M0 - Repository foundation is complete.** The Unity/macOS `.gitignore`, durable `AGENTS.md`, canonical research source, build guide, and AppleDouble protections are in place.

Our next bounded implementation slice is **M1 - Toolchain**:

1. Install Unity 6.3 LTS alongside the existing Unity 6.5 editor.
2. Include Android Build Support, Android SDK and NDK Tools, and OpenJDK.
3. Confirm the Unity-bundled `adb` path.
4. Connect the developer-mode Quest 3 and verify it reports status `device`.
5. Create the Universal 3D Unity project only after those checks pass.

## Primary references

- [Meta: Set up Unity for AI-powered Meta Quest development](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/)
- [Meta: Set up Unity for Quest development](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/)
- [Meta: Get started with Meta XR Simulator](https://developers.meta.com/horizon/documentation/unity/xrsim-getting-started/)
- [Unity: Get started with Unity MCP](https://docs.unity3d.com/Packages/com.unity.ai.assistant%402.0/manual/unity-mcp-get-started.html)
- [OpenAI: Codex CLI](https://developers.openai.com/codex/cli)
