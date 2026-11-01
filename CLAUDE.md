# CLAUDE.md — Unity course project · Assignment 01 "Out of the Box"

## Context
- Unity **6000.3.26f1** (Unity 6.3 LTS), template **Universal 2D** (URP, 2D Renderer). IDE: Rider.
- University course "Game Development in Game Engines". This repo will hold all course assignments, each in its own scene.
- Current task: **Homework 1**, scene `Assets/Scenes/Assignment_01.unity`. Deadline 13/11 21:00.
- Grading: every requirement implemented correctly, following practices shown in class.
  **Using features not yet taught lowers the grade.** Simple, readable, beginner-level code wins.
- The student must be able to explain every line. Prefer clarity over cleverness.
  Add short English comments that explain *why*, not *what*.

## Theme (presentation only — never changes mechanics)
A game table at night. Player = a lost white pawn. Enemy = the red pawn. Coins = round chips.
Table = green felt with a wooden rim. Code, tags and object names stay in the assignment's
vocabulary (Player, Enemy, Coin) so the grader recognizes every requirement instantly.

## Assignment spec — must match literally
1. Player is a **white capsule**, moved with **WASD**.
2. Enemy is a **red capsule** that **constantly chases** the player; speed adjustable in the Inspector.
3. Coin is a **circle**, created at runtime from **one prefab** with `Instantiate`.
4. 3 coin types. The type determines **score value and color**; values and colors editable in the
   Inspector. Type chosen **randomly when the coin is created** (`Random.Range`).
5. Game start: a batch of **5** coins. When **all 5** are collected → a new batch of 5, and so on.
6. Coin positions are random within a range (min/max) editable in the Inspector.
7. Triggers: player touches coin → coin destroyed immediately (`Destroy`).
   Enemy touches player → `Debug.Log("you lose")`. Target score reached → `Debug.Log("you won")`.
   **Exact strings, each printed once.**
8. Tags: a coin disappears **only** when the Player touches it. Enemy–coin or coin–coin overlap → nothing.
9. Player and enemy move with **Rigidbody2D inside FixedUpdate**.

Example config: Orange = 50, Green = 20, Blue = 10. Target = 1000.
No need to prevent coins from spawning on top of each other.

## Allowed in runtime scripts (lectures 1–2)
`MonoBehaviour`; `Awake` / `Start` / `Update` / `FixedUpdate`; **public fields** to expose values in the
Inspector (as in the lecture); references assigned in the Inspector; `GetComponent<T>()`;
`Debug.Log` / `Debug.LogWarning`; `Vector2` / `Vector3`; `Time.fixedDeltaTime`;
`Rigidbody2D.MovePosition`; `Collider2D`, `isTrigger`, `OnTriggerEnter2D`; `CompareTag`;
prefabs, `Instantiate`, `Destroy`; `Random.Range`; `Keyboard.current` from `UnityEngine.InputSystem`
(as in lecture 1); basic C# (`if`, `for`, arrays, `int`, `float`, `bool`, string interpolation).

## Forbidden in runtime scripts (not taught yet)
(Applies to the 5 core scripts. The only exceptions are the ones the lecturer approved, listed in
"Extensions" below, and only inside the extension scripts.)
UI / Canvas / TextMeshPro · coroutines / `IEnumerator` · `Invoke` · events, delegates, `Action`, `UnityEvent` ·
singletons / `static` instances · any `Find…` (`GameObject.Find`, `FindObjectOfType`,
`FindFirstObjectByType`, `FindGameObjectWithTag`) · ScriptableObjects · `[System.Serializable]` custom classes ·
attributes such as `[SerializeField]`, `[Header]`, `[Range]` · LINQ · C# properties, interfaces,
inheritance beyond `MonoBehaviour` · Animator · AudioSource · ParticleSystem · adding lights or
post-processing · `Time.timeScale` · `SceneManager` · `PlayerPrefs` · async.
If something seems to require one of these, stop and ask.

