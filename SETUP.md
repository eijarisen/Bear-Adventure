# Bear Adventure — local setup

This repository is arranged as a solution root with the Godot project inside `game/`.

## Required software

- Git
- Visual Studio Code
- .NET SDK 8 or newer
- Godot 4.7.2 .NET / Mono build
- VS Code extension: `ms-dotnettools.csharp`

## First run

From Git Bash in the repository root:

```bash
dotnet --version
dotnet restore BearAdventure.sln
dotnet build BearAdventure.sln
```

Open the Godot project by importing:

```text
game/project.godot
```

Press F6/F5 in Godot. The starter scene should show **Bear Adventure** and
**Godot C# scaffold is running.**

## VS Code debugging

The included `.vscode/launch.json` expects the environment variable `GODOT4`
to point to the Godot .NET executable.

Example for the current Git Bash session:

```bash
export GODOT4="/c/Tools/Godot/Godot_v4.7.2-stable_mono_win64.exe"
code .
```

Adjust that path to wherever you installed Godot.

To keep the variable for future Git Bash sessions, add the export line to
`~/.bashrc`, then restart the shell.

In VS Code:

1. Open the repository root with `code .`.
2. Install the recommended C# extension if prompted.
3. Open **Run and Debug**.
4. Choose **Bear Adventure (Godot)**.
5. Start debugging.

## Godot editor setting

In Godot set:

```text
Editor -> Editor Settings -> Dotnet -> Editor -> External Editor
```

to **Visual Studio Code**.

## Useful commands

```bash
# Build everything in the solution
dotnet build BearAdventure.sln

# Clean generated .NET output
dotnet clean BearAdventure.sln

# Verify the scaffold from Git Bash
bash scripts/verify-setup.sh

# Inspect repository changes
git status
```

## Repository layout

```text
Bear-Adventure/
├── game/
│   ├── project.godot
│   ├── BearAdventure.csproj
│   ├── scenes/
│   │   └── Main.tscn
│   └── scripts/
│       └── Main.cs
├── scripts/
│   └── verify-setup.sh
├── .vscode/
│   ├── extensions.json
│   ├── launch.json
│   ├── settings.json
│   └── tasks.json
├── BearAdventure.sln
├── .editorconfig
├── .gitattributes
├── .gitignore
└── SETUP.md
```

The future `src/` domain/persistence projects and `tests/` project are
intentionally not created yet. They should be added when the architecture
scaffold is introduced, rather than as empty projects.
