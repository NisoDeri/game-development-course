# Game Development in Game Engines: Assignments

Unity **6000.3.26f1** (Unity 6.3 LTS), template **Universal 2D**. Every assignment lives in its own scene.

| Assignment | Scene | Status |
|------------|-------|--------|
| 01: Out of the Box | `Assets/Scenes/Assignment_01.unity` | in progress |

---

# Assignment 01: Out of the Box

## The story

At night, the board games on the table wake up. You are a **white pawn** that wandered out of its box and got lost
on the green felt. A **red pawn** has noticed you and wants to knock you off the table. Between you and the box
lie **chips** worth 50, 20 or 10 points, and the dealer keeps handing out new ones in batches of five.
Collect **1000 points** and you are crowned. Get caught first and it is over.

The theme is only decoration (green felt background, wooden rim, round chips). The code, tags and object names use
the words of the assignment: `Player`, `Enemy`, `Coin`.

> Screenshot / GIF: _add `Docs/screenshot.png` here_

## Controls

| Key | Action |
|-----|--------|
| `W` `A` `S` `D` | Move the white pawn (diagonal movement is not faster) |

Tip: set the Game view to **16:9** so the whole table (and the wooden rim) is visible.

## Rules

- The white pawn is moved with WASD. The red pawn chases it all the time.
- At the start, **5 chips** appear at random positions. Each chip has a random type: orange = 50, green = 20, blue = 10.
- Touching a chip destroys it and adds its value to the score. Collect all 5 and the next batch of 5 appears.
- Reach the **target score (1000)** and the console prints `you won` (once). The red pawn is removed.
- If the red pawn touches you, the console prints `you lose` (once). The white pawn is removed.
- The red pawn does nothing to chips, and chips never react to each other. Only the player collects chips.
- Every collected chip prints a progress line to the console, for example `+50 → 420/1000`.

## Where each requirement is implemented

| # | Requirement | Implementation |
|---|-------------|----------------|
| 1 | White capsule, moved with WASD | `PlayerMovement.Update` reads `Keyboard.current` (normalized `Vector2`). White capsule sprite on the `Player` object |
| 2 | Red capsule that constantly chases the player, speed editable | `EnemyChase.FixedUpdate` moves towards the player; `public float speed` |
| 3 | Coin is a circle created at runtime from one prefab with `Instantiate` | `Assets/Prefabs/Coin.prefab`; `CoinSpawner.SpawnCoin` calls `Instantiate` |
| 4 | 3 coin types: value and color set in the Inspector, type chosen randomly when created | `CoinSpawner.coinValues` / `coinColors`; `Random.Range(0, coinValues.Length)` in `SpawnCoin` |
| 5 | Batch of 5 at start; when all 5 are collected a new batch | `CoinSpawner.Start` → `SpawnBatch`; `OnCoinCollected` counts down `coinsRemaining` and calls `SpawnBatch` at 0 |
| 6 | Random positions inside an editable range | `CoinSpawner.spawnMin` / `spawnMax`, `Random.Range` for x and y |
| 7 | Player touches coin → `Destroy`. Enemy touches player → `you lose`. Target reached → `you won` | `Coin.OnTriggerEnter2D`; `EnemyChase.OnTriggerEnter2D` → `GameManager.PlayerCaught`; `GameManager.CoinCollected` |
| 8 | Tags: only the player collects a coin | `other.CompareTag("Player")` in `Coin.OnTriggerEnter2D`. Coins have no `Rigidbody2D`, so coin–coin overlaps raise no events |
| 9 | Player and enemy move with `Rigidbody2D` in `FixedUpdate` | `rb.MovePosition(...)` in `PlayerMovement.FixedUpdate` and `EnemyChase.FixedUpdate` |

### Physics setup (why it is built this way)

| Object | Rigidbody2D | Collider2D |
|--------|-------------|------------|
| Player | Dynamic, gravity scale 0, freeze rotation Z, interpolate | Capsule, **not** a trigger |
| Enemy | Kinematic | Capsule, **trigger** |
| Coin (prefab) | none | Circle, **trigger** |
| Walls (4) | none | Box, not a trigger |

In 2D physics a Kinematic body gets no trigger events against bodies without a Rigidbody and is not blocked by walls.
A Dynamic player solves both problems, while the Kinematic trigger enemy never pushes the player.

## Scripts (`Assets/Scripts/Assignment01/`)

| Script | On | Role |
|--------|----|------|
| `PlayerMovement` | Player | WASD input, `MovePosition` |
| `EnemyChase` | Enemy | chase the player; calls `GameManager.PlayerCaught` on contact |
| `Coin` | Coin prefab | holds its value; calls `GameManager.CoinCollected` when the player touches it, then destroys itself |
| `CoinSpawner` | CoinSpawner | spawns batches of coins from the prefab |
| `GameManager` | GameManager | score, win and lose messages, game-over flag |

Call flow: `Coin → GameManager → CoinSpawner` and `Enemy → GameManager`.

## Parameters you can change in the Inspector

| Object / script | Field | Default |
|-----------------|-------|---------|
| Player / `PlayerMovement` | `speed` | 6 |
| Enemy / `EnemyChase` | `speed` | 4.5 |
| Enemy / `EnemyChase` | `player`, `gameManager` | references |
| CoinSpawner / `CoinSpawner` | `coinPrefab` | `Coin` prefab |
| | `coinsPerBatch` | 5 |
| | `spawnMin` | (-7.6, -3.8) |
| | `spawnMax` | (7.6, 3.8) |
| | `coinValues` | 50, 20, 10 |
| | `coinColors` | orange, green, blue |
| | `gameManager` | reference |
| GameManager / `GameManager` | `targetScore` | 1000 |
| | `coinSpawner`, `player`, `enemy` | references |

`coinValues` and `coinColors` are matched by index: element 0 of both arrays is the first coin type, and so on.

## How to run

1. Open the project with Unity **6000.3.26f1**.
2. Open `Assets/Scenes/Assignment_01.unity`.
3. Press Play and watch the Console.

---

_Course: Game Development in Game Engines._
