# ReSolved — 3D Third-Person Maze Game

## Project Overview

ReSolved is a 3D third-person maze game built in Unity 2022.3.48. The player navigates a maze environment, collecting powerups and avoiding enemies. This submission focuses on the design and implementation of a NavMesh-based enemy AI system built incrementally over a 6-week development period.

This submission targets the role of **Narrative Technical Designer**. The enemy AI system was designed with player experience and tension as the primary concern — treating technical implementation as a tool for gameplay feel and narrative atmosphere rather than an end in itself. The unpredictability of the area patrol mode, the realism of line-of-sight detection, and the visual clarity of enemy state feedback are all decisions driven by how the player experiences the maze, not just what was technically possible.

---

## New Feature: Patrolling Enemy AI System

### What was built

A multi-state enemy AI system built around Unity's NavMesh navigation. The system is contained within a single dedicated script — `PatrollingEnemyAI.cs` — and supports the following behaviour:

- **Waypoint patrol** — enemies navigate a defined route between manually placed waypoints
- **Area patrol** — enemies wander randomly within a defined zone using `NavMesh.SamplePosition`
- **Player detection** — a three-gate detection system: distance check → angle/FOV check → raycast line-of-sight
- **Sound-based detection** — a secondary omnidirectional hearing radius triggers chase even from behind
- **Chase and return** — enemies pursue the player and return to patrol after a timeout if line-of-sight is lost
- **Visual feedback** — per-enemy material colour change and particle alert system on chase state
- **Enemy type differentiation** — Wanderer and Guardian enemy types use a runtime material recolouring system. Both types share the same base mesh, with Wanderer enemies using a crimson/orange palette and Guardian enemies using blue/violet. This was a deliberate design decision over using separate models, keeping the prefab system clean and scalable.
- **Speed tuning** — independent patrol and chase speeds configurable per enemy in the Inspector

### Why this feature

The enemy AI system directly supports the core gameplay loop of ReSolved — the tension of navigating a maze under threat. Predictable, static enemies undermine that tension. The goal was to create AI that feels genuinely present and reactive: enemies that can be avoided through careful movement and use of cover, not just memorised patterns.

The area patrol mode in particular introduces unpredictability without requiring complex scripting — enemies appear to search rather than execute a fixed loop, which creates meaningful player decisions about when to move and when to wait.

---

## Code Architecture

### Single Responsibility Principle

The AI system was designed around the Single Responsibility Principle, a core software engineering practice covered during the course. Each script has one clearly defined job:

| Script | Responsibility |
|--------|---------------|
| `PatrollingEnemyAI.cs` | All AI state and behaviour logic |
| `DetectCollisions.cs` | Player contact, life deduction, enemy destruction |
| `GameManager.cs` | Game state, score, win/lose conditions |

These scripts communicate where necessary but are not dependent on each other's internal logic. `PatrollingEnemyAI.cs` does not manage lives — it simply exists in the scene. `DetectCollisions.cs` handles what happens when contact is made. This separation means either system can be modified without breaking the other — which is exactly what the Single Responsibility Principle is designed to achieve.

### State Machine Design

The AI uses an explicit finite state machine with four states: `Patrolling`, `Waiting`, `Chasing`, and `Returning`. Using an enum-based state machine rather than boolean flags makes the logic readable, debuggable, and extensible. Each state is self-contained — adding a new state does not require changes to existing state logic.

### Detection System — Three Gates

Detection is structured as three sequential checks, ordered cheapest-first to minimise performance cost:

1. **Distance** — simple arithmetic, runs every frame, most enemies bail here
2. **Angle** — dot product check against `viewAngle`, eliminates out-of-cone cases before physics
3. **Raycast** — physics engine query, only reached when both previous gates pass

This means the expensive raycast only fires when genuinely needed, making the system scalable to multiple simultaneous enemies without performance impact.

---

## Project Structure

```
Assets/
├── Animation/          — animator controllers
├── Asset Packs/        — third party and external assets
│   ├── Course Library/ — Unity Learn sample assets
│   ├── FastMesh/       — FastMesh asset pack
│   └── SyntyStudios/   — Synty Studios environment assets
├── Audio/              — sound effects and music
├── Characters/         — player and enemy models (created in Blender)
├── Environment/        — level geometry and props
├── Images/             — UI images and sprites
├── Materials/          — materials and shaders
├── Particles/          — particle system assets
├── Prefabs/            — enemy, powerup, and prop prefabs
├── Rendering/          — URP render pipeline assets and settings
├── Scenes/             — game scenes
├── Scripts/            — all C# scripts
├── Textures/           — texture assets
└── TextMesh Pro/       — UI font assets
```

---

## GitHub History

Development followed a one-task-one-commit approach throughout. The `ai_refactor` branch contains the full incremental commit history for the AI feature (Feb 6 – Mar 13 2026), with weekly merges into `main` reflecting the staged development process.

---

## References

Unity Technologies. *Finite State Machines* [online learning tutorial]. Unity Learn, 2021. Available at: https://learn.unity.com/course/finite-state-machines-1

> This tutorial was used as an introductory reference for understanding finite state machine concepts in Unity. All implementation in this project was developed independently. The state machine architecture, detection system, area patrol logic, visual feedback system, and enemy type differentiation were designed and written from scratch to suit the existing codebase, naming conventions, and gameplay requirements of ReSolved. No code from the reference tutorial appears in the final scripts.
