# Orbitfall

Orbitfall is an Oxygen Not Included mod for Spaced Out where the player starts in a damaged ship or ship-like habitat with finite reserves, buys time by restoring critical functionality, builds constrained extensions to survive longer, prepares for descent, and then pushes toward hostile-world industrial success on a planet that never fully becomes safe.

This is not a general content mod. It is intended as a scenario-overhaul-style experience.

## Requirements

- Oxygen Not Included
- Spaced Out DLC (required)

## Design Status

- Early design and prototype planning phase.
- Phase 1 concept is substantially defined at high level.
- Heat is the first hazard path.
- Exact numbers and implementation details remain intentionally open.
- The opening Phase 1 loop is based on finite reserves, broken infrastructure, and player-built extensions rather than inherited systems.

The project will prototype high-risk assumptions first, especially the damaged ship or ship-like habitat start environment.

## Repository Structure

- `PLAN.md`: main vision and planning document
- `AGENTS.md`: reusable rules for AI agents and contributors
- `docs/spec/OVERVIEW.md`: concept summary and scenario arc
- `docs/spec/GAMEPLAY_LOOP.md`: phase-to-phase gameplay loop
- `docs/spec/PHASES.md`: detailed phase structure and pressure model
- `docs/spec/TECHNICAL_NOTES.md`: validation targets, boundaries, and prototype assumptions
- `docs/spec/OPEN_QUESTIONS.md`: active unresolved design and prototype questions
- `src/Orbitfall.Mod/`: buildable C# mod project and metadata (`mod.yaml`, `mod_info.yaml`)
- `assets/`: future assets and data content

## Current Prototype

- Rocket interior world size override from vanilla `32x32` to `64x64`.
- Implemented by patching `TUNING.ROCKETRY.ROCKET_INTERIOR_SIZE` before rocket interior world creation.
- New-game startup bootstrap now moves starting dupes into a standalone in-space rocket interior world.
- Printing pod flow is suppressed at startup by removing telepads and halting immigration timers.

## Build and Install

1. Build the mod:
   - `dotnet build src\Orbitfall.Mod\Orbitfall.Mod.csproj -c Release`
2. Output folder:
   - `artifacts\mod\Orbitfall\`
3. Optional install to local ONI mods folder:
   - `powershell -ExecutionPolicy Bypass -File src\Orbitfall.Mod\build.ps1 -InstallToGame`

## Developer Utilities

- Local asset extraction helper: `.tmp_extract/`
- Extractor project: `.tmp_extract/OrbitfallAssetExtract.csproj`
- Current extractor entry point: `.tmp_extract/Program.cs`
- Run extractor:
  - `dotnet run --project .tmp_extract\OrbitfallAssetExtract.csproj`
- Extracted files are written to:
  - `.tmp_extract/out/`

## Design References

- Main vision and planning: [PLAN.md](PLAN.md)
- Concept summary: [docs/spec/OVERVIEW.md](docs/spec/OVERVIEW.md)
- Phase structure: [docs/spec/PHASES.md](docs/spec/PHASES.md)
- Validation and prototype assumptions: [docs/spec/TECHNICAL_NOTES.md](docs/spec/TECHNICAL_NOTES.md)
- Active unresolved decisions: [docs/spec/OPEN_QUESTIONS.md](docs/spec/OPEN_QUESTIONS.md)

Initial development will focus on validating the ship-start experience before expanding into full scenario implementation.
