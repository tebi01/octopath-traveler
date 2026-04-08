# Model Layer

Namespace raíz: `Octopath_Traveler_Model`
Namespace de combate: `Octopath_Traveler_Model.CombatFlow`

---

## Entidades de dominio

### `Unit` (abstracta)
Base de `Traveler` y `Beast`.

| Miembro | Tipo | Descripción |
|---------|------|-------------|
| `Name` | `string` (readonly) | No puede ser null/vacío |
| `Stats` | `CombatStats` (readonly) | Estadísticas de combate |
| `Speed` | `int` (computed) | Alias de `Stats.Speed` |

---

### `Traveler : Unit` (sealed)
Personaje jugable.

| Miembro | Tipo | Descripción |
|---------|------|-------------|
| `MaxActiveSkills` | `const int = 8` | Máximo habilidades activas |
| `MaxPassiveSkills` | `const int = 4` | Máximo habilidades pasivas |
| `SkillPoints` | `SkillPoints` (readonly) | SP actual/máximo |
| `Weapons` | `IReadOnlyList<string>` | Tipos de arma disponibles |
| `ActiveSkills` | `IReadOnlyList<string>` | Habilidades activas equipadas |
| `PassiveSkills` | `IReadOnlyList<string>` | Habilidades pasivas equipadas |

Constructor valida: sin duplicados, sin exceder máximos, strings normalizados.

---

### `Beast : Unit` (sealed)
Enemigo.

| Miembro | Tipo | Descripción |
|---------|------|-------------|
| `Skill` | `string` (readonly) | Habilidad especial única |
| `MaxShields` | `int` (readonly) | Escudos máximos |
| `CurrentShields` | `int` (readonly) | Escudos actuales |
| `Weaknesses` | `IReadOnlyList<string>` | Debilidades (armas/elementos) |
| `IsInBreakingPoint` | `bool` (computed) | `CurrentShields == 0` |

Dos constructores: con `currentShields` explícito, o simplificado donde `current = max`.

---

### `CombatStats` (value object, inmutable)

| Propiedad | Descripción |
|-----------|-------------|
| `MaxHp`, `CurrentHp` | HP máximo y actual |
| `PhysicalAttack`, `PhysicalDefense` | Combate físico |
| `ElementalAttack`, `ElementalDefense` | Combate elemental |
| `Speed` | Prioridad de turno |

Validaciones: MaxHp > 0, 0 ≤ CurrentHp ≤ MaxHp, resto ≥ 0.

---

### `SkillPoints` (value object, inmutable)

| Propiedad | Descripción |
|-----------|-------------|
| `MaxSp`, `CurrentSp` | SP máximo y actual |

Validación: MaxSp ≥ 0, 0 ≤ CurrentSp ≤ MaxSp.

---

## Equipos y tablero

### `PlayerTeam` (sealed)
- `MinTravelers = 1`, `MaxTravelers = 4`
- `Travelers`: `IReadOnlyList<Traveler>`
- Valida: conteo en rango, sin nombres duplicados (case-insensitive)

### `EnemyTeam` (sealed)
- `MinBeasts = 1`, `MaxBeasts = 5`
- `Beasts`: `IReadOnlyList<Beast>`
- Mismas validaciones que PlayerTeam

### `BattleBoard` (sealed)
Representa las posiciones físicas en el tablero.

| Miembro | Descripción |
|---------|-------------|
| `PlayerSlots` | `IReadOnlyList<Traveler?>` — 4 slots (puede ser null) |
| `EnemySlots` | `IReadOnlyList<Beast?>` — 5 slots (puede ser null) |
| `PlayerSlotsCount = 4` | |
| `EnemySlotsCount = 5` | |

Los teams se mapean de izquierda a derecha; slots sin unidad quedan `null`.

### `GameState` (sealed)
Contenedor principal del estado de la partida.

| Propiedad | Descripción |
|-----------|-------------|
| `PlayerTeam` | Equipo jugador |
| `EnemyTeam` | Equipo enemigo |
| `Board` | `BattleBoard` |
| `CombatFlow` | `CombatFlowState` (acceso privado a setter) |

Constructor `GameState(PlayerTeam, EnemyTeam)` inicializa el Board y el CombatFlowState.

---

## Utilidades internas del modelo

### `ValidationRules` (internal static)

| Método | Descripción |
|--------|-------------|
| `NormalizeNonEmptyStrings(IEnumerable<string>?, paramName)` | Trim + filtro de nulls/vacíos, lanza si alguno es inválido |
| `EnsureUniqueStrings(IReadOnlyList<string>, paramName)` | Lanza si hay duplicados (case-insensitive) |
| `EnsureUniqueUnitNames(IEnumerable<string>, paramName)` | Igual, para nombres de unidades |

---

## Estado de combate (`CombatFlow`)

### Enums (`CombatEnums.cs`)

**`CombatPhase`**: `NotStarted` → `RoundSetup` → `TurnInProgress` → `RoundEnd` → `Finished`

**`BattleResult`**: `Ongoing` | `PlayerVictory` | `PlayerDefeat`

**`CombatantKind`**: `Traveler` | `Beast`

