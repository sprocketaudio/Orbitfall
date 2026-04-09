# AGENTS.md

## Purpose

This file defines reusable operating rules for AI coding agents and contributors.

## Required Read Order

1. `README.md`
2. `PLAN.md`
3. Relevant `docs/spec/` files

Read `README.md`, `PLAN.md`, and relevant `docs/spec/` files before proposing or implementing design or code changes.

## Working Rules

- Keep changes small, focused, and reviewable.
- Preserve repository structure and naming consistency.
- Avoid speculative implementation outside documented scope.
- Do not silently invent product or design decisions.
- Document assumptions and unknowns explicitly.
- Prefer prototype slices that validate risk over broad scaffolding that assumes outcomes.
- When refining design docs, prioritise clarity of player decisions and gameplay loops.

## Scope and Certainty Control

- Do not narrow project scope accidentally through implementation or wording without updating plan and spec docs.
- Do not convert directional design notes into final implementation language unless plan and spec docs have been deliberately updated to lock decisions.
- Distinguish clearly between confirmed direction, likely direction, and unresolved questions when writing docs.

## Documentation Sync Rules

- Keep high-level docs, phase docs, and technical notes mutually consistent when any one of them changes.
- Update docs whenever behavior, interfaces, architecture, or workflow changes.
- Keep `PLAN.md` and `docs/spec/` aligned with implementation state.
- Update `README.md` when repository structure or contributor workflow changes.

## Documentation Language

- Avoid overstating certainty.
- Use precise language that matches decision maturity.
- Mark assumptions as assumptions.

## Environment References

- Local game install reference path: `E:\Games\Steam\steamapps\common\OxygenNotIncluded`.
- When implementation or behavior details are unclear, reference existing proven working ONI mods on GitHub for patterns and validation.
- Do not assume internal game behavior without validation against real mod examples or testing.
- Store temporary decompiled reference files in `.tmp_extract/decompile/` and keep root clean of ad-hoc `.tmp_*` files.

## Implementation Discipline

- Validate assumptions against current files before editing.
- Prefer implementation paths that align with how the game naturally expects systems to be registered, spawned, linked, and updated.
- Avoid hacks or workaround patches unless absolutely necessary after native-path options have been exhausted.
- If any hack or workaround is used, report it explicitly in the change summary, including why it was needed, scope of impact, and conditions for later removal.
- Prefer practical, production-minded choices over speculative abstractions.
- Add or update tests when behavior changes and test infrastructure exists.
- Keep commits logically grouped with specific messages.
- Avoid placeholder code that implies completed functionality.

## Handoff Quality

- Summarize what changed and why.
- Call out remaining risks or unknowns.
- List follow-up tasks when work is intentionally partial.
- Reference modified files directly for fast continuation.
