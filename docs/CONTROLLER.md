# Controller Layer

Namespace: `Octopath_Traveler`

---

## `Game` — Controlador principal

Orquesta el flujo completo de la partida.

**Constructor:** `Game(View view, string teamsFolder)`

---

### Flujo principal

```
Play()
 ├─ SelectTeamInfo()          → pide al usuario que elija archivo de equipo
 ├─ TeamsBuilder.Build()      → carga y valida el equipo
 └─ RunCombat()
      └─ bucle mientras batalla en curso:
           ├─ PrepareRound()
           └─ ExecuteRoundTurns()
```

---

### Métodos de flujo

| Método | Descripción |
|--------|-------------|
| `Play()` | Loop principal. Captura excepciones y muestra mensaje de error |
| `RunCombat()` | Bucle de rondas hasta `BattleResult != Ongoing` |
| `PrepareRound(combatState, roundNumber)` | Si ronda > 1: recarga BP. Construye colas. Llama `StartRound`. Muestra estado |
| `ExecuteRoundTurns(combatState)` | Itera turnos: `ResolveTurn` → evalúa resultado → muestra estado |

---

### Resolución de turnos

| Método | Descripción |
|--------|-------------|
| `ResolveTurn(combatState, currentTurn)` | Despacha a `ResolveTravelerTurn` o `ResolveBeastTurn` |
| `ResolveTravelerTurn(combatState, travelerTurn)` | Crea `TravelerTurnContext`. Loop hasta completar turno: 1=Ataque, 2=Habilidad, 3=Defender, 4=Huir |
| `ResolveBeastTurn(combatState, beastTurn)` | `SelectBeastTarget` → `ExecuteBeastAttack` → `CompleteTurn` |

---

### Acciones de Traveler

| Método | Descripción |
|--------|-------------|
| `TryHandleTravelerBasicAttack(context)` | `SelectWeapon` → `SelectTarget` → `AskBoostPoints` → `ExecuteAttack`. Retorna `false` si se cancela |
| `TryResolveTravelerBasicAttack(context)` | Resolución completa con cálculo de daño. Retorna `false` si se cancela |
| `ExecuteTravelerBasicAttack(context)` | `CalculateTravelerAttackDamage` → `ApplyDamage` → muestra resultado |

---

### Cálculo de daño

```csharp
// Daño físico
int damage = Math.Max(0, (int)Math.Floor(attackerPhysAtk * modifier - targetPhysDef));

// Modificador base de ataque básico
const double BasicAttackModifier = 1.3;
```

**`CalculatePhysicalDamage(attackPhysAtk, targetPhysDef, modifier)`**: retorna `max(0, floor(atk * mod - def))`

---

### Selección de objetivos

| Método | Descripción |
|--------|-------------|
| `SelectBeastTarget(combatState)` | Elige traveler vivo con mayor HP; en empate, menor `BoardPosition` |
| `EvaluateBattleResult(combatState)` | `PlayerVictory` si no hay beasts vivos; `PlayerDefeat` si no hay travelers vivos; `Ongoing` en otro caso |

---

### Helpers de combate

| Método | Descripción |
|--------|-------------|
| `RechargeBpForAliveTravelers(combatState)` | Añade +1 BP a cada traveler vivo (máximo 5) |
| `BuildEnemySnapshot(combatState, unitRef)` | Crea `UnitDisplaySnapshot` de un beast para mostrar en UI |

---

## `TeamsBuilder` — Constructor de equipos

Lee, parsea y valida el archivo de equipo, luego crea el `GameState`.

**Constructor:** `TeamsBuilder(TeamsInfo teamsInfo)`

---

### Pipeline `Build()`

```
LoadCatalogs()          → lee characters.json, enemies.json, skills.json, passive_skills.json
LoadAndValidateTeam()   → ParseTeamFile + ValidateTeam
BuildPlayerTeam()       → crea List<Traveler>
BuildEnemyTeam()        → crea List<Beast>
→ return new GameState(playerTeam, enemyTeam)
```

---

### Carga de catálogos

| Método | Descripción |
|--------|-------------|
| `LoadCatalogs()` | Carga todos los JSON; retorna `Catalogs` con diccionarios indexados por nombre |
| `ReadCatalog<T>(dataFolder, fileName)` | Lee y deserializa un JSON |
| `ReadJson<T>(path)` | Deserializa JSON con nombres de propiedad insensibles a mayúsculas |