---

### `UnitReference` (sealed)
Referencia ligera a una unidad con su posición en tablero.

| Propiedad | Descripción |
|-----------|-------------|
| `Unit` | La unidad real |
| `Kind` | `CombatantKind` |
| `BoardPosition` | Índice 0-based en su slot (≥ 0) |

---

### `CombatUnitState` (sealed)
Estado mutable de una unidad durante el combate. **Nunca modifica la unidad original.**

| Propiedad | Aplica a | Descripción |
|-----------|----------|-------------|
| `UnitReference` | Ambos | Referencia a la unidad |
| `MaxHp`, `CurrentHp` | Ambos | HP |
| `IsAlive` | Ambos | Flag de vida |
| `CanActThisRound`, `CanActNextRound` | Ambos | Puede actuar |
| `MaxSp`, `CurrentSp` | Traveler | SP |
| `CurrentBp` | Traveler | Boost Points (0-5), inicia en 1 |
| `UsedBoostingThisRound` | Traveler | Flag de boost usado |
| `MaxShields`, `CurrentShields` | Beast | Escudos |
| `IsInBreakingPoint` | Beast | Sin escudos |
| `BreakingRoundsRemaining` | Beast | Rondas en break (futuro) |

---

### `TurnEntry` (sealed)
Entrada en la cola de turnos.

| Propiedad | Descripción |
|-----------|-------------|
| `UnitReference` | A quién corresponde el turno |
| `Speed` | Copia de la velocidad al crear la entrada |
| `Initiative`, `PriorityTier` | Reservado para sistema futuro |

---

### `TurnQueue` (sealed)
Cola de turnos ordenada.

| Método | Descripción |
|--------|-------------|
| `PeekFirst()` | Ver siguiente sin consumir |
| `PopFirst()` | Consumir y retornar siguiente |
| `Add(TurnEntry)` | Agregar entrada |
| `RemoveAllForUnit(UnitReference)` | Remover todos los turnos de una unidad |
| `Clone()` | Copia superficial |

---

### `TurnQueueFactory` (static)

| Método | Descripción |
|--------|-------------|
| `BuildCurrentRoundQueue(CombatFlowState)` | Filtra `IsAlive && CanActThisRound`, ordena por Speed↓, Kind (Traveler primero), BoardPosition |
| `BuildNextRoundQueue(CombatFlowState)` | Igual con `CanActNextRound` |

---

### `RoundState` (sealed)

| Miembro | Descripción |
|---------|-------------|
| `Number` | Número de ronda (≥ 1) |
| `CurrentQueue` | Turnos pendientes esta ronda |
| `NextQueue` | Turnos de la siguiente ronda |
| `ResolvedTurns` | Turnos completados esta ronda |
| `PeekCurrentTurn()` | Ver turno actual |
| `ConsumeCurrentTurn()` | Consumir turno (mueve a ResolvedTurns) |
| `ReplaceNextQueue(TurnQueue)` | Reemplazar cola siguiente |

---

### `CombatFlowState` (sealed) — núcleo del combate

Maneja todo el estado mutable durante la batalla.

**Propiedades clave:**
- `Phase` (`CombatPhase`) — fase actual
- `Result` (`BattleResult`) — resultado
- `CurrentRound` (`RoundState?`) — ronda activa
- `UnitStates` — colección de todos los `CombatUnitState`

**Métodos de control de ronda:**

| Método | Descripción |
|--------|-------------|
| `StartRound(currentQueue, nextQueue)` | Crea `RoundState`, pone `Phase = RoundSetup`, resetea flags |
| `PeekTurn()` | Ver turno actual |
| `CompleteTurn()` | Consumir turno; si la cola queda vacía, `Phase = RoundEnd` |
| `FinishBattle(BattleResult)` | Pone `Phase = Finished` |

**Métodos de gestión de unidades:**

| Método | Descripción |
|--------|-------------|
| `GetUnitState(UnitReference)` | Lookup del estado (lanza si no existe) |
| `ApplyDamage(target, damage)` | Reduce HP; auto-mata si llega a 0; retorna HP nuevo |
| `MarkUnitAsDead(UnitReference)` | `IsAlive=false`, `CurrentHp=0`, quita de colas |
| `GetAliveTravelers()` | Lista ordenada por `BoardPosition` |
| `GetAliveBeasts()` | Lista ordenada por `BoardPosition` |

**Display:**

| Método | Descripción |
|--------|-------------|
| `BuildViewSnapshot()` | Crea `CombatViewSnapshot` para la UI |
| `BuildSlotLabel(int)` | Convierte 0→"A", 1→"B", etc. |

---

### `CombatViewSnapshot` — datos para UI

**`UnitDisplaySnapshot`** — datos de display de una unidad:
- `BoardSlot` (string), `Name`, `Kind`, `CurrentHp/MaxHp`, `CurrentSp/MaxSp`, `CurrentBp`, `CurrentShields`

**`CombatViewSnapshot`** — snapshot completo de la batalla:
- `RoundNumber`, `PlayerTeam`, `EnemyTeam`, `CurrentRoundTurns`, `NextRoundTurns`
