# Code Style — comet_cleave (Unity C#)

Conventions for `Assets/- Game -/`.

**Architecture:** [ARCHITECTURE.md](ARCHITECTURE.md) · **Systems:** [SYSTEMS.md](SYSTEMS.md)

---

## Agent instructions

- Unity C# unless stated otherwise.
- **Preserve style** in touched files; change only what logic requires.
- **Never add** comments in code (`//`, `/* */`, `///`).
- **Never delete** existing comments — the author uses them as reminders (`TODO`, stubs, notes).
- Follow existing **TODO** markers only when the task says so.
- Explanations to the user: **Ukrainian**.
- No full-file review unless asked; if logic is fine, say so explicitly.

## Comments

The author may leave comments as reminders. Agents never write comments and never remove existing ones.

---

## Naming

### No abbreviations

```csharp
// ✗
void Add(string id, int amt) { }
private Dictionary<string, CardItemRuntime> _cards;

// ✓
void AddAmount(string identifier, int amount) { }
private Dictionary<string, CardItemRuntime> _cardCollection;
```

### Types and files

```csharp
// File: CardSystem.cs
public class CardSystem : ISavable { }

// File: CardItemSave.cs
[Serializable]
public class CardItemSave
{
    public int Level;
    public int Amount;
    public UnlockState UnlockState;
}
```

### Fields and properties

```csharp
public class CardConfig : BaseConfig
{
    [SerializeField] private int _defaultAmount;
    [SerializeField] private int _maxSelected;

    public int DefaultAmount => _defaultAmount;
    public int MaxSelected => _maxSelected;
}
```

### Events

```csharp
public event Action OnSaveRequired;
public event Action OnSelectionModeCompleted;
public event Action<string> OnItemSelected;
public event Action<string, int> OnReplaceModePending;
```

### Constants

```csharp
private const string SAVE_KEY = "CardSave";
```

### UI types

```csharp
// File: UICardCollectionPresenter.cs
public class UICardCollectionPresenter : BaseBehaviour, IInitializable
{
    [SerializeField] private UICardCollectionElement _uiItemPrefab;
    [SerializeField] private RectTransform _elementsContainer;
}
```

### Casing

| Element | Style | Example |
|---------|--------|---------|
| Public type, method, property, event | PascalCase | `CardSystem`, `AddAmount`, `OnSaveRequired` |
| Private / protected field | `_camelCase` | `_cardCollection`, `_cardConfig` |
| Private constant | `SCREAMING_SNAKE_CASE` | `SAVE_KEY` |
| Local / parameter | camelCase, full words | `identifier`, `unlockStatus`, `cardItemRuntime` |

### Allowed suffixes

`System`, `Config`, `Save`, `ItemAsset`, `ItemRuntime`, `ItemSave`, `Asset`, `Runtime`, `Presenter`, `Element`, `Popup`.

### Forbidden in names

`id`, `cfg`, `btn`, `amt`, `num`, `tmp`, `util`, `mgr`, `ctrl`, `sys`, `go`, `obj` (as shorthand).

---

## Formatting

### `if` — always braces

```csharp
if (_itemCollection.TryGetValue(identifier, out var currencyItemRuntime))
{
    currencyItemRuntime.Amount.AddValue(amount);
    OnSaveRequired?.Invoke();
}
```

```csharp
if (selectedCounter >= maxSelected)
{
    break;
}
```

### `else`

```csharp
if (cardLevelUpAsset != null)
{
    return cardLevelUpAsset.GetLevelUpSettings(levelIndex);
}

return new CardLevelUpSettings();
```

### `switch`

```csharp
foreach (var cardItemRuntime in cardItemRuntimes)
{
    switch (cardItemRuntime.Unlock.State)
    {
        case UnlockState.Pending:
            _pendingCollection.Add(cardItemRuntime);
            break;
        case UnlockState.Unlocked:
            _unlockedCollection.Add(cardItemRuntime);
            break;
    }
}
```

### Pattern matching

