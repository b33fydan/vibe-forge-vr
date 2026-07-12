VR Game Learning with Codex

# **Building a Meta Quest 3 mixed-reality app with Unity and GPT‑5.6 Sol in Codex**

*Research snapshot: July 10, 2026\.*

## **What “Codex 5.6 Sol” means**

The current product name is **GPT‑5.6 Sol**, selected as the model inside Codex. From a project directory, the direct CLI command is:

codex \-m gpt-5.6-sol

OpenAI positions Sol as the strongest GPT‑5.6 model for complex coding, computer use, research, and cybersecurity. The normal default uses medium reasoning; higher reasoning is worth using for architecture, dependency problems, and difficult Unity build failures. Ultra mode uses multiple subagents and is more appropriate for broad audits than ordinary script changes. ([OpenAI Developers](https://developers.openai.com/codex/models))

For Unity development, the most effective arrangement is:

* **Codex CLI or IDE extension** working against the local Unity repository.  
* **Unity itself** compiling, testing, and building the project.  
* **Optional Unity MCP integration** allowing Codex to inspect and operate the Unity Editor.  
* **ADB, Meta Horizon Link, and a physical Quest 3** providing the final test evidence.

Codex can inspect files, modify the repository, run installed tools, review diffs, and invoke repeatable scripts. The CLI includes `/init`, `/permissions`, `/model`, `/status`, and `/review`. ([OpenAI Developers](https://developers.openai.com/codex/cli))

---

# **Recommended technical stack**

| Layer | Recommended choice |
| ----- | ----- |
| Unity | **Unity 6.3 LTS**, specifically a current `6000.3.x` patch |
| Render pipeline | Universal Render Pipeline through the **Universal 3D** template |
| XR provider | **Unity OpenXR Plugin** |
| Meta foundation | Meta XR Core SDK |
| Interactions | Meta XR Interaction SDK |
| Mixed-reality utilities | Meta XR MR Utility Kit, or MRUK |
| Editor automation | Unity MCP plus Meta XR MCP Extension, optional |
| Codex model | GPT‑5.6 Sol, medium for routine work and high for difficult planning/debugging |
| Initial target | Quest 3 standalone Android APK |

Unity considers newer Update releases suitable for new projects, but Meta’s current AI-assisted Quest workflow explicitly targets Unity 6.3 `6000.3.x+` with Android Build Support and recommends URP. For this guide, 6.3 LTS reduces compatibility variables while following Meta’s tested workflow. Unity 6.3 LTS is supported through December 2027\. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/))

Meta currently recommends Unity OpenXR for new Quest projects and identifies the older Oculus XR Plugin as deprecated and scheduled for removal. Meta XR Core SDK 203.0 was current when this research was conducted and requires at least Unity `6000.0.66f2`, so Unity 6.3 is comfortably within its supported range. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/))

---

# **The sample application: RoomDrop**

Rather than asking Codex to “make a mixed-reality app,” give it one small, verifiable vertical slice.

**RoomDrop** will:

1. Start in color passthrough.  
2. Track the Quest 3 Touch controllers.  
3. Cast a ray from the right controller into the real environment.  
4. Display a placement indicator on a detected surface.  
5. Place a virtual cube when the right index trigger is pressed.  
6. Let the user grab and move that cube.  
7. Keep the cube spatially stable for the current session.  
8. Build and run as a standalone Quest APK.

That single slice exercises passthrough, tracking, input, MRUK environment raycasting, spatial stability, interaction, Android deployment, and device logging. More ambitious features can then be added without debugging the entire universe at once—a traditional Unity pastime best avoided.

---

# **Step-by-step guide**

## **Step 1: Prepare the development machine**

Install **Unity Hub**, then install a current Unity 6.3 LTS patch with these modules:

* Android Build Support  
* Android SDK and NDK Tools  
* OpenJDK

Meta’s current setup guide requires all three Android components. Create the project from the **Universal 3D** template. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/))

### **Windows versus macOS**

Both Windows and macOS can build Quest applications, but **Windows is the more complete Quest development environment** because Meta Horizon Link is currently Windows-only. Link lets Unity enter Play Mode directly on the headset and materially shortens the iteration loop. macOS developers can use Meta XR Simulator and standalone APK builds instead. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-link/))

