# GameplayModule

## Purpose
Owns Night Shift, a two-floor stealth-action contract: movement, cover, doors, combat,
guard perception, extraction and immediate retry.

## Concepts
gameplay, run, Night Shift, stealth, guards, shooting, cover, stairwell doors, chase, extraction

## Decisions
- The frame is an internal tick sequence in GameplaySystemContext. Every step is synchronous.
- NightShiftModel owns run state; commands decide gameplay, and NightShiftView renders it.
- Input crosses the view/mediator boundary as ContractInputVO; edge inputs are consumed once.
- Everything on screen is a prefab the developer edits. The building, doors, crates, vents, stairwell doors,
  light rooms, exit, spawns, camera, rain and crosshair live in Prefabs/Level/PB_Level_NightShift;
  its LevelAuthoring reads the markers into a ContractLevelVO that StoreLevelCommand hands the model, so
  moving a marker in the prefab moves it in the game. ContractLevelVO's defaults are the fallback layout.
- Characters (PB_Player, PB_Guard with a nested PB_Revolver), bullets, effects and decals come from the
  Pool_Gameplay group, listed on GameplaySystemRoot through PoolSubContext. Keys are NightShiftPoolKeys.
- Tools/FlowIoC-Modules/Gameplay/Night Shift Assets writes all of it. Build missing never overwrites an
  existing asset, so edits survive; Rebuild all does, after asking.
