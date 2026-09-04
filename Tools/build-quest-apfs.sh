#!/bin/bash
# Quest development build via an APFS disk image.
#   Tools/build-quest-apfs.sh
# The repo lives on an exFAT volume where Unity's parallel artifact-DB
# writes fail ("attempt to write a readonly database"), so builds run in
# a 60 GB APFS sparsebundle (VFBuild.sparsebundle) kept at the volume root:
# source is synced in, the APK is built there, then copied back to
# Builds/Quest/VibeForgeAR-dev.apk. Exits nonzero on build failure.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VOLUME_ROOT="$(dirname "$REPO_ROOT")"
IMAGE="$VOLUME_ROOT/VFBuild.sparsebundle"
MNT="/Volumes/VFBuild"
WORK="$MNT/vf"
APK="Builds/Quest/VibeForgeAR-dev.apk"

export COPYFILE_DISABLE=1

if [ ! -d "$MNT" ]; then
  hdiutil attach "$IMAGE" >/dev/null
fi

mkdir -p "$WORK"
cd "$REPO_ROOT"
tar -cf - \
  --exclude=.git --exclude=Library --exclude=Builds --exclude=Artifacts \
  --exclude=Temp --exclude=Logs --exclude=UserSettings \
  --exclude='My project' --exclude='VR Terminal' \
  --exclude='*.csproj' --exclude='*.sln' . \
  | (cd "$WORK" && tar -xf -)
find "$WORK" -name "._*" -delete

bash "$WORK/Tools/build-quest.sh"

cp "$WORK/$APK" "$REPO_ROOT/$APK"
echo "apk: $REPO_ROOT/$APK"