For the smoothest workflow, use:

* Windows 11  
* A USB-C data cable  
* Meta Quest Developer Hub  
* Meta Horizon Link  
* Git  
* Node.js 18 or later  
* Codex CLI

Node 18+ is part of Meta’s current AI-assisted Unity workflow and is also useful for installing command-line tooling through npm. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/))

---

## **Step 2: Put the Quest 3 into developer mode**

You need a Meta developer team, a verified developer account, developer mode enabled, and—on Windows—the Oculus ADB drivers.

The condensed sequence is:

1. Create or join a developer team in the Meta developer dashboard.  
2. Complete account verification.  
3. Open the Meta Horizon mobile app.  
4. Select the paired headset.  
5. Open **Headset Settings → Developer Mode**.  
6. Turn developer mode on.  
7. Connect the headset using USB-C.  
8. Put on the headset and accept the USB debugging prompt.  
9. Select **Always allow from this computer**.  
10. Install the Oculus ADB driver on Windows.

These are prerequisites for directly installing and debugging your own APKs. ([Meta for Developers](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/))

Verify ADB connectivity:

adb devices

Expected result:

List of devices attached  
1WMHHxxxxxxxxx    device

If it says `unauthorized`, put on the headset and accept the debugging prompt. If nothing appears, check the cable, ADB driver, developer mode, and whether the headset is awake.

---

## **Step 3: Create the Unity project**

In Unity Hub:

1. Select a Unity 6.3 LTS editor.  
2. Choose **Universal 3D**.  
3. Name the project `QuestRoomDrop`.  
4. Create the project.  
5. Wait for package import and shader compilation to finish.

Create this directory structure:

Assets/  
└── App/  
    ├── Art/  
    ├── Editor/  
    ├── Materials/  
    ├── Prefabs/  
    ├── Scenes/  
    ├── Scripts/  
    │   ├── Interaction/  
    │   ├── Placement/  
    │   └── Runtime/  
    └── Tests/  
        └── EditMode/  
Tools/  
Artifacts/  
Builds/

Save the current scene as:

Assets/App/Scenes/MainMR.unity

---

## **Step 4: Put the project under Git before involving Codex**

Use a standard Unity `.gitignore`. At minimum, exclude:

\[Ll\]ibrary/  
\[Tt\]emp/  
\[Oo\]bj/  
\[Ll\]ogs/  
UserSettings/  
Builds/  
Artifacts/  
MemoryCaptures/  
.vs/  
.idea/

Commit these directories and files:

Assets/  
Packages/  
ProjectSettings/  
\*.meta

Do not ignore `.meta` files. Unity uses them to preserve asset identifiers and references.

Initialize the repository:

cd C:\\Dev\\QuestRoomDrop

git init  
git add .  
git commit \-m "Create Unity 6.3 Quest MR project"

