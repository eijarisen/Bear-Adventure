#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
command -v dotnet >/dev/null 2>&1 || { echo 'Install the .NET 8 SDK or a compatible newer SDK.' >&2; exit 1; }
LOG_DIR="$ROOT/game/.godot/verification"
mkdir -p "$LOG_DIR"
{
  echo 'Bear Adventure build 22-23.1'
  echo 'Project engine SDK: Godot.NET.Sdk/4.7.2 (unchanged)'
  echo "Verification time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  dotnet --info
} > "$LOG_DIR/toolchain-22-23.txt"
dotnet build BearAdventure.sln --configuration Debug
dotnet run --project tests/BearAdventure.Gameplay.Checks/BearAdventure.Gameplay.Checks.csproj --configuration Debug --no-build | tee "$LOG_DIR/gameplay-22-23.log"
dotnet run --project tests/BearAdventure.PixelArt.Checks/BearAdventure.PixelArt.Checks.csproj --configuration Debug --no-build | tee "$LOG_DIR/pixel-22-23.log"
REQUIRE_ENGINE=false
DO_EXPORT=false
for arg in "$@"; do
  [[ "$arg" == '--require-engine' ]] && REQUIRE_ENGINE=true
  [[ "$arg" == '--export-windows' ]] && DO_EXPORT=true
done
if [[ -n "${GODOT4:-}" ]]; then
  "$GODOT4" --version | tee -a "$LOG_DIR/toolchain-22-23.txt"
  "$GODOT4" --headless --path "$ROOT/game" --import > "$LOG_DIR/import-22-23.log" 2>&1
  "$GODOT4" --headless --path "$ROOT/game" --scene res://checks/RuntimeChecks.tscn --fixed-fps 60 --quit-after 3000 > "$LOG_DIR/engine-22-23.log" 2>&1
  cat "$LOG_DIR/engine-22-23.log"
  grep -q 'ENGINE CHECKS PASSED:' "$LOG_DIR/engine-22-23.log" || { echo 'Engine test did not finish successfully.' >&2; exit 1; }
  if grep -E '(^ERROR:|SCRIPT ERROR:|ENGINE CHECKS FAILED)' "$LOG_DIR/engine-22-23.log"; then
    echo 'Engine output contained an error.' >&2; exit 1
  fi
  if $DO_EXPORT; then
    mkdir -p "$ROOT/build"
    "$GODOT4" --headless --path "$ROOT/game" --export-release "Windows Desktop" "$ROOT/build/BearAdventure.exe" > "$LOG_DIR/export-22-23.log" 2>&1
    cat "$LOG_DIR/export-22-23.log"
    [[ -f "$ROOT/build/BearAdventure.exe" ]] || { echo 'Windows export did not create BearAdventure.exe. Install matching Godot export templates.' >&2; exit 1; }
  fi
else
  echo 'Build and .NET checks complete. ENGINE CHECKS NOT RUN: set GODOT4 to your Godot .NET executable.'
  $REQUIRE_ENGINE && exit 2
  $DO_EXPORT && { echo '--export-windows requires GODOT4.' >&2; exit 2; }
fi
echo "Verification logs: $LOG_DIR"
