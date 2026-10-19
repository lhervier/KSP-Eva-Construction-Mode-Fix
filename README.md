# EVA Construction Mode Ground Fix

A Kerbal Space Program mod that stops an engineer in EVA Construction Mode from moving a part into
the ground. Nothing in the game refuses it, and the damage only shows up on the next reload, when KSP
tries to put the vessel back above the surface.

It works on its own, and is best paired with
[TerrainPrecisionFix](https://github.com/lhervier/KSP-TerrainPrecisionFix): see
[Works best with TerrainPrecisionFix](#works-best-with-terrainprecisionfix).

EVA Construction Mode has other stock bugs, which this mod does not fix (yet): parts that come back
slightly moved or turned after a reload, and parts that pass through the ground after a change of tool.
They are listed, with the test planned for each, in [Work in progress](#work-in-progress).

## The problem

KSP's EVA Construction Mode lets a kerbal move a part anywhere — including *below* the surface.
Nothing in the game refuses that placement, and nothing warns about it. The damage only shows up on
the next reload, when the game tries to put the vessel back above the ground.

Here is the whole sequence, on Minmus.

### 1. An engineer deploys a ground anchor

A Stamp-O-Tron ground anchor is dropped on the surface, and becomes the root of the future structure.

![A ground anchor deployed on the ground](docs/00-Ground-Anchor.png)

### 2. Parts are attached to it in EVA Construction Mode

A first truss segment goes on the anchor…

![First truss segment attached to the anchor](docs/10-First-Part.png)

…then a second one…

![Second truss segment attached](docs/20-Second-Part.png)

…then a third one.

![Third truss segment attached](docs/30-Third-Part.png)

### 3. A part is grabbed again, rotated and moved

The last segment is picked up and repositioned with the translation/rotation gizmo.

![The segment is rotated and moved with the gizmo](docs/40-Rotate-And-Place.png)

### 4. …and ends up in the ground

Nothing stops the segment from sinking into the surface: it is dropped there, half buried, and the
game accepts it as a perfectly valid placement.

![The segment is moved into the ground](docs/50-Move-Into-The-Ground.png)

### 5. Save, reload — and the structure is ruined

What happens next depends on whether [KSPCommunityFixes](https://github.com/KSPCommunityFixes/KSPCommunityFixes)
is installed, but neither outcome is the one you built.

**With KSPCommunityFixes.** The buried segment is no longer in the ground, but the whole structure
has been distorted to get it out: the trusses are twisted and splayed apart, and the assembly no
longer has the shape it was given.

![After reload, with KSPCommunityFixes: the structure is distorted](docs/60-Save-And-Reload-KSPCommunityFixes.png)

**In stock.** The structure keeps its shape, but the game raises *everything* until no part is
underground — the ground anchor included. The anchor, which was bolted to the surface, now floats in
mid-air, and the whole structure hangs several meters above Minmus.

![After reload, in stock: the whole structure floats above the ground](docs/60-Save-And-Reload-Stock.png)

## The fix

This mod attacks the problem at the source: rather than repairing the vessel on reload, it makes the
invalid placement impossible in the first place. Step 4 above simply cannot happen — the part refuses
to go below the surface, so the reloaded structure is exactly the one that was built.

While EVA Construction Mode is on, the mod watches every part being moved and **truncates** the
requested move: the part travels up to the real ground contact, and stops there.

## Technical details

- Every collider of the moved part is tested against the scenery with Unity's physics system
  (`OverlapBox` / `OverlapCapsule` / `OverlapSphere`, then `ComputePenetration` to discard the
  overlaps that are not real penetrations).
- Box, capsule, sphere and mesh colliders are all supported.
- The move is truncated rather than refused, so a part always ends up in contact with the ground
  instead of anywhere above it.

## Works best with TerrainPrecisionFix

This mod stops a part at the ground it touches while it is being placed. It does that on its own,
with or without any other mod. But two stock behaviours, which have nothing to do with placing parts,
can still move what you built at the next load, and this mod cannot do anything about them.
[TerrainPrecisionFix](https://github.com/lhervier/KSP-TerrainPrecisionFix) fixes both.

**The ground does not come back at the same height.** KSP builds the ground again at every load, and
every time it lands at a slightly different height. A part this mod stopped right on the surface can
then come back slightly buried or slightly above it, and a large base can be strained at the first
physics frame. TerrainPrecisionFix rebuilds the ground at the very same height every time, so a part
stays where it was placed.

→ [Why the moving ground matters](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/README.md#why-the-moving-ground-matters)

**An anchored base is lifted at load.** KSP raises a vessel built on a ground anchor when it loads it,
and the anchor, riveted to the ground, holds it there: the anchor's spikes come out of the ground, and
a base can end up hanging in the air. This happens however carefully the parts were placed.
TerrainPrecisionFix loads an anchored base where it was saved.

→ [The ground anchor](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/README.md#the-ground-anchor)

Earlier versions of this mod kept anchored bases in place themselves, with a *Keep anchored bases in
place* setting. That job is now TerrainPrecisionFix's, and the setting is gone.

## Work in progress

Players report two more problems with EVA Construction Mode, and they have nothing to do with the
ground being uneven or moving:

- a part moved or rotated by an engineer comes back **slightly moved or slightly turned** after a
  reload;
- a part placed against the ground (the "legs" of a girder structure) **does not hold on the ground**: it
  passes through it as soon as the structure presses on it, until the game is saved and reloaded.

Reading the game's code for EVA Construction Mode (KSP 1.12) points to the stock bugs below. **None of
them has been measured in game yet**: each one is a suspicion, given with the test that should confirm or
rule it out. This mod does not fix any of them today. All tests are played in a sandbox save, by hand,
first **without this mod**, since they are about stock behaviour, then with it, to check that it changes
nothing.

### 1. A part switched between the Rotate and Move tools passes through the ground

**What the player sees.** A part is selected with the Rotate tool, then moved with the Move tool
without being deselected in between, or the other way round. Once released, it no longer collides with
anything outside its own vessel: legs placed this way let the structure sink into the ground. After a save
and a reload, the part collides again.

**What the code suggests.** While a part is being edited, its colliders are turned into triggers so that
it can be moved through other parts. `EVAConstructionModeEditor.FetchColliders` records the current
`isTrigger` of each collider as its "original" state, and `ToggleCollidersTrigger` puts that state back:

```csharp
// EVAConstructionModeEditor.ToggleCollidersTrigger (KSP 1.12)
selectedPartColliders[i].collider.isTrigger = toggle | selectedPartColliders[i].isTrigger;
```

Turning on the Rotate or the Move gizmo calls `FetchColliders` and then turns the triggers on; turning a
gizmo off only detaches it. Switching tools with the part still selected therefore records the colliders
*while they are triggers*, and every later restore leaves them as triggers. A reload rebuilds the part,
which clears it.

**Test.**
1. On a landed vessel, attach a girder close to the ground.
2. Rotate tool: select the girder, rotate it a little.
3. Move tool, **without deselecting**: move it down until it touches the ground.
4. Back to the Place tool, then load the structure (attach a few more girders on top of it).
5. Expected if the suspicion is right: the girder sinks through the ground. Control run: same steps, but
   deselect the girder between steps 2 and 3; it should hold.

### 2. An edited part comes back slightly moved or turned after a reload

**What the player sees.** A part attached, moved or rotated in EVA Construction Mode is, after a save and
a reload, a little off from where it was left, relative to the part it is attached to. The longer and
softer the structure, the larger the error, and retouching the same part several times can make it worse.

**What the code suggests.** A save does not store the parts as they are on screen. It stores, for each
part, a pose relative to the root part (`orgPos` / `orgRot`), and a reload rebuilds the vessel from
those poses. At the end of a move or rotation, and when a part is attached, the game refreshes that pose
**for the edited part only**, from the live positions:

```csharp
// Part.UpdateOrgPosAndRot (KSP 1.12)
orgPos = newRoot.partTransform.InverseTransformPoint(partTransform.position);
orgRot = Quaternion.Inverse(newRoot.partTransform.rotation) * partTransform.rotation;
```

In flight, the joints between the root and the edited part's parent are bent by gravity and by the
ground. That bend gets written into the edited part's pose, while every other part keeps its unbent one.
On reload, the structure comes back straight and the edited part comes back offset by the bend it had at
that moment.

**Tests.**
1. *Without reloading.* On a long, flexible landed structure (girders sticking out), move or rotate a part
   at the far end, deselect it, then time-warp once (on rails) and come back. Going on rails puts every
   part back on its stored pose: if the edited part jumps relative to its parent, the suspicion is
   confirmed.
2. *Accumulation.* Same part: rotate it and bring it back to where it was (a "no-op" rotation), save,
   reload; repeat a few times. If the suspicion is right, the part drifts a little further each time.
3. *Attaching.* Attach a part at the far end of the same structure, save, reload: it should come back a
   little off its attach node.

### 3. A released part drops slightly before its joint is created

**What the player sees.** When a part is deselected after a move or rotation, it gives a small jerk
downwards, and the position it ends up in is not the one saved.

**What the code suggests.** `EVAConstructionModeEditor.ResetToPhysicalPart` gives the part a new
rigidbody, waits one frame, and only then creates its attach joint. During that frame, gravity acts on a
part that is attached to nothing. The saved pose was taken before that fall.

**Test.** Attach a heavy part at the end of a girder on a landed vessel, move it, deselect it, and watch for
a small drop; then save, reload, and compare its position with the one before the save.

### 4. Snapping in *Absolute* mode rounds against the world axes, not the vessel's

**What the player sees.** With snapping on and the gizmo in *Absolute* mode, the part turns (up to half a
snap step, on every axis, including the ones not being dragged) or shifts the moment a handle is
clicked, before any drag.

**What the code suggests.** In flight, the *Absolute* gizmo is aligned with the vessel's root part, but
`GizmoRotate.SnapToGrid` and `GizmoOffset.SnapToGrid` round the part's rotation or position against the
axes of the game world, which have nothing to do with the vessel's orientation:

```csharp
// GizmoRotate.SnapToGrid (KSP 1.12)
Vector3 eulerAngles = host.transform.rotation.eulerAngles;
host.transform.rotation = Quaternion.Euler(Mathf.Round(eulerAngles.x / SnapDegrees) * SnapDegrees, …);
```

In *Local* mode, snapping applies to the angle or distance being added, and the part does not jump.

**Test.** On a vessel that is not aligned with the world axes (any vessel landed away from the equator, or
turned), turn snapping on, set the gizmo to *Absolute*, click a handle without dragging: if the part turns or
moves, the suspicion is confirmed. Control run: the same click in *Local* mode.

### Smaller points, to check later

- **A part dragged while its parent moves** keeps its drag anchored in world space: if the vessel wobbles
  or rotates during the drag, the part's position relative to its parent drifts by as much.
- **Struts and fuel lines** (`CompoundPart`) record their target again at every move or rotation of *any*
  part of the vessel, bends included. A strut end could come back slightly off after a reload.
- **Changing tool or closing EVA Construction Mode in the middle of a drag** could leave the part where it
  was dragged on screen while the save keeps its previous pose.

### What this mod could do about it

The first two bugs could be worked around from here: restore the colliders before
`FetchColliders` records them, and compute the edited part's stored pose from its parent's stored pose
rather than from the live, bent structure. Nothing is decided until the tests above have been played.

## Settings

The mod adds a button to the application launcher (in flight and at the space center) that opens a
small settings window with a single setting, the **log level**: how much the mod writes to `KSP.log`.

It is a setting of the *installation*, not of a save: it is stored in
`GameData/EvaConstructionModeFixMod/PluginData/settings.cfg` and applied as soon as it is changed. Deleting that
file restores the default.

Earlier versions also had a *Ground offset* setting, to stop a part some distance above the ground.
It is gone: a part now always stops right at the contact.

## Installation

1. Download the latest release
2. Extract the contents to your KSP GameData folder
3. The fix activates when you enter EVA Construction Mode

## Requirements

- Kerbal Space Program 1.12 or later
- Breaking Ground DLC (for EVA Construction Mode)

## License

This mod is released under the MIT License. See the LICENSE file for details.