Codex should begin from a clean commit. OpenAI explicitly recommends Git checkpoints before and after agent tasks so changes can be reviewed and reverted. ([OpenAI Developers](https://developers.openai.com/codex/cli))

---

## **Step 5: Install and launch Codex**

Using npm:

npm install \-g @openai/codex

Then start Codex from the Unity project root:

cd C:\\Dev\\QuestRoomDrop  
codex \-m gpt-5.6-sol

Sign in when prompted.

Inside Codex, check the session:

/status  
/model  
/permissions

Recommended operating posture:

* **Model:** GPT‑5.6 Sol  
* **Reasoning:** Medium for ordinary C\# changes  
* **Reasoning:** High for architecture, package conflicts, XR initialization, and build failures  
* **Permissions:** Workspace write with approval for sensitive or external commands  
* **Git:** Always start from a clean working tree

Avoid broad, unrestricted execution for a Unity project. Build scripts, package installation, and Android deployment can have consequences outside a single source file.

---

## **Step 6: Give Codex durable project instructions**

Run:

/init

Codex creates an `AGENTS.md` file. Replace or augment it with the following:

\# QuestRoomDrop Agent Instructions

\#\# Mission

Build a standalone mixed-reality application for Meta Quest 3\.

The first vertical slice must:  
1\. Launch into passthrough.  
2\. Track the right Touch controller.  
3\. Raycast against the physical environment.  
4\. Preview a valid placement point.  
5\. Place a grabbable object on trigger press.  
6\. Keep the placed object spatially stable during the session.  
7\. Build successfully as an Android Quest APK.

\#\# Fixed technical stack

\- Unity 6.3 LTS / 6000.3.x  
\- Universal Render Pipeline  
\- Unity OpenXR provider  
\- Meta XR Core SDK  
\- Meta XR Interaction SDK  
\- Meta XR MR Utility Kit  
\- Quest 3 standalone Android target

Packages/manifest.json and Packages/packages-lock.json are the source of  
truth for exact installed package versions.

\#\# Repository rules

\- Runtime code belongs under Assets/App/Scripts.  
\- Editor-only code belongs under Assets/App/Editor.  
\- Tests belong under Assets/App/Tests.  
\- Do not edit Library, Temp, Logs, obj, UserSettings, Builds, or Artifacts.  
\- Never delete or regenerate .meta files casually.  
\- Do not upgrade Unity or package versions without explaining the reason,  
  compatibility risk, and rollback path.  
\- Inspect the installed package source and assembly definitions before  
  using Meta XR or MRUK APIs. Do not guess API names from memory.  
\- Do not hand-edit Unity scene or prefab YAML unless explicitly instructed.  
  Prefer Unity MCP or an idempotent Unity Editor setup script.  
\- Editor code must not leak UnityEditor references into runtime assemblies.  
\- Avoid allocations, LINQ, repeated GetComponent calls, and verbose logging  
  in Update, FixedUpdate, or LateUpdate.  
\- Avoid introducing a new framework when the installed SDK already provides  
  the capability.

\#\# Change discipline

Before implementing:  
1\. Inspect the repository.  
2\. Confirm installed package versions.  
3\. State the files and scene objects that will change.  
4\. Propose the smallest vertical slice.  
5\. Identify device-only assumptions.

After implementing:  
1\. Report every changed file.  
2\. Report compilation and test evidence.  
3\. Report build evidence.  
4\. List remaining checks that require a physical Quest 3\.

\#\# Definition of done

A task is complete only when:  
\- Unity has zero compilation errors.  
\- New pure logic has EditMode tests where practical.  
\- Scene and prefab references are assigned.  
\- The Quest development build succeeds.  
\- No unrelated package, render pipeline, XR provider, or project setting changed.  
\- The Git diff has been reviewed.

`AGENTS.md` is important because it gives Codex persistent repository-level rules instead of making you restate platform constraints in every prompt. ([OpenAI Developers](https://developers.openai.com/codex/learn/best-practices))

---

## **Step 7: Configure the project for Meta Quest and OpenXR**

In Unity:

1. Open **File → Build Profiles**.  
2. Select **Meta Quest**.  
3. Choose **Enable Platform** or **Switch Platform**.  
4. Install `com.unity.xr.openxr` if Unity prompts you.  
5. Open **Edit → Project Settings → XR Plug-in Management**.  
6. Verify OpenXR is enabled for the Meta Quest/Android target.  
7. Under the Android OpenXR settings, enable the Meta XR feature group.

Meta recommends OpenXR for new projects and says all new projects should use it rather than the deprecated Oculus XR Plugin. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/))

### **Install Meta packages**

Install these packages through Unity Package Manager or the official Meta XR package flow:

1. **Meta XR Core SDK**  
2. **Meta XR Interaction SDK**  
3. **Meta XR MR Utility Kit**

MRUK’s package name is:

com.meta.xr.mrutilitykit

After Unity resolves the packages, commit:

Packages/manifest.json  
Packages/packages-lock.json

Do not leave dependency entries set to `latest` in a serious project. Allow Unity to resolve the chosen version, then pin that resolved version. AI agents and floating package versions are a remarkably efficient way to manufacture mysteries.

You do **not** need Meta XR Platform SDK merely to build this local MR prototype. Add it later only when you need platform identity, entitlement, social features, achievements, purchases, or cloud storage. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/))

### **Run Meta’s Project Setup Tool**

Open:

Meta → Tools → Project Setup Tool

Select the Android target, then run:

1. **Fix All**  
2. **Apply All**

Meta’s tool resolves required and recommended Quest settings, including several OpenXR and Android configuration issues. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/))