```csharp
if (cardItemRuntime.Unlock.State is UnlockState.Locked)
{
    _cardSystem.ChangeUnlock(cardItemAsset.Identifier, UnlockState.Unlocked);
}
```

### `foreach` (preferred over `for`)

```csharp
foreach (var itemAsset in _cardConfig.AssetCollection)
{
    var level = 0;
    var unlockStatus = UnlockState.Locked;

    if (save.CardCollection.TryGetValue(itemAsset.Identifier, out var itemSave))
    {
        level = itemSave.Level;
        unlockStatus = itemSave.UnlockState;
    }

    var cardItemRuntime = new CardItemRuntime(itemAsset, level, unlockStatus);
    _cardCollection.Add(itemAsset.Identifier, cardItemRuntime);
}
```

### Dictionary deconstruction

```csharp
foreach (var (identifier, cardItemRuntime) in _cardCollection)
{
    var shouldSave = cardItemRuntime.Level > 0
                     || cardItemRuntime.Unlock.State != UnlockState.Locked;
    if (shouldSave)
    {
        save.CardCollection.Add(identifier, new CardItemSave
        {
            Level = cardItemRuntime.Level,
            UnlockState = cardItemRuntime.Unlock.State
        });
    }
}
```

### No LINQ

```csharp
// ✗
var unlocked = _cardCollection.Values.Where(h => h.Unlock.State == UnlockState.Unlocked).ToList();

// ✓
var unlockedCollection = new List<CardItemRuntime>();
foreach (var cardItemRuntime in _cardCollection.Values)
{
    if (cardItemRuntime.Unlock.State == UnlockState.Unlocked)
    {
        unlockedCollection.Add(cardItemRuntime);
    }
}
```

### `var` — when yes / when no

```csharp
var save = SaveLoadStorage.Load<CardSave>(SAVE_KEY);
var cardItemRuntime = GetItemRuntime(identifier);
foreach (var itemAsset in _config.AssetCollection) { }

int level = 0;
UnlockState unlockStatus = UnlockState.Locked;
```

### Target-typed `new`

```csharp
private readonly Dictionary<string, CardItemRuntime> _cardCollection = new();
private readonly List<CardItemRuntime> _pendingCollection = new();
```

### Object initializer

```csharp
var save = new CardSelectionSave
{
    SelectedCollection = _selectedCollection
};

var itemSave = new CardItemSave
{
    Level = cardItemRuntime.Level,
    UnlockState = cardItemRuntime.Unlock.State
};
```

### Blank lines

- One blank line after `using` block.
- Blank line between methods.
- Optional blank line inside a method between logical steps.

---

## Class layout (full example)

```csharp
using System;
using System.Collections.Generic;

public class CardSelection : ISavable, IInitializable, IDisposable
{
    private const string SAVE_KEY = "CardSelection";

    public event Action OnSaveRequired;
    public event Action<string, int> OnItemSelected;

    private readonly CardConfig _cardConfig;
    private readonly CardUnlocker _cardUnlocker;
    private readonly Dictionary<string, int> _selectedCollection = new();

    public CardSelection(CardConfig cardConfig, CardUnlocker cardUnlocker)
    {
        _cardConfig = cardConfig;
        _cardUnlocker = cardUnlocker;
    }

    void ISavable.Load() { }

    void ISavable.Save() { }

    void IInitializable.Initialize() { }

    void IDisposable.Dispose() { }

    public bool IsSelected(string identifier) { return false; }

    private void Select(string identifier, int index) { }

    private void Unselect(string identifier, int index) { }
}
```

Member order:

1. `private const`
2. `public event`
3. `private readonly` fields, then other `private` fields
4. Constructor
5. Explicit interface implementations
6. Public methods
7. Private methods

---

## Explicit interface implementation

```csharp
void ISavable.Load()
{
    var save = SaveLoadStorage.Load<CardSave>(SAVE_KEY);
}

void ISavable.Save()
{
    var save = new CardSave();
    SaveLoadStorage.Save(SAVE_KEY, save);
}

void IInitializable.Initialize()
{
    _cardUnlocker.OnUnlockCompleted += AutoChoose;
}

void IDisposable.Dispose()
{
    _cardUnlocker.OnUnlockCompleted -= AutoChoose;
}
```

