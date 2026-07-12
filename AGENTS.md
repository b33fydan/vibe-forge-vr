# Vibe Forge AR Agent Instructions

## Mission

Build a standalone mixed-reality application for Meta Quest 3. The product name is Vibe Forge AR; Meta SDK and platform documentation may describe the same passthrough experience as mixed reality (MR).

The first vertical slice must:

1. Launch into color passthrough.
2. Track the right Touch controller.
3. Raycast against the live physical environment.
4. Preview a valid placement point.
5. Place one forge object on right-index-trigger press.
6. Make the placed object grabbable.
7. Keep it spatially stable for the current session.
8. Build successfully as a standalone Android Quest APK.

## Fixed technical baseline

- Hardware target: Meta Quest 3
- Runtime target: standalone Meta Quest Android APK
- Unity: 6.3 LTS, pinned to the selected `6000.3.x` editor patch
- Render pipeline: Universal Render Pipeline
- XR provider: Unity OpenXR Plugin
- Meta foundation: Meta XR Core SDK
- Interaction: Meta XR Interaction SDK
- Environment understanding: Meta XR MR Utility Kit (MRUK)
- Initial input: Touch controllers
- Host environment: Apple Silicon macOS
- Editor simulation: Meta XR Simulator

Do not change Unity versions, render pipelines, XR providers, or Meta package versions as part of an ordinary feature task. Any upgrade must be isolated, justified, reversible, and verified separately.

## Repository layout

- Runtime code belongs under `Assets/App/Scripts`.
- Editor-only code belongs under `Assets/App/Editor`.
- Materials belong under `Assets/App/Materials`.
- Prefabs belong under `Assets/App/Prefabs`.
- Production scenes belong under `Assets/App/Scenes`.
- Tests belong under `Assets/App/Tests`.
- Repeatable shell tooling belongs under `Tools`.
- Generated APKs belong under `Builds/Quest` and are not committed.
- Generated logs and evidence belong under `Artifacts` and are not committed.
- Research and planning Markdown may remain at the repository root until a deliberate documentation move is approved.

Commit Unity source-of-truth files, including:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- All required `.meta` files

Never commit or edit generated Unity folders such as `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `Builds`, or `Artifacts`.

## External-volume rules

This repository lives under `/Volumes/beefybackup`. AppleDouble sidecars can appear as `._*` files, including inside `.git`.

- Use `COPYFILE_DISABLE=1` for write-producing Git and package-manager commands where practical.
- Ignore and prune generated `._*` sidecars before reporting Git state.
- Do not mistake AppleDouble files for project assets.
- Preserve unrelated user work in a dirty tree.
- Stage only the files belonging to the current slice.

## Unity and SDK rules

- Treat `Packages/manifest.json` and `Packages/packages-lock.json` as the source of truth for installed package versions.
- Inspect installed package source, samples, and assembly definitions before using Meta XR, Interaction SDK, or MRUK APIs.
- Do not guess type names or component combinations from memory.
- Prefer supported Meta Building Blocks or idempotent Unity Editor setup utilities for scene configuration.
- Do not hand-edit `.unity` or `.prefab` YAML unless explicitly required and reviewed.
- Never casually delete, regenerate, or rename `.meta` files.
- Keep `UnityEditor` references entirely inside editor-only code or assemblies.
- Do not introduce a second camera rig, EventSystem, input framework, or XR provider.
- Do not add Meta XR Platform SDK until a feature actually needs identity, entitlement, social features, achievements, purchases, or cloud storage.

## Runtime engineering rules

- Use serialized references for scene and prefab dependencies.
- Validate required references and fail clearly during initialization.
- Avoid allocations, LINQ, repeated `GetComponent` calls, and verbose logging in per-frame methods.
- Log state transitions and actionable failures, not normal per-frame state.
- Treat unsupported, not-ready, and no-hit environment-raycast results as normal placement states.
- Never place an object when the placement preview is invalid.
- Keep surface-alignment and placement-validation policies explicit and testable.
- Keep controller input available while hand tracking remains experimental.
- Separate session stability from cross-session anchor persistence.

## Change discipline

Before implementing:

1. Inspect the repository and current Git state.
2. Read this file and the relevant plan or issue.
3. Confirm the Unity editor and installed package versions.
4. Report existing compilation errors before adding changes.
5. State the smallest vertical slice, files, scene objects, and device-only assumptions.

After implementing:

1. Allow Unity to compile and report console evidence.
2. Run relevant EditMode tests.
3. Run the narrowest repeatable build or validation path.
4. Report every changed file.
5. Review `git diff` for unrelated settings, package, scene, and metadata changes.
6. List checks that still require a physical Quest 3.

Do not fix unrelated failures or upgrade packages as a first response to a build error. Identify the first actionable root cause and change one failure domain at a time.

## Definition of done

A feature is complete only when:

- Unity has zero new compilation errors.
- Pure logic has EditMode tests where practical.
- Scene and prefab references are assigned and verified.
- The intended Quest development build succeeds when the toolchain is available.
- Device-only behavior is verified on a physical Quest 3 or explicitly left pending.
- No unrelated package, render pipeline, XR provider, or project setting changed.
- The Git diff has been reviewed.
- The working tree contains no generated AppleDouble sidecars.

## Current roadmap

Follow `VIBE_FORGE_AR_BUILD_GUIDE.md`. Keep the first playable bounded to passthrough, right-controller surface placement, one grabbable forge object, and session-level spatial stability.