Commit the clean configuration:

git add .  
git commit \-m "Configure OpenXR and Meta XR packages"

---

## **Step 8: Make an empty device build before adding application logic**

This isolates toolchain failures from application failures.

1. Connect the Quest 3\.  
2. Open **File → Build Profiles**.  
3. Verify **Meta Quest** is active.  
4. Add `MainMR.unity` to the scene list.  
5. Select the Quest under **Run Device**.  
6. Enable **Development Build**.  
7. Select **Build and Run**.  
8. Save the APK as:

Builds/Quest/QuestRoomDrop-empty.apk

Quest standalone builds are Android `.apk` files. Meta recommends Development Build for debugging, but it should be disabled for final performance testing. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-build/))

Do not proceed until this empty build installs and launches. Otherwise, later errors become impossible to classify cleanly.

---

# **Optional: Give Codex direct access to the Unity Editor**

## **Step 9: Install Unity MCP**

This is the highest-leverage part of the workflow, but it is also the least mature. Unity currently documents Unity MCP through the pre-release `com.unity.ai.assistant` package. Treat it as an optional productivity layer, not a dependency your project must have to compile.

Unity MCP can expose the scene hierarchy, components, Play Mode, editor state, and object modification tools to an MCP-compatible coding agent. Meta’s AI-assisted Quest documentation says the same workflow works with Codex and other coding agents. ([Unity Docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant%402.0/manual/unity-mcp-get-started.html))

Install Unity’s AI Assistant/MCP package, then open:

Edit → Project Settings → AI → Unity MCP

Verify that **Unity Bridge** shows **Running**.

Unity installs the MCP relay at:

Windows:  
%USERPROFILE%\\.unity\\relay\\relay\_win.exe

macOS Apple Silicon:  
\~/.unity/relay/relay\_mac\_arm64.app/Contents/MacOS/relay\_mac\_arm64

The relay must be launched with `--mcp`. ([Unity Docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant%402.0/manual/unity-mcp-get-started.html))

### **Connect Codex on Windows**

Exit the active Codex session and run:

codex mcp add unity-mcp \-- "$env:USERPROFILE\\.unity\\relay\\relay\_win.exe" \--mcp

Restart Codex:

codex \-m gpt-5.6-sol

Then check:

/mcp

Unity should show a pending MCP connection. Open:

Edit → Project Settings → AI → Unity MCP

Review it and select **Accept**.

### **Install Meta’s XR-specific MCP extension**

In Unity Package Manager:

1. Select **\+**.  
2. Select **Install package from git URL**.  
3. Enter:

https://github.com/meta-quest/Unity-MCP-Extensions.git

Meta says this extension adds Quest-specific tools for OVR components, Meta XR SDK integration, and Quest build settings. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/))

Restart Unity and Codex after installation.

### **Verify without allowing modifications yet**

Give Codex this prompt:

Use Unity MCP in read-only mode.

Inspect the open Unity project and report:  
1\. The current scene name.  
2\. The root scene objects.  
3\. The active build target.  
4\. The configured XR provider.  
5\. Installed Meta XR and MRUK package versions.  
6\. Current Unity console errors and warnings.

Do not modify any files, scene objects, settings, or packages.

If MCP fails, continue without it. Codex can still create C\# files, tests, editor scripts, build scripts, and analyze logs. Meta explicitly documents manual code generation as the fallback when Unity MCP is unavailable. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-ai-vr-setup/))

---

# **Build the mixed-reality scene**

## **Step 10: Add passthrough and tracking**

Open:

Meta XR Tools → Building Blocks

Search for **Passthrough** and add it to the current scene.

Meta’s Passthrough Building Block adds both:

* A Meta XR Camera Rig  
* The passthrough configuration

Delete the ordinary Unity `Main Camera` if it remains. Do not leave two active camera rigs in the scene. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-passthrough-tutorial-with-blocks/))

Then add:

* **Controller Tracking**  
* The appropriate Interaction SDK controller/ray blocks for your installed version

Building Blocks are intended to install their dependencies and apply required project configuration automatically. The exact interaction block names can change between Meta SDK releases, so Codex should inspect what is actually installed rather than inventing component names from an older tutorial. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/bb-overview/))

