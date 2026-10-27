# GameplayModule

## Purpose
Owns Night Shift, a two-floor stealth-action contract: movement, cover, doors, combat,
guard perception, extraction and immediate retry.

## Concepts
gameplay, run, Night Shift, stealth, guards, shooting, cover, stairwell doors, chase, extraction

## Decisions
- The frame is an internal tick sequence in GameplaySystemContext. Every step is synchronous.
- Editor Play Mode QA shortcuts run as the first tick Command and are excluded from player builds:
  Ctrl+Shift+1 kills enemies, 2 kills the player, 3 wins the mission, 4 abandons it, 5 attacks an owned
  location, 6 expires an attack, 7 defends an attacked location, and 8 pays one working-location cycle.
- NightShiftModel owns run state; commands decide gameplay, and NightShiftView renders it.
- The player belongs to the Square family. Each CD_Family declares whether it is Player, Friendly or Enemy;
  only Enemy families enter the working-location raid draw. A mission's AttackingFamilies list can restrict
  that draw, while an empty list allows every Enemy family. The selected family is saved with the active raid,
  supplies the attackers' rank/name/prefab config, and owns the location if the defence fails. The shipped
  roster is Square, Triangle, Circle, Pentagon, Hexagon and Diamond; Circle starts Friendly and the other
  non-player families start Enemy. The hideout's Family Board opens its own informational panel listing every
  family and its current relation with Square. Starting a takeover against a Friendly family's location records
  that family in ProgressSVO.EnemyFamilies, announces the change, and permanently moves it into the Enemy raid pool.
  Every CD_Family also carries a Relations list of FamilyDiplomacyCVO entries, so every pair of NPC families can
  independently be Friendly or Enemy. CD_Families resolves a direct entry, then its reverse, and defaults an
  unconfigured pair to Enemy. The Family Board shows every family's friends and enemies; Square's saved hostility
  overrides its authored diplomacy without writing into a CD_ asset at runtime.
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
- The cast is shapes: the player is a tall rectangle in a trench coat and fedora (CharacterShape.Rectangle, built
  by NightShiftAssetBuilder.Hero); the square is a hooded friendly; triangle, pentagon, hexagon and diamond are enemies and the
  circle a friendly by default. Faction is the only thing that makes a character hostile - change it in
  CD_Characters. Friendlies only patrol and do not count towards clearing the building.
- A character that dies drops its weapon, loaded, as a PickupRVO. Walking over a weapon you carry (in hand
  or pocket) takes its rounds without a key. E on one you do not carry puts it in hand; the one you held goes
  to an empty pocket, or to the floor when the pocket is full. Crouching makes that pickup E's first choice
  over every other interaction. Q swaps hand and pocket. Level WeaponSpawn markers lay weapons down.
- Character and weapon prefabs are instantiated by the view from the configs (a run has a handful), not
  pooled; bullets, effects, decals and pickups are pooled.
- Play starts in the hideout (CD_Missions.Hideout, PB_Level_Hideout): a mission board lists CD_Missions'
  contracts as a bird's-eye city map and an upgrade bench sells CD_Upgrades. The map replaces the old rows with
  clickable top-down buildings and shops laid between streets; each roof shows the location, current family and
  income. Square-owned, foreign-owned, under-attack and lost properties have distinct colours. Choosing a building
  sets model.Mission and restarts the
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
- Pixel-art presentation uses Unity's official 2D Pixel Perfect package. CD_Gameplay owns the camera toggle,
  assets PPU and reference resolution; the editor setup applies those values to every level prefab camera.
  GameplayModule/Art textures import with Point filtering, no mipmaps and no compression.
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
- Music: Art/Audio/Music/Hideout and Art/Audio/Music/Contracts are listed into Scriptables/CD_Music on import
  (NightShiftMusicImporter), filed as Hideout or Contract tracks by folder. The hideout starts silent: E on
  its cassette player (StationKind.CassettePlayer, TapeDeckView, PB_CassettePlayer) opens the tape deck panel
  on the HUD (DeckShown / ControlTape / CloseDeck): play-pause where the tape is (MusicPaused), next, previous;
  the run is paused while it is open. The now-playing card is drawn as a cassette with turning reels. A contract opens on the square taking out a phone
  (ContractRVO IntroRemaining, IntroCommand): on the tap the card slides in at the HUD's bottom right, the music
  starts a beat later, and the run unpauses when the phone is away. A retry keeps the contract's track; coming
  home fades it out and a tape keeps playing. The contract track is the mission's own, else ContractTrack,
  else a random Contract track.