---

### Parseo de archivo de equipo

Formato esperado:
```
Player Team
TravelerName (ActiveSkill1, ActiveSkill2) [PassiveSkill1]
Enemy Team
BeastName
```

| Método | Descripción |
|--------|-------------|
| `ParseTeamFile(string[] lines)` | Normaliza líneas, encuentra headers, extrae secciones |
| `ParseTravelerLine(string line)` | Parsea `Nombre (activas) [pasivas]` |
| `ExtractTravelerSections(string line)` | Divide por nombre, contenido `()` y contenido `[]`; valida corchetes |
| `SplitSkills(string?)` | CSV parser para habilidades |

---

### Validación de equipo

| Método | Qué valida |
|--------|------------|
| `ValidateTeam(parsedTeam, catalogs)` | Orquesta todas las validaciones |
| `ValidateTeamSize()` | Travelers: [1,4], Beasts: [1,5] |
| `ValidateTeamDuplicates()` | Sin nombres repetidos en cada equipo |
| `ValidateTravelers()` | Existencia, conteo de habilidades, duplicados, validez en catálogo |
| `ValidateBeasts()` | Existencia en catálogo |
| `ValidateTravelerSkillCounts()` | Activas ≤ 8, pasivas ≤ 4 |
| `ValidateTravelerSkillsExist()` | Cada habilidad existe en catálogo |

---

### Creación de unidades

| Método | Descripción |
|--------|-------------|
| `BuildTraveler(parsedLine, catalogs)` | Lookup en catálogo → crea `Traveler` con stats y habilidades del archivo |
| `BuildBeast(beastName, catalogs)` | Lookup en catálogo → crea `Beast` con stats, skill, shields, weaknesses |
| `BuildCharacterCombatStats(CharacterDto)` | `CharacterDto` → `CombatStats` |
| `BuildCharacterSkillPoints(CharacterDto)` | `CharacterDto` → `SkillPoints` |
| `BuildEnemyCombatStats(EnemyDto)` | `EnemyDto` → `CombatStats` |

---

### Tipos internos de `TeamsBuilder`

**Records:**

| Tipo | Campos |
|------|--------|
| `ParsedTeam` | `IReadOnlyList<ParsedTravelerLine> Travelers`, `IReadOnlyList<string> Beasts` |
| `ParsedTravelerLine` | `string Name`, `IReadOnlyList<string> ActiveSkills`, `IReadOnlyList<string> PassiveSkills` |
| `TravelerSections` | `string Name`, `string? ActiveSkillsContent`, `string? PassiveSkillsContent` |
| `Catalogs` | `Dictionary<string,CharacterDto> Characters`, `Dictionary<string,EnemyDto> Enemies`, `HashSet<string> ActiveSkills`, `HashSet<string> PassiveSkills` |

**DTOs (de JSON):**

| DTO | Campos relevantes |
|-----|-------------------|
| `CharacterDto` | `Name`, `Stats` (CharacterStatsDto), `Weapons` (List<string>) |
| `CharacterStatsDto` | `HP`, `SP`, `PhysAtk`, `PhysDef`, `ElemAtk`, `ElemDef`, `Speed` |
| `EnemyDto` | `Name`, `Stats` (EnemyStatsDto), `Skill`, `Shields`, `Weaknesses` |
| `EnemyStatsDto` | igual que CharacterStatsDto pero sin `SP` |
| `SkillDto` | `Name` |

---

## `TurnContexts` — Contextos de turno

Agrupan los datos necesarios para resolver un turno específico.

### `TravelerTurnContext`
| Propiedad | Tipo |
|-----------|------|
| `CombatState` | `CombatFlowState` |
| `TravelerTurn` | `TurnEntry` |
| `Traveler` | `Traveler` (cast validado del unit) |

### `TravelerBasicAttackContext`
| Propiedad | Tipo |
|-----------|------|
| `TravelerTurnContext` | `TravelerTurnContext` |
| `WeaponType` | `string` (no vacío) |
| `TargetBeast` | `UnitReference` |

### `BeastTurnContext`
| Propiedad | Tipo |
|-----------|------|
| `CombatState` | `CombatFlowState` |
| `BeastTurn` | `TurnEntry` |
| `Beast` | `Beast` (cast validado del unit) |
| `TargetTraveler` | `UnitReference` |