---

## **Step 11: Configure MRUK and permissions**

Create an empty GameObject named:

MRSystems

Add:

EnvironmentRaycastManager

To follow Meta’s MRUK scene setup:

1. Select the `OVRCameraRig`.  
2. Find `OVRManager`.  
3. Set **Scene Support** to **Required**.  
4. Enable the **Scene** permission under startup permission requests.  
5. Set **Passthrough Support** to **Required**.  
6. Enable the passthrough permission.  
7. Run **Meta → Tools → Update AndroidManifest.xml**.  
8. Run the Project Setup Tool again.

Meta’s MRUK setup documentation specifies the camera rig, Scene Support, Scene permission, passthrough support, and manifest update for an MRUK scene. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs/))

### **Why use environment raycasting?**

On Quest 3 and Quest 3S, `EnvironmentRaycastManager` can raycast against live environmental depth. Its API provides:

* `Raycast`  
* `PlaceBox`  
* `CheckBox`  
* `IsSupported`

Ray hits only succeed inside the headset depth camera’s current frustum. A failed hit does not necessarily mean your code is broken; the ray may be outside the depth view, the environment data may not be ready, or the feature may not be supported by the current runtime. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-environment-raycast/))

---

## **Step 12: Create the placeable object**

Create a cube and turn it into:

Assets/App/Prefabs/PlaceableCube.prefab

Recommended initial properties:

Scale: 0.15, 0.15, 0.15  
BoxCollider: enabled  
Rigidbody: enabled  
Mass: 0.25  
Use Gravity: initially false

Using the installed Interaction SDK, make the prefab grabbable. Prefer the current SDK’s grab-related Building Block or documented prefab configuration.

Do not ask Codex to guess which six Interaction SDK components belong on the prefab. Give it access to the installed assemblies and require it to inspect an included sample or Building Block first.

---

# **Use Codex to implement the vertical slice**

## **Step 13: Start with an audit and plan**

Set Sol to higher reasoning for this first architectural task, then give it the following prompt:

We are implementing the first Quest 3 mixed-reality vertical slice.

Before editing anything:

1\. Read AGENTS.md.  
2\. Inspect Packages/manifest.json and Packages/packages-lock.json.  
3\. Inspect the installed Meta XR Core, Interaction SDK, and MRUK source or  
   assembly metadata.  
4\. Use Unity MCP, when available, to inspect the current scene hierarchy,  
   OVRManager configuration, EnvironmentRaycastManager, controller anchors,  
   and PlaceableCube prefab.  
5\. Report any compilation errors before making changes.  
6\. Produce a plan only.

Goal:  
\- Show a placement preview where a ray from the right controller hits the  
  live physical environment.  
\- Place PlaceableCube when the right index trigger is pressed.  
\- Make the placed cube grabbable.  
\- Keep it spatially stable for the current session.

Constraints:  
\- Quest 3 standalone Android target.  
\- Unity OpenXR provider.  
\- Use the installed APIs as the source of truth.  
\- Do not upgrade packages.  
\- Do not hand-edit .unity or .prefab YAML.  
\- Do not create a second camera rig or input framework.  
\- Avoid per-frame allocations and repeated component searches.  
\- Keep the change limited to the placement vertical slice.

The plan must include:  
\- Files to create or modify.  
\- Scene references required.  
\- Input API selected and why.  
\- Environment-raycast readiness and failure handling.  
\- Spatial-anchor or MRUK world-lock strategy.  
\- Testable pure logic.  
\- Editor, Link, and standalone device verification.

Review its plan before allowing implementation. Pay particular attention to:

* Whether it found the correct right-controller anchor.  
* Whether it identified the installed input API correctly.  
* Whether it understands the difference between MRUK world locking and per-object spatial anchors.  
* Whether it plans to handle unsupported or not-ready environment raycasts.  
* Whether it proposes a preview indicator rather than blindly spawning objects.

Then say:

Implement only the placement vertical slice from the approved plan.

After editing:  
1\. Allow Unity to compile.  
2\. Read all new console errors.  
3\. Fix only errors caused by this change.  
4\. Run the relevant EditMode tests.  
5\. Report changed files, test evidence, and unresolved device-only checks.