- HUD belongs to GameplayScreenModule.
- Closed doors and the middle floor block both shots and vision through TraceContractFunction.
- One hit ends the run. Six guards must be eliminated before using the ground-floor exit.
- Plays like Deadbolt: a six-shot revolver loaded one shell per R press (hold R to keep loading, firing
  interrupts), click-per-shot, linked vents that hide the player completely, per-room light switches
  (dark rooms shrink guard sight to arm's length and guards walk back to turn the light on), guards
  that investigate gunshots, door kicks and bodies, kicked doors knock down whoever is behind them,
  and melee that kills an unaware, turned-away or knocked-down guard but only shoves an aware one.
- Bullets are projectiles, not hitscan: player and guard shots live in ContractRVO.Bullets and
  MoveBulletsCommand steps them each frame, so a guard's shot can be dodged by ducking. Every stop is an
  ImpactRVO, drawn for the rest of the run as a bullet hole or blood. Particles, shake and recoil are the
  view's alone; the run never reads them.
- Cover is physical, not a hidden flag: each crate is a solid slab up to ContractRVO.CoverHeight in
  TraceContractFunction, so it stops sight and bullets both ways. A crouched player is safe only from
  the side the crate faces; a guard on the near side sees and hits them, and their own crouched shots hit
  the crate. Standing up fires over it.
- Floors are joined by linked stairwell doors (StairRVO, StairDoorView), as in Deadbolt: E at one comes out
  of its partner. There are no ladders.
- A guard who has fully seen the player hunts them for ChaseMemory seconds after losing sight, walking to
  them and taking stairwell doors to change floor; a vent or the dark breaks the trail. Noise is
  investigated on any floor within its radius.
- Computers (TerminalRVO, TerminalView, PB_Computer) open a text screen on the HUD: TerminalOpened goes out,
  SaveTerminal and CloseTerminal come back through the Connector. While one is open the run is paused and
  input is ignored. Saved text lives in the model's level, so it survives a retry but not a restart of Play.
- NightShiftAssetsAutoBuild runs Build missing after every compile when something is missing or the level
  predates a fixture; it never overwrites an existing asset except a level from an earlier build.
- Weapons and characters are data. Scriptables/CD_Weapons lists every weapon (magazine, reload per round or
  per magazine, fire rate, automatic, pellets, spread, damage, bullet speed and life, noise, prefab);
  Scriptables/CD_Characters lists every character type (shape, Faction, prefab, default weapon, health,
  speeds, sight, reaction). Both are filed on GameplaySystemRoot's RootAdapter and NightShiftModel reads them
  in PostConstruct, falling back to FillDefaults when missing. Spawn markers name a type and optionally a weapon.
- The cast is shapes: the player is a square; triangle, pentagon, hexagon and diamond are enemies and the
  circle a friendly by default. Faction is the only thing that makes a character hostile - change it in
  CD_Characters. Friendlies only patrol and do not count towards clearing the building.
- A character that dies drops its weapon, loaded, as a PickupRVO. Walking over a weapon you carry (in hand
  or pocket) takes its rounds without a key. E on one you do not carry puts it in hand; the one you held goes
  to an empty pocket, or to the floor when the pocket is full. Crouching makes that pickup E's first choice
  over every other interaction. Q swaps hand and pocket. Level WeaponSpawn markers lay weapons down.
- Character and weapon prefabs are instantiated by the view from the configs (a run has a handful), not
  pooled; bullets, effects, decals and pickups are pooled.
- Play starts in the hideout (CD_Missions.Hideout, PB_Level_Hideout): a mission board lists CD_Missions'
  contracts and an upgrade bench sells CD_Upgrades. Choosing a contract sets model.Mission and restarts the
  run in its level; RequestLevelCommand asks the view to switch level prefabs before every run. Winning pays
  the reward once and goes home by itself: JudgeContractCommand dispatches Extracted, which runs the
  loading set ReturnHome (declared in LoadingModule's CD_LoadingSets) - DebriefCommand shows the payout on
  the bar, ReturnHomeCommand starts the hideout run behind it. Dying and R retries the contract. Station lists reach the HUD as
  MenuOpened(title, cash, lines) and come back as ChooseMenu(index) / CloseMenu.
- Progress (cash, finished contracts, upgrade levels) is a ProgressSVO saved as JSON in PlayerPrefs under
  "nightshift.progress" - there is no LocalSave module in this project. Upgrades apply at Reset: extra
  health, reload scale, move scale, extra spare rounds.
- The look is noir: ivory, gold and crimson on near-black with a serif face (the machine's Georgia or Times,
  else the built-in font) on the HUD, main menu and loading screen, and a URP volume (Art/Materials/VP_Noir:
  desaturated, warm, vignette, grain) added to every level by the asset builder. The ReturnHome loading set
  ends on a Continue step that WaitForKeyCommand holds until a key or mouse button; the loading screen
  pulses its message while its bar is full and the set still runs.
- Difficulty: LevelAuthoring.Difficulty (Easy/Normal/Hard/Brutal) scales every enemy's health, reaction,
  sight, fire rate, bullet speed and chase speed at Reset (NightShiftModel.ApplyDifficulty); each GuardSpawn
  adds Toughness (hits) and Aggression (pace).
- Props (PropRVO, PropView, PB_Prop_*): bullets stop in them and break them. A ceiling lamp falls and kills
  whoever is under it, a barrel explodes (kills within 2.4, opens doors, chains barrels within 2.8),
  bottles, monitors and plants burst. ResolvePropsCommand does what a broken prop does next. Every impact
  carries an ImpactKind - wall, ceiling, floor, wood, glass, metal, dirt, blood, explosion - which picks
  its burst and its mark; a shot into the ceiling also brings plaster down.
- A level with FollowCamera keeps its camera's authored size and follows the player along X.
- PB_Level_Docks ("Docks Massacre", Brutal, 60 wide, 22 enemies) is the long action level; the asset
  builder writes it and adds it to CD_Missions.
- A whole-magazine reload shows MagazineSwap over the head; a round-by-round one shows RevolverCylinder.
- Tools/FlowIoC-Modules/Gameplay/Level Editor places spawns, characters, weapons, doors, stairwell and vent
  pairs, computers, crates, light rooms and the exit by clicking in PB_Level_NightShift's prefab mode, and
  lists what the level is missing or names wrongly.
- E does one thing per press; InteractContractCommand offers stairs, vent, switch, door, loot, exit in that order.

## Known gaps
- One level prefab and two floors; the rules assume two storeys and markers are snapped to them.
- The generated art is placeholder rectangles; swap sprites inside the prefabs for production art.
- There is no general-purpose physics platform controller; floors are changed only through stairwell doors.

Update: never

<!-- FLOWIOC:BEGIN version=1 hash=e4f4c45e | generated by Tools/FlowIoC/Module Scanner - do not edit inside this block -->
**Kind** Main · **Assemblies** Modules.Gameplay, Modules.Gameplay.Signals
**Root** GameplaySystemRoot → GameplaySystemContext
**Incoming** ChooseMenu(int) · CloseMenu() · CloseTerminal() · LeaveContract() · SaveTerminal(string) · StartContract()
**Outgoing** HudChanged(string, string) · MenuClosed() · MenuOpened(string, string, String[]) · OpenHud() · ReturnedToMenu() · TerminalClosed() · TerminalOpened(string, string)
**Sub modules** GameplayScreenModule (Screen) · GameplayTestModule (Test)
<!-- FLOWIOC:END -->
