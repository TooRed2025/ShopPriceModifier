## Readme

After installing convenience mods such as TeamUpgrades and SharedHealth, the game becomes too easy in the later stages and loses playability. The goal of this plugin is to force players to spend more money on repeatedly purchasable items, preserving resource pressure and strategic depth in the mid-to-late game.

## Features

- Separately adjusts prices for **Upgrade Items**, **Health Packs**, and **Energy Crystals** (weapons and handcarts are **not** affected)
- Each category independently supports:
  - **Base multiplier** for direct price scaling
  - **Random multiplier** for per-shop-visit price fluctuation
  - **Player-count influence** for dynamic pricing in lobbies
  - **Custom growth values** that override the vanilla "purchase count / level → price" scaling
- Default configuration removes the vanilla discount scheme (more players = cheaper) and **reverses** it (more players = more expensive)
- Works in **both single-player and multiplayer**

## Configuration

### Upgrade Price

| Key | Default | Range | Description |
|---|---|---|---|
| `BaseMultiplier` | `1.0` | 0.01 ~ 10 | Base price multiplier for upgrade items. `0.5` = half price, `1.0` = vanilla |
| `PlayerInfluence` | `0.1` | -0.1 ~ 1 | Price adjustment per additional player. `0.1` = +10% per player, `-0.05` = −5% per player |

### Health Pack Price

| Key | Default | Range | Description |
|---|---|---|---|
| `BaseMultiplier` | `1.0` | 0.01 ~ 10 | Base price multiplier for health packs |
| `PlayerInfluence` | `0.1` | -0.1 ~ 1 | Price adjustment per additional player (same semantics as above) |

### Energy Crystal Price

| Key | Default | Range | Description |
|---|---|---|---|
| `BaseMultiplier` | `1.0` | 0.01 ~ 10 | Base price multiplier for energy crystals |

### Random Price (Global)

| Key | Default | Range | Description |
|---|---|---|---|
| `RandomEnable` | `false` | true / false | Master switch for random price fluctuation across all items |
| `RandomMinMultiplier` | `0.8` | 0.1 ~ 2 | Lower bound of the random multiplier |
| `RandomMaxMultiplier` | `1.2` | 0.1 ~ 5 | Upper bound of the random multiplier |

When enabled, every shop visit draws a random multiplier from `[min, max]` and stacks it onto each item's base multiplier.

### Advanced — Override Vanilla Growth

Disabled by default. When enabled, these values replace the vanilla "purchase count / cleared levels → price" formula.

| Key | Default | Range | Description |
|---|---|---|---|
| `EnableCustomBaseIncrease` | `false` | true / false | Master switch for custom growth values |
| `UpgradeValueOwnedIncrease` | `0.5` | 0.1 ~ 1 | Price growth factor per upgrade item purchase |
| `HealthPackValueLevelIncrease` | `0.05` | 0.01 ~ 0.1 | Price growth factor per cleared level for health packs |
| `CrystalValueLevelIncrease` | `0.2` | 0.01 ~ 0.5 | Price growth factor per cleared level for energy crystals |
| `MaxLevelLimit` | `15` | 15 ~ 100 | Upper limit of affected levels |

### Logging

| Key | Default | Range | Description |
|---|---|---|---|
| `EnableDebugLogging` | `false` | true / false | When enabled, prints per-item price calculation details to the BepInEx log for tuning/debugging |