Public API on the class:

```csharp
public void LevelUp(string identifier)
{
    const int defaultValue = 1;
    var cardItemRuntime = GetItemRuntime(identifier);

    cardItemRuntime.AddLevel(defaultValue);
    OnSaveRequired?.Invoke();
}
```

---

## Early return

```csharp
private void AutoChoose(string identifier)
{
    var isCompleted = _selectedCollection.Count >= _cardConfig.MaxSelected;
    if (isCompleted)
    {
        _cardUnlocker.OnUnlockCompleted -= AutoChoose;
        return;
    }

    if (IsSelected(identifier))
    {
        return;
    }

    var index = _selectedCollection.Count;
    Select(identifier, index);
}
```

---

## TryGetValue

```csharp
if (save.CardCollection.TryGetValue(itemAsset.Identifier, out var itemSave))
{
    level = itemSave.Level;
    unlockStatus = itemSave.UnlockState;
}
```

```csharp
public CardItemRuntime GetItemRuntime(string identifier)
{
    return _cardCollection.GetValueOrDefault(identifier);
}
```

---

## out parameters

`out` is fine when the caller uses the value (`TryGetValue`, `TryGetReward`).

`out _` means this caller does not need that value. Add a `bool` method for that question (`HasClaimableReward`, `IsClaimableReward`). Do not discard the parameter, and do not implement the new method as `return TryGetSomething(out _)`.

Keep a shared search private. Each public method returns only what its caller uses.

```csharp
// ✗ caller ignores the value
var hasReward = TryGetClaimableReward(out _);

// ✗ wrapper around the same discard
public bool HasClaimableReward()
{
    return TryGetClaimableReward(out _);
}

// ✓
public bool HasClaimableReward()
{
    return FindClaimableRewardIndex() >= 0;
}
```

---

## Unity — Config (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "Config_Card", menuName = ProjectSettings.TITLE + "/Configs/Card Config")]
public class CardConfig : BaseConfig
{
    [Header("Item Collections")]
    [SerializeField] private List<CardItemAsset> _assetCollection = new();
    [ReadOnly]
    [SerializeField] private Dictionary<string, CardItemAsset> _assetByIdentifier = new();

    public IReadOnlyList<CardItemAsset> AssetCollection => _assetCollection;

    public CardItemAsset GetItemAsset(string identifier)
    {
        return _assetByIdentifier.GetValueOrDefault(identifier);
    }
}
```

```csharp
[CreateAssetMenu(fileName = "CardItemAsset", menuName = ProjectSettings.TITLE + "/Cards/Card Item Asset")]
public class CardItemAsset : BaseItemAsset
{
    [SerializeField] private RarityType _rarityType;
    [SerializeField] private string _title;

    public RarityType RarityType => _rarityType;
    public string Title => _title;
}
```

---

## Unity — UI and Inject

```csharp
public class UICardCollectionPresenter : BaseBehaviour, IInitializable, ITickable
{
    [SerializeField] private UICardCollectionElement _uiItemPrefab;
    [SerializeField] private RectTransform _elementsContainer;

    private CardSystem _cardSystem;
    private CardSelection _cardSelection;

    [Inject]
    public void Inject(
        CardSystem cardSystem,
        CardUnlocker cardUnlocker,
        CardSelection cardSelection,
        CardLevelUp cardLevelUp,
        UIMenuMediator uiMenuMediator)
    {
        _cardSystem = cardSystem;
        _cardSelection = cardSelection;
    }

    void IInitializable.Initialize()
    {
        _uiElementCollection.Clear();

        var itemCollection = _cardSystem.GetItemCollection();
        foreach (var itemRuntime in itemCollection)
        {
            var uiElement = Instantiate(_uiItemPrefab, _elementsContainer);
            _uiElementCollection.Add(itemRuntime.ItemAsset.Identifier, uiElement);
        }
    }
}
```

MonoBehaviour: **`[Inject]` method**, not constructor injection.

---

## Runtime item

```csharp
public class CardItemRuntime
{
    public CardItemAsset Asset { get; }
    public int Level { get; private set; }
    public UnlockProperty Unlock { get; }