## Physics setup (2D gotcha — keep exactly)
- **Player**: `Rigidbody2D` **Dynamic**, gravity scale 0, freeze rotation Z, interpolation Interpolate;
  `CapsuleCollider2D` **not** a trigger. Why: in 2D a Kinematic body gets no trigger callbacks against
  static colliders or other Kinematic bodies (unless "Use Full Kinematic Contacts"), and a Dynamic body
  is blocked by the walls when moved with `MovePosition`.
- **Enemy**: `Rigidbody2D` **Kinematic**; `CapsuleCollider2D` **isTrigger**. Its trigger fires against the
  Dynamic player, and it never physically pushes the player.
- **Coin prefab**: `CircleCollider2D` **isTrigger**, **no** Rigidbody2D, so coin–coin overlaps produce no events.
- **Walls**: 4 × `BoxCollider2D` (not trigger), no Rigidbody2D.
- Everything at z = 0.

## Runtime architecture — 5 scripts in `Assets/Scripts/Assignment01/`
- **`PlayerMovement`** (on Player): `public float speed = 6f`. `Update`: read W/A/S/D with
  `Keyboard.current` into a **normalized** `Vector2` (diagonals not faster); guard `Keyboard.current == null`.
  `FixedUpdate`: `rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime)`.
- **`EnemyChase`** (on Enemy): `public float speed = 4.5f; public GameObject player;
  public GameManager gameManager`. `FixedUpdate`: if `player == null` (destroyed) do nothing; else move
  toward it with `MovePosition`, scaling by `speed * Time.fixedDeltaTime` **exactly once**.
  `OnTriggerEnter2D`: if `other.CompareTag("Player")` → `gameManager.PlayerCaught()`.
- **`Coin`** (on the prefab): `public int value; public GameManager gameManager` — both set by the
  spawner, because a prefab cannot reference scene objects. Private `bool collected` guard.
  `OnTriggerEnter2D`: if not collected and `other.CompareTag("Player")` → mark collected,
  `gameManager.CoinCollected(value)`, `Destroy(gameObject)`.
- **`CoinSpawner`** (empty object): `public GameObject coinPrefab; public int coinsPerBatch = 5;
  public Vector2 spawnMin; public Vector2 spawnMax; public int[] coinValues; public Color[] coinColors;
  public GameManager gameManager`. `Start` → `SpawnBatch()`. `SpawnCoin()`:
  `int type = Random.Range(0, coinValues.Length)`, random position inside min/max (z = 0), `Instantiate`,
  then set `value` and `gameManager` via `GetComponent<Coin>()` and the color via
  `GetComponent<SpriteRenderer>().color`. `OnCoinCollected()`: decrement `coinsRemaining`;
  at 0 → `SpawnBatch()`. In `Start`, warn once if the two arrays have different lengths.
- **`GameManager`** (empty object): `public int targetScore = 1000; public CoinSpawner coinSpawner;
  public GameObject player; public GameObject enemy`. Private `int score`, `bool isGameOver`.
  `CoinCollected(int value)`: if over → return; add to score; log progress like `+50 → 420/1000`;
  if score ≥ target → `isGameOver = true`, `Debug.Log("you won")`, `Destroy(enemy)`, return
  (the win can happen mid-batch); else `coinSpawner.OnCoinCollected()`.
  `PlayerCaught()`: if over → return; `isGameOver = true`; `Debug.Log("you lose")`; `Destroy(player)`.
  Extension getters (fields stay private): `GetScore()`, `IsGameOver()`, `IsWon()` (private `bool isWon`, set on the win).
  `CoinSpawner` also has a private `int batchNumber` (incremented at the start of `SpawnBatch`, so 1 during the
  first batch) and a getter `GetBatchNumber()`.
- Call flow of the core: Coin → GameManager → CoinSpawner. Enemy → GameManager. No other cross-references.
  Extensions read the state through the getters above (and the public `targetScore`); the only call into the core is
  `KnightEnemy` → `GameManager.PlayerCaught()`, exactly like `EnemyChase` (see "Extensions").

## Local Editor builder (NOT graded, NEVER in the submission repo)
- `Assets/_LocalTools/Editor/Assignment01Builder.cs`. Editor scripting is only taught in week 13, so it must
  never be submitted. Any Editor API is fine inside it.