---

## **Step 14: Require these implementation behaviors**

The completed placement controller should have the following behavior:

1. References are serialized and assigned in the Inspector.  
2. The ray originates from the right controller anchor.  
3. The ray’s direction is the controller’s forward direction.  
4. `EnvironmentRaycastManager.IsSupported` is checked.  
5. A configurable maximum ray distance is used, such as eight meters.  
6. A preview marker is shown only for a valid hit.  
7. The marker is hidden when the result is unsupported, not ready, occluded, outside the depth frustum, or otherwise invalid.  
8. Trigger input places one object on the valid hit.  
9. The object orientation follows an explicit surface-alignment policy.  
10. An `OVRSpatialAnchor` is added when MRUK world locking is not active.  
11. No object is spawned on an invalid placement.  
12. Logs are emitted on state transitions, not every frame.  
13. Pure orientation and placement-validation logic is covered by EditMode tests.

Meta’s reference workflow similarly casts from the right controller, instantiates at the environment hit, and adds an `OVRSpatialAnchor` when MRUK world locking is not active. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-environment-raycast/))

A useful follow-up prompt is:

Use Unity MCP to inspect the PlaceableCube prefab and the installed  
Interaction SDK examples.

Configure the prefab to be grabbable using the installed SDK version.  
Do not guess type names. Reuse the same interaction architecture already  
present in the scene. Afterward, enter Play Mode, inspect the Unity console,  
and report which checks still require a physical Quest headset.

---

# **Test in progressively more realistic environments**

## **Step 15: Test in the Unity Editor**

First verify:

* No compilation errors.  
* Exactly one XR camera rig.  
* `EnvironmentRaycastManager` is present.  
* Right-controller reference is assigned.  
* Preview prefab is assigned.  
* Placeable prefab is assigned.  
* Main scene is included in the build.  
* The Project Setup Tool reports no required Android fixes.

Do not interpret “Unity entered Play Mode” as proof that an MR feature works. It merely proves that Unity survived entering Play Mode, an achievement of modest but nonzero value.

---

## **Step 16: Test with Meta XR Simulator**

Meta XR Simulator is a standalone OpenXR runtime for testing Quest head, controller, and hand input on the development computer. It supports Windows and Apple Silicon macOS. It can be activated from Unity through:

Meta → Meta XR Simulator → Activate

It can simulate input from the keyboard, mouse, Xbox controller, or forwarded Quest controllers. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/xrsim-getting-started/))

Use the simulator to verify:

* Scene startup  
* Camera pose  
* Controller rays  
* Trigger input  
* Preview visibility logic  
* Grabbing  
* Exceptions  
* UI behavior

Treat simulator results as logic and interaction validation, not proof that live physical depth placement behaves correctly.

---

## **Step 17: Test through Meta Horizon Link**

On Windows:

1. Install and launch Meta Horizon Link.  
2. Set Meta Horizon Link as the active OpenXR runtime.  
3. Enable Developer Runtime Features.  
4. Enable passthrough over Link.  
5. Restart Unity after changing Link feature settings.  
6. Start Quest Link in the headset.  
7. Press Play in Unity.

Link removes most APK build time from the interaction loop. However, Meta warns that visual appearance and performance can differ from standalone Quest execution, and final behavior must be checked on-device. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-link/))

---

## **Step 18: Build and run on the Quest 3**

Create a development build:

Builds/Quest/QuestRoomDrop-dev.apk

Acceptance checklist:

* The application launches without a black screen.  
* The passthrough view is visible.  
* Scene and passthrough permissions are requested appropriately.  
* The right controller is tracked.  
* A placement preview appears on visible real surfaces.  
* The preview disappears for invalid rays.  
* Trigger press creates exactly one cube.  
* The cube is positioned on the intended surface.  
* The cube does not visibly drift during normal movement.  
* The cube can be grabbed and released.  
* No unhandled exception appears in device logs.  
* Relaunch behavior is understood: objects are not expected to persist yet.

Only after this passes should you add persistent anchors, semantic room understanding, hand tracking, occlusion, or multiplayer.

---

# **Automate Unity builds so Codex can verify its work**

## **Step 19: Have Codex create a build entry point**

Give Codex:

Create a repeatable Quest development-build workflow.