    public CardItemRuntime(CardItemAsset itemAsset, int level, UnlockState unlockState)
    {
        Asset = itemAsset;
        Level = level;
        Unlock = new UnlockProperty(itemAsset.Identifier, unlockState);
    }

    public void AddLevel(int value)
    {
        if (value > 0)
        {
            Level += value;
        }
    }
}
```

---

## Save / dirty

```csharp
private void Select(string identifier, int index)
{
    _selectedCollection[identifier] = index;

    OnItemSelected?.Invoke(identifier, index);
    OnSaveRequired?.Invoke();
}
```

```csharp
public void RemoveAmount(string identifier, int amount)
{
    var cardItemRuntime = GetItemRuntime(identifier);

    cardItemRuntime.Amount.RemoveValue(amount);
    OnSaveRequired?.Invoke();
}
```

Do not call `SaveLoadStorage.Save` from gameplay methods. See [SYSTEMS.md](SYSTEMS.md).

---

## Scene entry (plain C#)

```csharp
namespace Menu
{
    public class MenuEntryPoint : IInitializable
    {
        private readonly RewardRepository _rewardRepository;
        private readonly TransactionSystem _transactionSystem;

        public MenuEntryPoint(RewardRepository rewardRepository, TransactionSystem transactionSystem)
        {
            _rewardRepository = rewardRepository;
            _transactionSystem = transactionSystem;
        }

        void IInitializable.Initialize()
        {
            var pendingRewards = _rewardRepository.GetRewards();
            if (pendingRewards.Count > 0)
            {
                _transactionSystem.ClaimReward(pendingRewards);
                _rewardRepository.Clear();
            }
        }
    }
}
```

---

## Namespaces

| Area | Namespace |
|------|-----------|
| Core | none (same as Engine) |
| Menu scene | `Menu` |
| Gameplay scene | `Gameplay` |

---

## Editor-only code

```csharp
#if UNITY_EDITOR
using UnityEditor;

[Button]
private void RefreshCollection()
{
    ValidateCollection(_assetCollection, _assetByIdentifier);
    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssets();
}
#endif
```

---

## usings order

1. `System`, `System.Collections.Generic`, …
2. Third-party (`Sirenix`, …)
3. `UnityEngine` / `UnityEditor` (editor in `#if UNITY_EDITOR`)
4. Project (`Menu`, `Gameplay`, …)

---

## Anti-patterns

```csharp
// ✗ Abbreviation
void Upd(string id) { }

// ✗ LINQ
_items.Values.Select(x => x.Amount).Sum();

// ✗ Single-line if without braces
if (count > 0) DoWork();

// ✗ Comment
// increase gold
gold += 10;

// ✗ Public ISavable.Load
public void Load() { }

// ✗ Direct save from API
public void AddGold(int value) => SaveLoadStorage.Save(SAVE_KEY, save);

// ✗ out discarded — the caller needs a bool method, not TryGet(out _)
var hasReward = TryGetClaimableReward(out _);

// ✓
public void AddAmount(string identifier, int amount)
{
    var cardItemRuntime = GetItemRuntime(identifier);
    cardItemRuntime.Amount.AddValue(amount);
    OnSaveRequired?.Invoke();
}
```

---

## Checklist

- [ ] Full words in names (`identifier`, `unlockState`)
- [ ] `_camelCase` private, PascalCase public
- [ ] Braces on all `if` / `else` / `switch` bodies
- [ ] `foreach`, no LINQ
- [ ] `var` only when obvious
- [ ] `out` only when the caller uses the value; no `out _`
- [ ] Explicit interface for `ISavable` / `IInitializable` / `IDisposable`
- [ ] `OnSaveRequired?.Invoke()` after persisted changes
- [ ] Agent added no comments; existing comments left in place