- Hideout life: a wardrobe (StationKind.Wardrobe) lists Scriptables/CD_Outfits - add an outfit there and it
  hangs in the wardrobe; CharacterRig.Wear dyes the rectangle's outfit slots and shows or hides hat, scarf and
  cigarette. A weapon locker (StationKind.WeaponLocker) stores weapons; ProgressSVO keeps Hand, Pocket, Locker
  and Outfit, so the run starts with what was carried and a weapon found on a job comes home. No firing and no
  crosshair at home; the gun stays holstered. The asset builder adds a "Home" child to the hideout with the
  wardrobe, locker, pendant lights, fairy lights, a neon sign, shelves and a wandering cat (CatView).
- The hideout has no words: every station carries an icon (StationView._icon - pin, wrench, hanger, bullet, note)
  that swells and glows while the player is in reach, and the prompt there is only "E". The wall neon is a
  rectangle, a heart and a square. NightShiftAssetBuilder.IconsForHideout strips old labels in place.
- The HUD says little: the gun and its rounds (cash at home) on top, and underneath only a state (dead,
  paused), what E does here, or the enemy count. The controls strip is gone.
- A whole-magazine reload shows MagazineSwap over the head; a round-by-round one shows RevolverCylinder.
- Tools/FlowIoC-Modules/Gameplay/Level Editor places spawns, characters, weapons, doors, stairwell and vent
  pairs, computers, crates, light rooms and the exit by clicking in PB_Level_NightShift's prefab mode, and
  lists what the level is missing or names wrongly.
- Tuning lives in configs, each with a coloured inspector (Scripts/Editor/ConfigInspectors.cs): CD_Weapons (every
  weapon stat), CD_Characters (weapon pool and drop chance, senses: sight, suspicion rate/decay, hearing, chase memory,
  investigate time, curiosity; shooting pace), CD_Gameplay (player movement, melee, doors, gunfire, props), CD_Missions.
- A player bullet striking a wall leaves a disturbance (CD_Gameplay.ImpactCuriosityTime); guards within their
  Curiosity range raise suspicion to ImpactSuspicion and go to look.
- The ten campaign levels between Night Shift and the Docks are recipes in NightShiftAssetBuilder.Contracts.cs,
  laid out from a seed; delete a PB_Level_ prefab and Build Missing writes it again. Their texts have rows in CD_Localization.
- E does one thing per press; InteractContractCommand offers stairs, vent, switch, door, loot, exit in that order.

- Every campaign contract is a location takeover. MissionCVO configures its original OwnerFamily and CivilianCount;
  Reset guarantees those civilians and one unarmed owner in addition to the authored enemy guards. Documents left in
  older level prefabs are hidden and are not objectives. After the armed enemies are clear, the player presses E beside
  the boss and finishes the conversation to take the location. Old document-era run saves are deliberately not restored.
- Families are authored as one CD_Family asset each and registered in CD_Families. A family has separate Don,
  Consigliere, Caporegime, Soldiers and Associates entries; every entry supplies its name list, CD_Characters type
  for gameplay stats, and prefab for presentation. Characters are instantiated directly rather than pooled, so these
  entries use prefab references, not pool keys. MissionCVO.OwnerFamily and AttackingFamily hold family IDs. Normal
  contracts draw the boss, civilians and armed hierarchy from the owning family; defence attackers use the attacking
  family. GuardRVO records the chosen family, rank and name so run saves restore the same cast.
  The shipped player and rival configs are named SQUARE FAMILY and TRIANGLE FAMILY; their stable IDs remain
  `player` and `rival`, so display-name edits never invalidate missions or saves.
- The first time home in a session the phone rings (RingPhoneCommand / PhoneCallCommand); the caller and lines are in CD_Missions.
- A contract intro puts the Rectangle's headphones on before the phone comes out (ContractRVO.IntroHeadset).

