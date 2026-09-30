#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if ! command -v dotnet >/dev/null 2>&1; then
  echo "The .NET SDK was not found in PATH." >&2
  exit 1
fi
dotnet run --project "$ROOT/tests/BearAdventure.PixelArt.Checks/BearAdventure.PixelArt.Checks.csproj"
dotnet build "$ROOT/BearAdventure.sln"
