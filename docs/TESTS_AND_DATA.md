# Tests & Data Layer

---

## Tests (`Octopath-Traveler.Tests`)

Namespace: `Octopath_Traveler.Tests`
Framework: **xUnit** con `[Theory] + [MemberData]`

### Cómo funciona un test

```
RunTest(teamsFolder, testFile)
  1. View.BuildTestingView(testFile)    → carga el script esperado + extrae inputs
  2. new Game(view, teamsFolder).Play() → ejecuta el juego completo con inputs del script
  3. view.GetScript()                   → extrae el output real producido
  4. CompareScripts(actual, expected)  → Assert.Equal línea a línea
```

`CompareScripts` prefija cada aserción con `[L{N}]` para facilitar diagnóstico.

---

### Grupos de tests

Los tests se parametrizan automáticamente por carpeta/archivo usando `MemberData`.

| Test | Carpeta de datos | Qué prueba |
|------|-----------------|------------|
| `TestE1_BasicCombat` | `E1-BasicCombat` | Mecánica de ataque básico |
| `TestE1_InvalidTeams` | `E1-InvalidTeams` | Validación de equipos inválidos |
| `TestE1_RandomBasicCombat` | `E1-RandomBasicCombat` | Combinaciones variadas de equipos |
| `TestE2_BeastsSkills` | `E2-BeastsSkills` | Habilidades de bestias |
| `TestE2_DefendAndBreakingPoint` | `E2-DefendAndBreakingPoint` | Defensa y sistema de break |
| `TestE2_OffensiveSkills` | `E2-OffensiveSkills` | Habilidades ofensivas |
| `TestE2_HealingAndQueueSkills` | `E2-HealingAndQueueSkills` | Curación y manipulación de cola |
| `TestE2_BaseStatsPassives` | `E2-BaseStatsPassives` | Pasivas de estadísticas base |
| `TestE2_Mix` | `E2-Mix` | Mecánicas mixtas |
| `TestE2_Random` | `E2-Random` | Escenarios aleatorios |
| `TestE3_BasicAttackBoosting` | `E3-BasicAttackBoosting` | BP con ataque básico |
| `TestE3_BasicAttackBoostingRandom` | `E3-BasicAttackBoostingRandom` | Escenarios aleatorios con BP |
| `TestE3_BasicPassives` | `E3-BasicPassives` | Mecánica de pasivas |
| `TestE3_BasicPassivesRandom` | `E3-BasicPassivesRandom` | Pasivas en escenarios aleatorios |

> **Nota:** Los tests E2 y E3 están definidos pero sus carpetas de datos aún no existen (features pendientes de implementar).

---

### Formato de archivos de test (`.txt`)

Cada archivo contiene la sesión completa de I/O:

```
[línea de output del juego]
INPUT: [valor que ingresó el usuario]
[siguiente línea de output]
...
```

`TestingView` extrae las líneas `INPUT:` como inputs y el resto como output esperado.

---

## Data Layer (`data/`)

### `characters.json` — Personajes jugables

```json
{
  "Name": "Olberic",
  "Stats": {
    "HP": 5781, "SP": 291,
    "PhysAtk": 434, "PhysDef": 378,
    "ElemAtk": 237, "ElemDef": 271,
    "Speed": 235
  },
  "Weapons": ["Sword", "Spear"]
}
```

**Personajes disponibles (8):** Olberic, Cyrus, Tressa, Ophilia, Primrose, Alfyn, Therion, H'aanit

---

### `enemies.json` — Enemigos

```json
{
  "Name": "Meep",
  "Stats": {
    "HP": 1016,
    "PhysAtk": 399, "PhysDef": 432,
    "ElemAtk": 419, "ElemDef": 254,
    "Speed": 207
  },
  "Skill": "Attack",
  "Shields": 2,
  "Weaknesses": ["Bow", "Stave", "Dark"]
}
```

Sin campo `SP`. `Weaknesses` son tipos de arma o elemento.

---

### `skills.json` — Habilidades activas

```json
{
  "Name": "Heal Wounds",
  "SP": 6,
  "Type": "Light",
  "Description": "Restaura HP a todos las unidades del grupo.",
  "Target": "Party",
  "Modifier": 1.5,
  "Boost": "Aumenta el modificador en 0.5 por cada BP"
}
```

Campos: `Name`, `SP` (costo), `Type` (elemento), `Target` (Single/Party/Ally/Enemies), `Modifier` (multiplicador), `Boost` (descripción de escala con BP).

> Solo se valida existencia en catálogo; no están implementadas en combate.

---

### `passive_skills.json` — Habilidades pasivas

```json
{
  "Name": "Inner Strength",
  "Description": "Aumenta en 50 el SP máximo de la unidad que porte la habilidad",
  "Target": "User"
}
```

**Pasivas disponibles:**
| Nombre | Efecto |
|--------|--------|
| Inner Strength | +50 SP máximo |
| Elemental Augmentation | +50 ElemAtk |
| Summon Strength | +50 PhysAtk |
| Hale and Hearty | +500 HP |
| Fleefoot | +50 Speed |
| Patience | Turno extra si HP y SP son ambos pares al fin de ronda |

> Solo se valida existencia en catálogo; no están implementadas en combate.

---

### `beast_skills.json` — Habilidades de bestias

```json
{
  "Name": "Attack",
  "Modifier": 1.3,
  "Description": "Realiza un ataque físico al viajero con mayor HP",
  "Target": "Single",
  "Hits": 1
}
```

Campos: `Name`, `Modifier`, `Target` (Single/Enemies), `Hits` (número de instancias de daño).

> Definidas pero no utilizadas; las bestias actualmente solo hacen ataque básico.

---

## Tipos de arma y elementos válidos

Los siguientes strings aparecen en `Weapons` y `Weaknesses`:

**Armas:** `Sword`, `Spear`, `Dagger`, `Axe`, `Bow`, `Staff`, `Stave`, `Claws`

**Elementos:** `Fire`, `Ice`, `Wind`, `Lightning`, `Light`, `Dark`
