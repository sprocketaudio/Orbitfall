# Orbitfall Plan

## Project Vision

Orbitfall is a single-mod Oxygen Not Included: Spaced Out project built as a scenario-overhaul-style experience. The player begins in a damaged ship or ship-like habitat with finite reserves, buys time through triage and repairs, prepares for descent, then builds hostile-world industrial capability on a planet that never fully becomes safe.

## Design Pillars

- Mission-failure fantasy first: opening tone is salvage and survival, not smooth colony bootstrap.
- Finite reserve pressure: early play is driven by depletion and prioritization.
- Restore, then extend: player progress comes from repairing broken routes and building limited extensions.
- Short replayable intro burst: strong first minutes without long mandatory scripting.
- World-driven hostility: preserve vanilla machine readability and apply pressure through scenario and world conditions.
- Phased identity: early, transition, mid, and late game should feel distinct but coherent.
- Prototype before commitment: validate high-risk assumptions, especially ship-start feasibility, before deep implementation.

## Phase Breakdown

### Phase 1: Damaged Ship or Ship-Like Habitat Survival

#### Starting Model

- Player starts with finite oxygen, finite water, ration-based food, and limited power capability.
- No full sustainable life support exists at start.
- Player is not simply operating existing weak systems.
- The ship contains reserves plus broken infrastructure, and the player must restore routes and build extensions.

#### Representative Phase 1 Pattern (Directional, Not Locked)

- Oxygen tank holds finite oxygen reserve.
- Vents and pipe routes include broken or disconnected segments.
- No initial oxygen generation exists.
- Player restores oxygen distribution first.
- Player then builds oxygen generation that consumes finite water, requires power, and adds heat pressure.
- Other systems (power, water, heat) should follow similar restore-then-extend patterns where appropriate.

This pattern represents intended design style and system coupling, not a final fixed implementation.

#### Opening Flow

- First minutes should be short and legible.
- One obvious immediate problem anchors early decisions.
- Quick first stabilisation follows.
- Priorities branch after stabilisation into repair, extension, and risk-reward choices.
- This opening should typically resolve its first critical problem within a few minutes, not form a long fixed sequence.
- Avoid a long mandatory repair checklist on every replay.

#### Repair Pattern

- Reserves are consumed while functionality is restored.
- Restoring one system should create pressure on another.
- Player-built stopgaps extend survival but do not grant permanent stability.
- Research and build options should open meaningful extensions without collapsing pressure too early.
- Multiple viable extension paths should exist so the player is not forced into a single optimal solution.

#### Pressure Hierarchy (Current Direction)

- Oxygen is likely the clearest immediate urgency driver.
- Water and power are likely the strongest gates on survival extension depth.
- Heat is present in Phase 1, then becomes the defining environmental identity later.

This hierarchy is a design direction for prototyping and tuning, not final fixed values.

#### Replayability, Optional Content, and Internal Gating

- Intro should be short and replayable.
- Repair geometry or pressure composition should vary enough to avoid rote play.
- Variation should affect execution, not clarity of the core problem.
- One optional sealed room is a strong current direction.
- Optional room reward should be meaningful.
- Optional room hazards can include heat, vacuum, harmful gas, leaks, or radiation.
- Optional room should be valuable but never immediately mandatory.
- Blocked sections and visible resources behind barriers are good progression tools.
- Some content can be gated behind research or stronger digging.
- Critical path should remain readable and fast, not hard-locked behind deeper gating.

### Transition: Descent Readiness and Commit

- Gate model is hard capability gate plus soft readiness layer.
- Hard capability gate represents the ship being physically capable of descent at all.
- Soft readiness layer determines likely survivability after descent.
- Once descent is possible, player judgment should matter.
- Pressure to move on should come from worsening conditions and tradeoffs, not a single hard timer alone.

### Mid Game: Hostile Beachhead

- Mid game is a beachhead phase, not a normal base transition.
- Initial foothold should feel harsh and limited.
- Ship remains relevant as fallback, staging, and support for a while.
- Ship-to-foothold dependency should taper over time rather than disappear immediately.
- Player should expand carefully under pressure rather than normalize quickly into standard ONI colony flow.

### Late Game: Managed Hostile-World Industry

- Planet should never become fully tame or fully safe.
- Player can engineer around hostility and contain it, but not erase it.
- End state should feel like managed industrial success under ongoing hostility, not comfort-state colony stabilisation.

## Player Experience Goals and Tensions

- Triage now versus resilience later.
- Tight space versus essential system growth.
- Reserve conservation versus extension investment.
- Optional high-risk gains versus survival consistency.
- Descent timing judgment under pressure.
- Persistent world hostility versus growing player mastery.

## Current Hazard Direction

Heat is the first concrete implementation path because it is familiar, systemically broad, and phase-relevant.

- Preserve vanilla machine readability.
- Do not change how vanilla machines work as the primary difficulty tool.
- Apply pressure through ongoing environmental heat injection or similar world-driven methods.
- Future hazard variety is possible later, but first implementation should fully validate heat direction before expansion.

## Win Condition Direction

Win direction should not be normal escape progression.

- Directional target is sustained production, exploitation, megastructure, or system-level success under hostile conditions.
- End condition should reward durable operation under pressure, not simple stockpile-only framing.
- Victory should require sustained operation over time, not a single completion event.
- Final win implementation remains intentionally open during prototype stage.

## What This Project Is Not

- Not a total rewrite of vanilla machine behavior.
- Not a same-map ship-in-the-sky simulation requirement.
- Not a normal ONI start with only harsher numbers.

## Prototype Priorities

1. Validate oversized damaged ship or ship-like habitat starting environment.
2. Validate Phase 1 opening flow and replayability.
3. Validate finite reserves plus player-built extension survival model.
4. Validate hard capability gate plus soft readiness layer for descent.
5. Validate hostile beachhead handoff with ship still relevant.
6. Validate persistent world-driven heat pressure.

## Major Risks

- Phase 1 becomes too linear if only one obvious survival path exists.
- Optional content becomes effectively mandatory if reward-risk tuning is off.
- Larger ship start space reduces tension if too much interior is freely usable.
- Opening sequence becomes repetitive on replay.
- Ship-start scale causes pathing or performance issues.
- Descent gate tuning collapses into trivial pass or hard wall.
- Mid game normalizes too quickly into standard colony flow.
- Heat pressure becomes ignorable or oppressive without strategic depth.
- Win condition remains vague too long and weakens late-game direction.

## Open Questions Summary

Detailed tracker: [docs/spec/OPEN_QUESTIONS.md](docs/spec/OPEN_QUESTIONS.md)

Highest-impact unknowns:

- Exact starting reserve values and early repair puzzle pattern.
- Exact ship size envelope and fixed versus destructible structure split.
- Exact degree of intro randomness and replay variation.
- Whether one optional room is sufficient final scope or only initial scope.
- Exact hard capability gate representation and soft readiness factors.
- How clearly readiness should be surfaced before descent.
- Final sustained-production hostile-world mastery win implementation.
