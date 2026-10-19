# Linked Storage

Adds four linked buildings. Every building of a kind shares one set of contents, with no limit on how much it holds. What goes in at one Linked Storage Bin can come out at any other.

## The buildings

Each one has the size, art, cost and build menu place of its vanilla counterpart. Unlike vanilla, it needs no power and can be built anywhere, with no floor under it.

| Building | Like | Holds | Build menu | Research |
|---|---|---|---|---|
| Linked Storage Bin | Storage Bin | Solid materials, but no food | Base, after the Storage Bin | None |
| Linked Refrigerator | Refrigerator | Food | Food, after the Refrigerator | Agriculture |
| Linked Liquid Reservoir | Liquid Reservoir | Liquids | Base, after the Liquid Reservoir | Improved Plumbing |
| Linked Gas Reservoir | Gas Reservoir | Gases | Base, after the Gas Reservoir | HVAC |

The four kinds keep separate contents: a Linked Refrigerator never sees what the bins hold.

### Each building is its own interface

The contents are shared, but each building has its own settings:

- **Filter.** Each building picks the items or elements it works with, in the usual storage-bin filter list (a reservoir's is like a Bottle Emptier's). New buildings start with nothing ticked. A reservoir's output pipe sends out what's ticked, taking turns when several are.
- **Allow Storing / Stop Storing** (button). Duplicants bring the building what its filter ticks. A reservoir's input pipe stores whatever it carries, whatever the filter or this switch says.
- **Allow Taking / Stop Taking** (button). Duplicants may take from the building. It keeps a stock of each ticked item or element, out of the shared contents, for them to take. A reservoir always keeps that stock for its output pipe, which works whatever this switch says.

| Default | Storing | Taking |
|---|---|---|
| Bin, refrigerator | On | On |
| Reservoirs | On (nothing ticked) | Off |

Copy Settings copies the filter and both switches. Duplicants never carry items from one linked building to another of the same kind.

### Seeing the contents

- **Status lines.** A selected building shows three: **Linked Contents** with the total, then **Storing** and **Taking**, each Enabled or Disabled. Hover a line for what it means.
- **Contents section.** The building's usual Contents section lists the whole shared contents, biggest first, with temperature and amount. That includes the stock every linked building of the kind keeps. Clicking a row selects this building's own stock of it, if it has any.
- **Resources.** The resource panel, pinned resources, the Resources screen and the colony's calorie count all include the shared contents of the planetoid you're viewing. With Separate Contents per Planetoid off, every planetoid shows the one shared store.

The material picker in the build menu counts the shared contents too. A building made from material that's only in the shared contents waits for its delivery until a linked building with Taking on stocks that material.

### What the contents keep

- **Insulated.** Everything keeps its temperature. Mixing two batches of the same item averages their temperature by mass.
- **Germs.** Germs are kept, and when two germ types meet, the stronger one wins, as in the game's own mixing.
- **Food** in the shared contents and in a Linked Refrigerator is deep frozen in a sterile atmosphere, so it doesn't spoil. Freshness is averaged by mass when batches mix.
- Stored items are sealed, so sublimating materials don't give off gas.

Each kind of item is kept as one batch, so per-item extras are lost: spices on food are dropped. Items with state of their own aren't accepted: clothing and suits, eggs, charged power banks, bionic boosters and dehydrated food.

### Per planetoid

By default each planetoid, and each rocket interior, has its own contents. Turn off **Separate Contents per Planetoid** to share one set across the whole save. Changing the option moves what's stored: everything merges into one set, or the one set goes to the starting planetoid.

### Deconstructing

A deconstructed linked building drops nothing: its stock goes back into the shared contents. They stay in the save even when no building of that kind is left, and come back when you build one.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Separate Contents per Planetoid | On | | Each planetoid and rocket interior has its own contents |
| Storage Bin Stock | 500 kg | 10 to 5000 | How much of each picked item a bin keeps out for Duplicants to take |
| Refrigerator Stock | 100 kg | 1 to 1000 | The same for each picked food in a refrigerator |
| Liquid Reservoir Stock | 100 kg | 20 to 5000 | How much of its output liquid a reservoir keeps for its pipe and Duplicants |
| Gas Reservoir Stock | 10 kg | 2 to 1000 | The same for a gas reservoir |

## How it works

`LinkedPool`, a component on the save's `SaveGame` object, holds the contents. It keeps one entry per kind, world and item ID: mass (as a double, so very large amounts stay exact), temperature, germs and freshness.

Each building's real `Storage` is only a small buffer, so Duplicant errands, pipes and the side screens all work on real items with the game's own code. `LinkedStorage.Sync` runs every second for bins and refrigerators, and every 200 ms for reservoirs. It puts into the shared contents everything the building doesn't stock and any stock over the limit. Then it tops the stock up from the shared contents. Items a Duplicant has reserved are left alone.

Every linked building of a kind shares a `storageNetworkID`, which `FetchManager.IsFetchablePickup` reads to never fetch from one storage into another on the same network. That stops Duplicants carrying items between linked buildings.

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: adds the strings, the build menu entries and the research unlocks |
| `SaveGame_OnPrefabInit_Patch` | `SaveGame.OnPrefabInit` | Postfix: adds `LinkedPool` to the save |
| `Storage_OnQueueDestroyObject_Patch` | `Storage.OnQueueDestroyObject` | Prefix: a linked building returns its stock to the shared contents before the storage drops it |
| `Rottable_RefrigerationLevel_Patch` | `Rottable.RefrigerationLevel` | Prefix: food in a Linked Refrigerator is deep frozen |
| `Rottable_AtmosphereQuality_Patch` | `Rottable.AtmosphereQuality` | Prefix: food in a Linked Refrigerator is in a sterile atmosphere |
| `SimpleInfoScreen_RefreshStoragePanel_Patch` | `SimpleInfoScreen.RefreshStoragePanel` | Prefix: a linked building's Contents section lists the shared contents |
| `WorldInventory_GetTotalAmount_Patch` | `WorldInventory.GetTotalAmount` | Postfix: adds the shared contents to the world's amounts, by item and by every tag the item has. `GetAmount` reads through it |
| `RationTracker_CountAmount_Patch` | `WorldResourceAmountTracker<RationTracker>.CountAmount` | Postfix: adds the shared contents' food to the calorie count |
| `RationTracker_CountAmountForItemWithID_Patch` | `RationTracker.CountAmountForItemWithID` | Postfix: the same, for one food |

## Notes

- The building text is English only.
- Saves store the `LinkedStorage.LinkedPool` and `LinkedStorage.LinkedStorage` components and their fields by name. Removing the mod loses the shared contents.
- Patch targets were checked against game build 744825.