Add:  
\- Assets/App/Editor/QuestBuild.cs  
\- Tools/build-quest.ps1

Requirements:  
\- Use BuildPipeline.BuildPlayer.  
\- Include enabled production scenes.  
\- Produce Builds/Quest/QuestRoomDrop-dev.apk.  
\- Enable development-build options only in the development method.  
\- Fail with a nonzero process exit code when the build fails.  
\- Print a concise build summary.  
\- Do not change the active XR provider or package versions.  
\- Use an active Meta Quest build profile when the project has one;  
  otherwise build the Android target explicitly.  
\- Keep UnityEditor references entirely inside the Editor assembly.

Also document the required UNITY\_EDITOR environment variable.

Unity supports command-line builds through `-batchmode`, `-projectPath`, `-buildTarget` or `-activeBuildProfile`, `-executeMethod`, and `-logFile`. Unity recommends specifying the target or build profile explicitly rather than relying on whichever target the Editor last used. ([Unity Docs](https://docs.unity3d.com/6000.4/Documentation/Manual/build-command-line.html))

A typical PowerShell invocation will resemble:

$env:UNITY\_EDITOR \=  
    "C:\\Program Files\\Unity\\Hub\\Editor\\6000.3.XXf1\\Editor\\Unity.exe"

& $env:UNITY\_EDITOR \`  
    \-batchmode \`  
    \-quit \`  
    \-projectPath (Resolve-Path .) \`  
    \-buildTarget Android \`  
    \-executeMethod QuestBuild.BuildDevelopment \`  
    \-logFile "Artifacts\\quest-build.log"

if ($LASTEXITCODE \-ne 0\) {  
    throw "Quest build failed with exit code $LASTEXITCODE"  
}

Replace `6000.3.XXf1` with the installed editor version. If Codex finds a saved Meta Quest build profile, have it use `-activeBuildProfile` instead.

After running the build, ask:

Read Artifacts/quest-build.log.

Report:  
1\. Whether the build succeeded.  
2\. The APK output path and size.  
3\. All compiler errors.  
4\. All package or OpenXR warnings.  
5\. The first actionable cause of failure, when unsuccessful.

Do not propose package upgrades until you have identified the root cause.

---

# **Use Codex as a debugging partner**

## **Step 20: Capture Unity and Quest logs**

The Windows Unity Editor log is located at:

%LOCALAPPDATA%\\Unity\\Editor\\Editor.log

On macOS:

\~/Library/Logs/Unity/Editor.log

Unity also supports a custom `-logFile` location for automated runs. Android player logs are obtained through `adb logcat`. ([Unity Docs](https://docs.unity3d.com/6000.5/Documentation/Manual/log-files.html))

For a Quest failure:

New-Item \-ItemType Directory \-Force Artifacts | Out-Null

adb devices  
adb logcat \-c  
adb logcat \-v threadtime |  
    Tee-Object \-FilePath Artifacts\\quest-logcat.txt

Reproduce the problem, then press `Ctrl+C`.

Give Codex:

Analyze:  
\- Artifacts/quest-logcat.txt  
\- Artifacts/quest-build.log  
\- the Unity Editor log  
\- the current Git diff

Separate the findings into:  
1\. C\# or Unity exceptions.  
2\. Android permission failures.  
3\. OpenXR initialization failures.  
4\. Meta XR or MRUK readiness failures.  
5\. Missing scene or prefab references.  
6\. Performance symptoms.  
7\. Warnings that are unrelated noise.

For every conclusion, cite the exact log lines.  
Identify the smallest likely root cause and propose one change at a time.  
Do not upgrade packages as a first response.

For visual failures such as a pink material, misplaced UI, incorrect controller orientation, or a black passthrough layer, give Codex a screenshot using its image-input capability and include the corresponding logs. Codex supports image context through `codex --image`. ([OpenAI Developers](https://developers.openai.com/codex/cli))

---

## **Step 21: Review every agent change**

After a successful build:

/review

Ask the review to focus on:

Review the uncommitted changes for:

\- Unity lifecycle mistakes  
\- per-frame allocations  
\- null scene references  
\- incorrect Meta XR or MRUK API assumptions  
\- Android-only failures hidden by Editor testing  
\- accidental UnityEditor references in runtime code  
\- direct serialized-scene modifications  
\- permission and privacy problems  
\- spatial-anchor lifecycle mistakes  
\- package or ProjectSettings changes outside the task

Do not modify the working tree during review.  
Rank findings by likely user impact.

Then inspect:

git status  
git diff \--stat  
git diff

Commit each verified milestone separately:

git add .  
git commit \-m "Add passthrough Quest scene"

git add .  
git commit \-m "Add MRUK environment placement"

git add .  
git commit \-m "Add grabbable placed objects"

git add .  
git commit \-m "Add automated Quest development build"

This makes regressions bisectable and gives Codex clean rollback points.

---

# **Recommended feature sequence after the MVP**

Once RoomDrop passes on a physical Quest 3, add capabilities in this order:

## **1\. Placement quality**

Use MRUK’s `PlaceBox` and `CheckBox` to ensure an object fits on the selected surface and does not intersect physical geometry. This is more reliable than accepting every ray hit. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-environment-raycast/))

## **2\. Semantic scene understanding**

Use MRUK room and anchor data to distinguish floors, walls, ceilings, tables, couches, doors, and other meaningful surfaces. Keep semantic placement separate from live-depth placement so either system can fail gracefully.

## **3\. Persistent spatial anchors**

Save anchor identifiers and reload them in later sessions. Add persistence only after session-level anchoring works; otherwise you will be debugging persistence and tracking simultaneously.

## **4\. Real-world occlusion**

Add depth-based occlusion so virtual objects can appear behind real objects. Profile it on the standalone headset, not only through Link.

## **5\. Hand tracking**

Add hands through Interaction SDK after controller interaction is solid. Keep controller input available as a fallback during development.

## **6\. Colocation and shared spaces**

Only after single-user anchors and coordinate systems are stable should you add shared anchors or multiplayer synchronization.

## **7\. Production performance**

Profile CPU, GPU, memory, shader variants, draw calls, allocations, thermal behavior, and the configured display refresh target directly on the Quest.

---

# **Current caveats worth knowing**

### **Unity MCP is optional beta infrastructure**

The currently documented Unity MCP package is pre-release. It can save substantial time by letting Codex inspect scenes and configure GameObjects, but the project should remain buildable without it. Prefer reproducible C\# editor utilities for critical scene configuration. ([Unity Docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant%402.0/manual/unity-mcp-get-started.html))

### **Environment raycasting has a real camera-frustum limitation**

A controller ray outside the Quest’s depth camera frustum cannot produce an environment hit. Your UI should communicate “no valid surface” rather than treating every missed raycast as an error. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-environment-raycast/))

### **Link is not the standalone headset**

Link is excellent for rapid iteration, but performance and some feature behavior differ from the final Android application. Device builds remain mandatory. ([Meta for Developers](https://developers.meta.com/horizon/documentation/unity/unity-link/))

### **Meta XR Core SDK 203.0 has a Link-specific passthrough issue**

As of the research date, Meta lists a known issue in Core SDK 203.0 where `passthroughLayerResumed` does not fire when running through Meta Horizon Link; Meta says a fix is planned for v204. If that specific callback fails only under Link, verify it in a standalone build before rewriting working application code. ([Meta for Developers](https://developers.meta.com/horizon/downloads/package/meta-xr-core-sdk/))

### **Package upgrades should be isolated changes**

Never combine “upgrade Meta XR,” “upgrade Unity,” and “rewrite interaction logic” in one Codex task. Those are three separate failure domains wearing a trench coat.

---

# **The most reliable execution sequence**

Use these as your first five milestones:

1. **Toolchain proof:** empty Unity project installs and opens on Quest 3\.  
2. **MR proof:** passthrough and controller tracking work.  
3. **Spatial proof:** the environment raycast preview follows real surfaces.  
4. **Interaction proof:** a placed object can be grabbed and remains spatially stable.  
5. **Engineering proof:** Codex can run tests, build an APK, analyze logs, and review the Git diff.

That sequencing is more important than the size of the model. GPT‑5.6 Sol can write a large portion of the implementation, but it performs best when every task has explicit boundaries, installed-package context, a measurable definition of done, and real evidence from Unity or the headset.

