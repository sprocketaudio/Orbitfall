# Technical Notes

## Baseline Assumptions

- Target game is Oxygen Not Included with Spaced Out DLC required.
- Project scope is a single scenario-overhaul-style mod.
- The project intends to preserve vanilla machine readability.
- The design assumes players will rely on familiar vanilla systems, so clarity must be preserved.
- Preferred difficulty source is world pressure, not machine rewrites.
- Initial concrete implementation path is one primary hazard model: heat.

## Scope Boundaries

- Same-map ship-above-planet simulation is not required.
- The fantasy can be delivered with a separate damaged ship or ship-like habitat start environment.
- Avoid early commitment to engine-hostile mechanics before feasibility validation.

## Key Validation Targets

1. Oversized damaged ship or ship-like habitat start feasibility
   - Prototype larger envelopes first, likely 64x64 or similar, then adjust.
   - Validate play feel as strongly as performance and pathing.
   - Confirm constrained tension can survive larger layout footprints.
   - Ensure space constraints still force trade-offs even in larger layouts.
   - Ensure that increasing size does not remove meaningful spatial constraints or decision pressure.
2. Structural model
   - Validate hybrid fixed/destructible structure approach.
   - Treat full non-destructible interiors as likely too restrictive unless proven otherwise.
3. Phase 1 pressure model
   - Validate finite reserves plus player-built extension loop.
   - Confirm restore/reconnect gameplay remains legible and replayable.
   - Ensure multiple viable extension paths exist to avoid a single optimal solution.
4. Descent gate model
   - Prototype hard capability gate plus soft readiness layer.
   - Preserve a real player judgment point after descent becomes possible.
5. Hostile foothold handoff
   - Validate that beachhead phase is harsh but not dead-end.
   - Keep ship relevant as fallback, staging, and support during early foothold.
6. Persistent hazard delivery
   - First path is environmental heat pressure.
   - Deliver pressure through world behavior, not vanilla machine rule changes.

## Design-to-Implementation Guidance

- Build prototype slices that answer specific risk questions.
- Prioritize tunability and scenario-authoring controls.
- Treat balance iteration as core delivery work, not polish.
- Avoid broad scaffolding that assumes unresolved outcomes.

## Likely Constraints

- Large starting spaces can cause pathing and performance stress.
- Large starting spaces can also reduce tension if too much interior is freely usable.
- Phase 1 can become too linear if only one survival path is viable.
- Phase 1 can become repetitive if intro variation is too weak.
- Intro sequence may become trivial if initial problem is solved too quickly without follow-up pressure.
- Scenario-specific setup can create tooling or pipeline friction.
- Multi-phase tuning burden is likely substantial.

## Immediate Prototype Order

1. Prototype oversized damaged ship or ship-like habitat start space and validate feel.
2. Prototype short replayable opening stabilisation burst with branching decisions.
3. Prototype finite-reserve-to-extension survival loop.
4. Prototype hard capability gate plus soft readiness layer for descent.
5. Prototype hostile beachhead handoff with ship still relevant.
6. Prototype persistent world-driven heat pressure.

## Do Not Assume Yet

- Final ship size.
- Final fixed versus destructible structure split.
- Final win-condition implementation.
- Final multi-hazard scope.
