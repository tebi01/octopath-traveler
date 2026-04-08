# View Layer

Namespace: `Octopath_Traveler_View`

---

## Jerarquía de clases

```
View (facade pública)
 └─ AbstractView (base abstracta)
      ├─ ConsoleView        → I/O real por consola
      └─ TestingView        → I/O automático por script
           └─ ManualTestingView  → semiauto con feedback visual

MainConsoleView             → UI del juego (usa View internamente)
```

---

## `View` — Facade pública

Único punto de entrada para el I/O del juego.

**Factories estáticas:**

| Método | Descripción |
|--------|-------------|
| `BuildConsoleView()` | Crea vista interactiva por consola |
| `BuildTestingView(pathTestScript)` | Crea vista automatizada para tests |
| `BuildManualTestingView(pathTestScript)` | Tests con feedback visual (verde/rojo) |

**Métodos delegados a `AbstractView`:**

| Método | Descripción |
|--------|-------------|
| `ReadLine()` | Lee entrada del usuario |
| `WriteLine(message)` | Escribe línea de salida |
| `GetScript()` | Retorna todo el I/O acumulado como `string[]` |

---

## `AbstractView` — Base abstracta

Acumula todo el I/O en un `Script` interno.

| Método | Descripción |
|--------|-------------|
| `WriteLine(object text)` | Añade texto + `\n` al script |
| `Write(object text)` | Virtual; añade texto sin newline |
| `ReadLine()` | Llama `GetNextInput()` y registra en script |
| `GetNextInput()` | **Abstracto** — implementado por subclases |
| `ExportScript(string path)` | Escribe el script a archivo |
| `GetScript()` | Retorna líneas del script |

---

## `ConsoleView` : AbstractView

Vista interactiva real.

| Override | Comportamiento |
|----------|----------------|
| `Write(object text)` | Escribe en script Y en `Console` |
| `GetNextInput()` | `Console.ReadLine()` con prompt `"INPUT: "` |

---

## `TestingView` : AbstractView

Vista automatizada para tests; lee inputs de un archivo de script.

| Campo | Descripción |
|-------|-------------|
| `_expectedScript` | Líneas esperadas del archivo de test |
| `_inputsFromUser` | `Queue<string>` con los inputs extraídos |

| Override | Comportamiento |
|----------|----------------|
| `GetNextInput()` | `Dequeue()` del próximo input; lanza si la cola está vacía |

---

## `ManualTestingView` : TestingView

Tests semimanuales con comparación visual línea a línea.

| Campo | Descripción |
|-------|-------------|
| `_currentLine` | Línea actual siendo verificada |
| `_isOutputCorrectSoFar` | `false` si ya hubo una discrepancia |

| Override | Comportamiento |
|----------|----------------|
| `Write(object text)` | Compara contra línea esperada; imprime en **verde** si coincide, **rojo** + mensaje si no |
| `GetNextInput()` | Si hay discrepancia, pide input manual; si no, usa el del script |

**Helpers:**

| Método | Descripción |
|--------|-------------|
| `CheckIfCurrentOutputIsAsExpected(text)` | Compara normalized con línea esperada |
| `IsThisLineDifferentFromTheExpectedValue(line)` | Comparación de línea |
| `GetExpectedLine()` | Retorna la línea esperada o `"[EndOfFile]"` si pasó el fin |

---

## `MainConsoleView` — UI del juego

Gestiona toda la interacción con el usuario durante la partida. Usa `View` internamente.

**Constructor:** `MainConsoleView(View view, string teamsFolder)`

---

### Selección de equipo

| Método | Descripción |
|--------|-------------|
| `SelectTeamInfo()` | Lista archivos de equipo disponibles, retorna `TeamsInfo` con la ruta seleccionada |
| `ShowInvalidTeamMessage()` | Muestra error de equipo inválido |

---

### Display de combate

| Método | Descripción |
|--------|-------------|
| `ShowRoundStart(roundNumber)` | Encabezado de inicio de ronda |
| `ShowCombatStatus(snapshot, includeLeadingSeparator)` | Muestra estado completo: equipo jugador, equipo enemigo, colas de turno |
| `ShowPlayerTeam(snapshots)` | Tabla: Slot \| Nombre \| HP \| SP \| BP |
| `ShowEnemyTeam(snapshots)` | Tabla: Slot \| Nombre \| HP \| Escudos |
| `ShowRoundQueues(snapshot)` | Orden de turno ronda actual y siguiente |

---

### Acciones del jugador

| Método | Retorna | Descripción |
|--------|---------|-------------|
| `AskTravelerMainAction(travelerName)` | `int 1-4` | 1=Ataque, 2=Habilidad, 3=Defender, 4=Huir |
| `AskWeaponSelection(weapons)` | `int 1..n+1` | Selección de arma; `n+1` = cancelar |
| `AskTravelerTarget(travelerName, enemies)` | `int` | Selección de objetivo enemigo |
| `AskTravelerSkill(travelerName, activeSkills)` | `int` | Selección de habilidad activa |
| `AskBoostPointsToUse()` | `int ≥ 0` | BP a gastar en este ataque |

---

### Resultados de combate

| Método | Descripción |
|--------|-------------|
| `ShowTravelerAttackResult(TravelerAttackViewData)` | Muestra daño infligido por traveler |
| `ShowBeastAttackResult(BeastAttackViewData)` | Muestra daño infligido por beast |
| `ShowFleeMessage()` | Mensaje de huida |
| `ShowPlayerWinMessage()` | Mensaje de victoria |
| `ShowEnemyWinMessage()` | Mensaje de derrota |

---

### Helpers de I/O

| Método | Descripción |
|--------|-------------|
| `AskOptionWithCancel(title, options)` | Muestra lista numerada con opción de cancelar |
| `ReadOption(min, max)` | Lee entero validado en rango |
| `ReadNonNegativeInt()` | Lee entero ≥ 0 |
| `PrintSeparator()` | Imprime línea divisoria |

---

## Tipos auxiliares

### `TravelerAttackViewData` (record)
```
string AttackerName, string TargetName, string WeaponType, int Damage, int TargetCurrentHp
```

### `BeastAttackViewData` (record)
```
string BeastName, string TargetName, int Damage, int TargetCurrentHp
```

### `TeamsInfo`
- `TeamFilePath` (string) — ruta al archivo de equipo seleccionado

### `Script`
Acumulador interno de todo el I/O.

| Método | Descripción |
|--------|-------------|
| `AddInput(string)` | Agrega `"INPUT: {input}\n"` |
| `AddToScript(string)` | Agrega texto sin formato |
| `GetScript()` | Retorna string completo acumulado |
| `ExportScript(string path)` | Escribe a archivo |

### `InvalidInputRequestException`
Excepción custom lanzada por `ManualTestingView` cuando se solicita un input inesperado (discrepancia detectada).
