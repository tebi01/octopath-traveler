# Octopath Traveler Combat Simulator — Project Overview

Simulador de combate por turnos inspirado en Octopath Traveler, implementado en C# con arquitectura MVC.

## Estructura de carpetas

```
Octopath-Traveler-Model/       → Dominio: entidades, estado de combate
Octopath-Traveler-Controller/  → Lógica de juego, flujo de combate
Octopath-Traveler-View/        → I/O: consola, testing automatizado
Octopath-Traveler.Tests/       → Suite Xunit parametrizada
data/                          → Catálogos JSON + archivos de equipo
docs/                          → Esta documentación
```

## Flujo de datos general

```
Archivo equipo (.txt)
        ↓
TeamsBuilder → ParseTeamFile → ValidateTeam → Build Traveler/Beast
        ↓
GameState (PlayerTeam + EnemyTeam + BattleBoard + CombatFlowState)
        ↓
Game.Play() → PrepareRound → ExecuteRoundTurns → ResolveTurn
        ↓
MainConsoleView (muestra estado, solicita acciones)
        ↓
View (loguea todo el I/O → sirve para tests)
```

## Archivos de configuración de equipo

Formato de `teams.txt`:
```
Player Team
TravelerName (ActiveSkill1, ActiveSkill2) [PassiveSkill1]
...
Enemy Team
BeastName
...
```

- Sección `Player Team` primero, luego `Enemy Team`
- Las habilidades activas van entre `()`, las pasivas entre `[]`
- Máximo 4 travelers, máximo 5 beasts

## Documentación por capa

| Capa | Archivo |
|------|---------|
| Model | [MODEL.md](MODEL.md) |
| Controller | [CONTROLLER.md](CONTROLLER.md) |
| View | [VIEW.md](VIEW.md) |
| Tests & Data | [TESTS_AND_DATA.md](TESTS_AND_DATA.md) |

## Estado de implementación

| Feature | Estado |
|---------|--------|
| Ataque básico (Traveler) | ✅ Implementado |
| Ataque básico (Beast) | ✅ Implementado |
| Defender | ✅ Implementado |
| Huir | ✅ Implementado (derrota) |
| Sistema de Boost Points (BP) | ✅ Parcial (recarga, selección) |
| Breaking Point (shields) | 🔲 Definido en modelo, no aplicado |
| Habilidades activas | 🔲 Definidas en JSON, no implementadas |
| Habilidades pasivas | 🔲 Definidas en JSON, no implementadas |
| Habilidades de bestias | 🔲 Definidas en JSON, no implementadas |