- **Repo roles:** `NisoDeri/game-development-course` is ONLY a private transfer channel between the student's two PCs
  and will be deleted later. The builder (and `CLAUDE.md`, `PROMPTS.md`) MAY be committed THERE. The real
  submission repo is a different one: it must not contain `Assets/_LocalTools/` (+ `.meta`), `CLAUDE.md` or
  `PROMPTS.md` (add them to its `.gitignore`). Never push anything to the submission repo unless asked.
- Menu `Tools/Assignment 01/Build Scene`. Idempotent: running it again rebuilds the scene and prefab.
- Never hand-edit `.unity` / `.prefab` YAML. Change the builder and re-run it instead.
- Generated sprites go to `Assets/Art/Generated/` and **are** committed (they are just images).

## Visual rules for HW1 (static sprites only; HUD text is the approved exception)
- Allowed: sprites, SpriteRenderer colors, sorting order, child "Shadow" sprites, camera background.
- Not allowed: lights beyond the template default, particles, Animator, post-processing.
  (UI is allowed only for the approved HUD extension.)
- Palette: felt `#1E5A3C` (darker toward edges), rim wood `#5B3A24`, player `#FFFFFF`,
  enemy `#D93A3A`, chips orange `#F28C28` / green `#3CB371` / blue `#3A7BD5`,
  knight `#202020`, shadows black at 50% alpha (made clearly visible by the lecturer-approved polish).
- Sorting order: background -10, chessboard -9, rim -5, shadows 5, knight marker 6, coins 8, pawns 10, knight 11.
- Sizes (world units): pawn 0.5 × 1, chip diameter 0.5. Camera orthographic size 5.
  Walls 0.6 thick, centered at x = ±8.5 and y = ±4.7 (inner area about x ±8.2, y ±4.4).
  Default spawn range (-7.6, -3.8) to (7.6, 3.8).

## Repo
- Standard Unity `.gitignore` (Library, Temp, Obj, Logs, UserSettings, IDE files) + `*.pdf` + the syllabus image.
  In the private transfer repo `CLAUDE.md`, `PROMPTS.md` and `Assets/_LocalTools/` are tracked on purpose
  (see "Local Editor builder"). The submission repo ignores them.
- Standing order from the student: when a task is finished, and after every later change,
  commit (small steps, trailer per the system reminder), `git push origin main`, and list the changed files.
- Identity: only the personal GitHub account `NisoDeri` (repo-local git config with the no-reply email). Never the NVIDIA one.
- The submitted version gets the git tag `assignment-01` (in the submission repo).
- `README.md` (English): pitch, controls, rules, requirement → implementation table,
  Inspector parameters, scene name, Unity version, screenshot/GIF.

## Extensions (approved by the lecturer)
The lecturer confirmed in writing that HW1 may be extended as long as every requirement still works.
Rules:
- All 9 spec items must keep working unchanged. The exact `Debug.Log("you won")` / `Debug.Log("you lose")`
  messages stay, each printed once.
- Never add: lives/shields, freezing the red enemy, a 4th coin type, time-based coin spawns, score multipliers.
- Each extension is its own script/component in `Assets/Scripts/Assignment01/Extensions/`. Disabling or
  deleting it must give back the exact original assignment. The builder creates every extension in a state
  where the original game is unaffected if its component is switched off (e.g. the knight starts hidden).
- Core scripts change only by the additive read-only getters listed in "Runtime architecture". Extensions READ the
  game state through them (`GetScore()`, `IsGameOver()`, `IsWon()`, `GetBatchNumber()`) and the public `targetScore`,
  never by checking whether objects were destroyed. The only call into the core is `KnightEnemy` →
  `GameManager.PlayerCaught()`. The only core fields an extension writes are `PlayerMovement.speed` (`PlayerDash`)
  and `EnemyChase.speed` (`EnemyDifficulty`).
- Code stays simple and explainable. **No coroutines**: use timers in `Update` / `FixedUpdate`
  (`timer += Time.deltaTime`). Private fields; public fields only for Inspector values and references.
