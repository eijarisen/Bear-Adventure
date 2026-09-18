#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Bear Adventure setup check"
echo "Repository: $ROOT"
echo

required_files=(
  "$ROOT/BearAdventure.sln"
  "$ROOT/game/project.godot"
  "$ROOT/game/BearAdventure.csproj"
  "$ROOT/game/scenes/Main.tscn"
  "$ROOT/game/scripts/Main.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "ERROR: Missing $file"
    exit 1
  fi
done

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet was not found in PATH."
  exit 1
fi

echo "dotnet: $(dotnet --version)"

if [[ -n "${GODOT4:-}" ]]; then
  echo "GODOT4: $GODOT4"
  if [[ ! -f "$GODOT4" && ! -x "$GODOT4" ]]; then
    echo "WARNING: GODOT4 is set, but the executable was not found at that path."
  fi
else
  echo "WARNING: GODOT4 is not set. VS Code launch/debug will need it."
fi

echo
echo "Restoring and building solution..."
dotnet restore "$ROOT/BearAdventure.sln"
dotnet build "$ROOT/BearAdventure.sln" --no-restore

echo
echo "Setup check passed."
