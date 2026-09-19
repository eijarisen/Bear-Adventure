#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Bear Adventure Batch 02 verification"
echo

required_files=(
  "$ROOT/game/project.godot"
  "$ROOT/game/BearAdventure.csproj"
  "$ROOT/game/scripts/Main.cs"
  "$ROOT/game/scripts/Interaction/HarvestController.cs"
  "$ROOT/game/scripts/UI/GameHud.cs"
  "$ROOT/src/BearAdventure.Domain/Gameplay/HarvestRules.cs"
  "$ROOT/src/BearAdventure.Persistence/SaveGameStore.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "ERROR: missing $file"
    exit 1
  fi
done

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet was not found in PATH."
  exit 1
fi

echo "dotnet: $(dotnet --version)"
dotnet restore "$ROOT/BearAdventure.sln"
dotnet build "$ROOT/BearAdventure.sln" --no-restore

echo
echo "Batch 02 build passed."