- Extra APIs allowed ONLY inside the extension scripts: `UnityEngine.UI` / Canvas, `TMPro` (`TMP_Text`),
  `SceneManager.LoadScene` / `SceneManager.GetActiveScene().name`, `GameObject.SetActive`, `Mathf.Min`,
  `Vector2.Lerp` / `Vector2.Distance`, `OnEnable` / `OnDisable`, `Keyboard.current.<key>.wasPressedThisFrame`,
  and the C# statements `while` and `continue`. Still forbidden everywhere: `Find…`, singletons,
  events/delegates, `Invoke`, Animator, AudioSource, ParticleSystem, `Time.timeScale`, ScriptableObjects, LINQ.

Extensions (files in `Assets/Scripts/Assignment01/Extensions/`):
- **`KnightEnemy`** (on a separate object "Knight"; tag `Enemy`, Kinematic `Rigidbody2D`, trigger capsule collider).
  Starts hidden (renderers + collider disabled by the builder). When `coinSpawner.GetBatchNumber() >= appearAtBatch`
  (default 3) it appears on the corner cell farthest from the player. Board = 16 × 8 cells of 1 unit, cell centers at
  x ±0.5…±7.5, y ±0.5…±3.5 (`gridMin` (-7.5, -3.5), `gridMax` (7.5, 3.5)). Every `moveInterval` = 1.2 s it picks, among
  the 8 L-moves (±1,±2) / (±2,±1) that stay inside the grid, the landing cell closest to the player, shows a
  semi-transparent marker there for `markerTime` = 0.5 s, then hops (`hopDuration` ≈ 0.15 s) with
  `rb.MovePosition(Vector2.Lerp(...))` in `FixedUpdate`. `OnTriggerEnter2D` with the Player → `gameManager.PlayerCaught()`.
  After game over it stops (and vanishes on a win, like the red enemy). `HasAppeared()` tells the HUD.
- **`PlayerDash`** (on Player): Space → `PlayerMovement.speed` × 2.5 for 0.15 s, 1.5 s cooldown (timers in `Update`).
  Dash only has an effect while the player is moving. It restores the normal speed afterwards and in `OnDisable`.
  This is the one place an extension writes a core field (`PlayerMovement.speed`), so `PlayerMovement` stays untouched.
- **`EnemyDifficulty`** (on Enemy): each new batch multiplies `EnemyChase.speed` by `1 + speedIncreasePerBatch`
  (0.05), capped at `maxSpeedPercentOfPlayer` (0.9) × the player's base speed (read once in `Start`).
  The first batch is not a "new" batch. Writes `EnemyChase.speed`.
- **`Hud`** + **`RestartOnR`** (on a Canvas "HUD", Screen Space Overlay, TextMeshPro texts): score `420 / 1000`,
  `Batch N`, "Knight incoming!" for 2 s when the knight appears, big end message "PROMOTED!" (win) / "CAPTURED!" (lose)
  and "Press R to restart". R reloads the active scene, only after game over.
  TMP Essential Resources must be imported once (`Window > TextMeshPro > Import TMP Essential Resources`);
  the builder skips the HUD with a warning if they are missing.
- **`CoinPopIn`** (on the Coin prefab): the coin scales from (almost) 0 to full size in 0.2 s in `Update`.
  Collected coins are still destroyed immediately, even mid-animation. Unticking the component gives full-size coins.
- **Chessboard** (visual only, no script): generated `Chessboard.png`, 18 × 10 cells of 1 unit (the rim covers most
  of the outer ring; thin partial cells at the edges are fine), very low contrast so the chips stay the most visible thing, aligned with the knight's grid.
- **Shadows**: alpha 50% and about 30% larger.

## How to work with me (the student)
- Before writing code, show a short plan and wait for my OK.
- Only touch `Assets/Scripts/Assignment01/` (including `Extensions/`), `Assets/_LocalTools/`, `README.md`,
  `.gitignore`, `CLAUDE.md` (and `Assets/Art/Generated/` through the builder). Ask before touching anything else.
- After each change, tell me exactly what to do in the Unity Editor and what I should see.
- Unity recompiles when its window gets focus. If something fails, ask me for the Console text.