- Sounds are synthesized WAVs under Art/Audio (and the UI clicks in each screen's Resources); CD_Sounds lists them and
  NightShiftView plays footsteps, switch clicks, lamp hum, the cat and the ambience from the run state.

- The HUD's MENU pauses the run behind the main menu (PauseForMenuCommand); Play resumes it (PlayCommand), or on a fresh
  launch opens the contract the save was on. The save is persistentDataPath/save.json (ProgressSVO): cash, finished
  contracts, upgrades, owned outfits (CD_Outfits.Price), worn outfit, hand/pocket/locker weapons, current contract.
  The contract itself (the whole ContractRVO) goes to run.json every 3 s and when the menu opens (SaveRunCommand);
  a fresh launch into that contract restores it (RestoreRunCommand). Finishing it or going home deletes run.json.

- Guard tactics (CD_Characters → Tactics): UsesCover runs to a free crate between it and the player and peeks to shoot
  (crouched it is a 1.0-high target); RaisesAlarm makes a guard who saw the player run to the unwarned ones first and
  tell them where (they come to fight but do not pass the warning on). Shipped: triangles raise the alarm, hexagons rush.

- All harm goes through INightShiftModel.DamagePlayer / DamageGuard: bullets (their Damage), blasts, falling lamps and
  takedowns (CD_Gameplay → Damage) take Health off; nothing sets Alive=false directly.

- Money: dead enemies drop cash (CD_Characters → Loot.CashDrop) as PB_CashPickup; walking over it adds to
  ContractRVO.CashCarried, banked only when the contract is finished. Upgrades cost cash; CD_Gameplay → Money switches
  mission rewards (MissionsPay), recurring working-location income (WorkingLocationsPay), and outfit prices
  (OutfitsCost). A completed mission pays MissionCVO.WorkPayment every WorkPaymentInterval real-time seconds;
  either value at zero disables that location. ProgressSVO stores each location's last UTC payment boundary, so
  paused time and time while the game is closed are collected on the next tick. Legacy saves start their clock when
  first opened with this feature and receive no arbitrary retroactive windfall.
- Working locations are territories. Every MissionCVO sets LocationDifficulty, AttackChance and AttackingFamily;
  CD_Gameplay sets the real-time roll interval, the five-minute response window and attacker counts by difficulty.
  ProcessWorkingLocationAttacksCommand advances the same UTC schedule online and offline. A successful roll marks the
  location UnderAttack and raises a localized HUD banner; the mission board shows its live deadline and launches its
  defence. Missing the deadline marks it Lost, removes its recurring income and records the attacking family. A lost
  row starts the original contract again so it can be taken back. The defence reuses the location level, spawns the
  assigned MissionGuards as friendly armed guards, and lets both factions shoot one another. Clearing every attacker
  and exiting restores Owned; death or abandoning transfers the location to the configured family.
- Posted men are paid from CD_Territory: GuardHirePrice is charged once when the map's plus button posts one,
  with no refund on removal, and GuardUpkeepPerPayment is deducted per man from every location payment. The map
  shows net hourly income, hourly guard upkeep and the hire price; a location's net payment never falls below zero.

- Every contract level was widened x1.25 and given extra guards in roomy rooms by a one-off builder pass
  (NightShiftAssetBuilder.Spacious, LevelAuthoring.LayoutVersion). Feel: per-weapon shake, camera kick, hit flash and
  a short hit-stop on kills (NightShiftView); melee swings, lunges and knocks the guard back (MeleeKnockback).

## Known gaps
- Levels have at most two floors; the rules assume two storeys and markers are snapped to them.
- The generated art is placeholder rectangles; swap sprites inside the prefabs for production art.
- There is no general-purpose physics platform controller; floors are changed only through stairwell doors.

Update: never

<!-- FLOWIOC:BEGIN version=1 hash=af88de6f | generated by Tools/FlowIoC/Module Scanner - do not edit inside this block -->
**Kind** Main · **Assemblies** Modules.Gameplay, Modules.Gameplay.Signals
**Root** GameplaySystemRoot → GameplaySystemContext
**Incoming** ChooseMenu(int) · CloseDeck() · CloseMenu() · CloseTerminal() · ControlTape(int) · LeaveContract() · MapAction(int, int) · Play() · SaveTerminal(string) · SetMusicVolume(float) · StartContract()
**Outgoing** BannerShown(string) · CallEnded() · CallRinging(string) · CallShown(string, string) · DeckClosed() · DeckShown(string, string, bool) · HudChanged(string, string) · LocationChanged(string) · MenuClosed() · MenuOpened(string, string, String[], bool) · MusicStarted(string, string) · MusicVolumeShown(float) · OpenHud() · ReturnedToMenu() · TerminalClosed() · TerminalOpened(string, string)
**Sub modules** GameplayScreenModule (Screen) · GameplayTestModule (Test)
<!-- FLOWIOC:END -->
