# Octopath Traveler Agent Guide

## Big picture
- Solution: `Octopath-Traveler.sln` on .NET 8 (`global.json` pins `8.0.0`).
- The game is split into `Octopath-Traveler-Model` (rules/state), `Octopath-Traveler-Controller` (orchestration), `Octopath-Traveler-View` (all I/O), and `Octopath-Traveler.Tests` (scripted approval tests).
- Main data flow: `Program.cs` / tests choose a team folder → `Game.Play()` → `TeamsBuilder` loads `data/<group>` JSON + a team `.txt` file → `GameState` builds `CombatFlowState` → `Game` drives turns → `MainConsoleView` renders `CombatViewSnapshot` and action results.

## Non-negotiables
- Preserve exact output text, order, spacing, and separators; tests compare scripts line by line in `Octopath-Traveler.Tests/Tests.cs`.
- Do not use `Console.WriteLine` / `Console.ReadLine` in model or controller code. Real console access lives in `Octopath-Traveler-View/ConsoleView.cs` and the manual debug harness in `Octopath-Traveler-Controller/Program.cs`.
- Keep names descriptive and methods small; prefer extracting helpers/classes over growing `Game.cs` or `MainConsoleView.cs`.

## Refactor seams to respect
- `Octopath-Traveler-Controller/Game.cs` should stay orchestration-first: round setup, traveler actions, beast actions, battle completion, and winner messaging are separate responsibilities.
- `Octopath-Traveler-Controller/TeamsBuilder.cs` handles parsing/loading/validation only; keep file-format rules and catalog loading together, but do not move combat rules here.
- `Octopath-Traveler-View/MainConsoleView.cs` is a formatting adapter: prompts, separators, and result messages only. It should not decide combat behavior.
- DTO/snapshot types such as `CombatFlow/CombatViewSnapshot.cs` are intentional; use them to pass read-only state to the view.

## Project conventions worth copying
- Validation is exception-based (`InvalidOperationException`, `ArgumentException`) rather than error codes.
- Team files are normalized by trimming blank lines; folder names are path-sensitive and come from `data/<group>` / `data/<group>-Tests`.
- Output-ready text should go through `View.WriteLine`; input should go through `View.ReadLine`.
- When an action fails, keep the current user-facing message behavior intact unless a test explicitly changes.

## Safe workflow
- Re-read `docs/clean_code_guidelines.md` before any refactor.
- Change one responsibility slice at a time, then run tests immediately.
- If enough methods can be attributed to a new class, extract it and move the methods there instead of adding more to the existing class.
- Re-read the class file after refactor to check for SRP violations, naming issues, or opportunities to extract more helpers.
- Primary validation: `dotnet test Octopath-Traveler.sln`.
- For a failing E2 script, use `Octopath-Traveler-Controller/Program.cs` with `ManualTestingView` to replay the exact test file and inspect the line-by-line mismatch.

## Useful files
- `Octopath-Traveler-Controller/Game.cs`
- `Octopath-Traveler-Controller/TeamsBuilder.cs`
- `Octopath-Traveler-View/View.cs`
- `Octopath-Traveler-View/MainConsoleView.cs`
- `Octopath-Traveler.Tests/Tests.cs`
- `Octopath-Traveler-Model/CombatFlow/CombatFlowState.cs`

