using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class Game
{
    private const double BasicAttackModifier = 1.3;
    private const string SpearheadSkillName = "Spearhead";
    private const string LegholdTrapSkillName = "Leghold Trap";
    private const int SpearheadSpCost = 6;
    private const int LegholdTrapSpCost = 6;
    private const double SpearheadModifier = 1.5;
    private const string NightmareChimeraSkillName = "Nightmare Chimera";
    private const string ShootingStarsSkillName = "Shooting Stars";

    private static readonly IReadOnlyList<string> PhysicalSkillWeapons = new[]
    {
        "Sword", "Spear", "Dagger", "Axe", "Bow", "Stave"
    };

    private readonly MainConsoleView _view;
    private readonly BeastSkillCatalog _beastSkillCatalog = BeastSkillCatalog.LoadDefault();
    private GameState _state = new();

    public Game(View view, string teamsFolder)
    {
        _view = new MainConsoleView(view, teamsFolder);
    }

    public void Play()
    {
        try
        {
            var teamInfo = _view.SelectTeamInfo();
            var teamBuilder = new TeamsBuilder(teamInfo);
            _state = teamBuilder.Build();
            RunCombat();
        }
        catch (Exception)
        {
            _view.ShowInvalidTeamMessage();
        }
    }

    private void RunCombat()
    {
        var combatState = GetCombatState();
        var roundNumber = 1;

        while (IsBattleOngoing(combatState))
        {
            PrepareRound(combatState, roundNumber);
            ExecuteRoundTurns(combatState);
            roundNumber++;
        }
    }

    private CombatFlowState GetCombatState()
    {
        return _state.CombatFlow ?? throw new InvalidOperationException("Combat flow is not initialized.");
    }

    private static bool IsBattleOngoing(CombatFlowState combatState)
    {
        return combatState.Result == BattleResult.Ongoing;
    }

    private void PrepareRound(CombatFlowState combatState, int roundNumber)
    {
        RechargeBpIfNeeded(combatState, roundNumber);
        UpdateBreakingPointStates(combatState);
        UpdatePriorityModifiers(combatState);
        StartRoundQueues(combatState);
        _view.ShowRoundStart(roundNumber);
        _view.ShowCombatStatus(combatState.BuildViewSnapshot());
    }

    private static void RechargeBpIfNeeded(CombatFlowState combatState, int roundNumber)
    {
        if (roundNumber > 1)
        {
            RechargeBpForAliveTravelers(combatState);
        }
    }

    private static void StartRoundQueues(CombatFlowState combatState)
    {
        var currentRoundQueue = BuildCurrentRoundQueueForRoundStart(combatState);
        ConsumeRoundPriorityFlags(combatState);
        var nextRoundQueue = TurnQueueFactory.BuildNextRoundQueue(combatState);
        combatState.StartRound(currentRoundQueue, nextRoundQueue);
    }

    private static TurnQueue BuildCurrentRoundQueueForRoundStart(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is null)
        {
            return TurnQueueFactory.BuildCurrentRoundQueue(combatState);
        }

        return combatState.CurrentRound.NextQueue.Clone();
    }

    private void ExecuteRoundTurns(CombatFlowState combatState)
    {
        while (HasPendingTurns(combatState) && IsBattleOngoing(combatState))
        {
            var currentTurn = GetCurrentTurn(combatState);
            ResolveTurn(combatState, currentTurn);
            RefreshProjectedNextRoundQueue(combatState);

            if (!IsBattleOngoing(combatState))
            {
                break;
            }

            if (TryFinishBattle(combatState))
            {
                break;
            }

            ShowRoundStateAfterTurn(combatState);
        }
    }

    private static bool HasPendingTurns(CombatFlowState combatState)
    {
        return combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty;
    }

    private static TurnEntry GetCurrentTurn(CombatFlowState combatState)
    {
        return combatState.PeekTurn() ?? throw new InvalidOperationException("Round queue is inconsistent.");
    }

    private void ResolveTurn(CombatFlowState combatState, TurnEntry currentTurn)
    {
        if (currentTurn.UnitReference.Kind == CombatantKind.Traveler)
        {
            ResolveTravelerTurn(combatState, currentTurn);
            return;
        }

        ResolveBeastTurn(combatState, currentTurn);
    }

    private bool TryFinishBattle(CombatFlowState combatState)
    {
        var battleOutcome = EvaluateBattleResult(combatState);
        if (battleOutcome == BattleResult.Ongoing)
        {
            return false;
        }

        combatState.FinishBattle(battleOutcome);
        ShowWinner(battleOutcome);
        return true;
    }

    private void ShowRoundStateAfterTurn(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty)
        {
            _view.ShowCombatStatus(combatState.BuildViewSnapshot(), includeLeadingSeparator: true);
        }
    }

    private void ResolveTravelerTurn(CombatFlowState combatState, TurnEntry travelerTurn)
    {
        var travelerTurnContext = new TravelerTurnContext(combatState, travelerTurn);

        while (IsBattleOngoing(travelerTurnContext.CombatState))
        {
            var selectedAction = AskTravelerAction(travelerTurnContext);
            if (TryHandleTravelerAction(travelerTurnContext, selectedAction))
            {
                return;
            }
        }
    }

    private int AskTravelerAction(TravelerTurnContext travelerTurnContext)
    {
        return _view.AskTravelerMainAction(travelerTurnContext.Traveler.Name);
    }

    private bool TryHandleTravelerAction(TravelerTurnContext travelerTurnContext, int selectedAction)
    {
        return selectedAction switch
        {
            1 => TryHandleTravelerBasicAttack(travelerTurnContext),
            2 => ShowTravelerSkills(travelerTurnContext),
            3 => DefendTraveler(travelerTurnContext),
            4 => TravelerFlees(travelerTurnContext),
            _ => false
        };
    }

    private bool TryHandleTravelerBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        if (!TryResolveTravelerBasicAttack(travelerTurnContext))
        {
            return false;
        }

        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private bool ShowTravelerSkills(TravelerTurnContext travelerTurnContext)
    {
        var availableSkills = GetAvailableSkillsForTraveler(travelerTurnContext);
        var selectedSkill = _view.AskTravelerSkill(travelerTurnContext.Traveler.Name, availableSkills);
        if (selectedSkill == availableSkills.Count + 1)
        {
            return false;
        }

        var skillName = availableSkills[selectedSkill - 1];
        return skillName switch
        {
            SpearheadSkillName => TryResolveSpearhead(travelerTurnContext),
            LegholdTrapSkillName => TryResolveLegholdTrap(travelerTurnContext),
            NightmareChimeraSkillName => TryResolveNightmareChimera(travelerTurnContext),
            ShootingStarsSkillName => TryResolveShootingStars(travelerTurnContext),
            "Heal Wounds" => TryResolvePartyHeal(travelerTurnContext, "Heal Wounds", 6, 1.5),
            "Heal More" => TryResolvePartyHeal(travelerTurnContext, "Heal More", 25, 2.0),
            "First Aid" => TryResolveFirstAid(travelerTurnContext),
            "Revive" => TryResolveRevive(travelerTurnContext),
            "Vivify" => TryResolveVivify(travelerTurnContext),
            _ => TryResolveSingleTargetOffensiveSkill(travelerTurnContext, skillName)
                || TryResolveEnemiesTargetOffensiveSkill(travelerTurnContext, skillName)
        };
    }

    private static IReadOnlyList<string> GetAvailableSkillsForTraveler(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        return travelerTurnContext.Traveler.ActiveSkills
            .Where(skill => GetSkillSpCost(skill) <= travelerState.CurrentSp)
            .ToList();
    }

    private static int GetSkillSpCost(string skillName)
    {
        return skillName switch
        {
            "Holy Light" => 6,
            "Tradewinds" => 7,
            "Cross Strike" => 12,
            "Moonlight Waltz" => 7,
            "Icicle" => 7,
            "Amputation" => 8,
            "Wildfire" => 7,
            "True Strike" => 10,
            "Thunderbird" => 7,
            "Mercy Strike" => 4,
            "Qilin's Horn" => 35,
            "Phoenix Storm" => 35,
            "Fireball" => 8,
            "Icewind" => 8,
            "Lightning Bolt" => 8,
            "Luminescence" => 9,
            "Trade Tempest" => 10,
            "Level Slash" => 9,
            "Night Ode" => 10,
            "Tiger Rage" => 35,
            "Yatagarasu" => 35,
            "Fox Spirit" => 35,
            "Last Stand" => 16,
            SpearheadSkillName => 6,
            LegholdTrapSkillName => 6,
            NightmareChimeraSkillName => 35,
            ShootingStarsSkillName => 35,
            "Heal Wounds" => 6,
            "Heal More" => 25,
            "First Aid" => 4,
            "Revive" => 50,
            "Vivify" => 16,
            _ => 0
        };
    }

    private bool TryResolvePartyHeal(TravelerTurnContext travelerTurnContext, string skillName, int spCost, double modifier)
    {
        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, spCost))
        {
            return false;
        }

        var casterReference = travelerTurnContext.TravelerTurn.UnitReference;
        var targets = GetOrderedTravelerTargets(travelerTurnContext.CombatState, casterReference, includeDead: false)
            .Where(target => target is not null)
            .Cast<UnitReference>()
            .ToList();

        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * modifier));
        var healingResults = new List<TravelerHealingTargetViewData>();
        foreach (var target in targets)
        {
            var targetState = travelerTurnContext.CombatState.GetUnitState(target);
            targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);
            healingResults.Add(new TravelerHealingTargetViewData(target.Unit.Name, recoveredAmount, targetState.CurrentHp));
        }

        _view.ShowTravelerHealingSkillResult(new TravelerHealingSkillViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            healingResults));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveFirstAid(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = TrySelectAllyTarget(travelerTurnContext, includeDead: false);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, 4))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * 1.5));
        targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);

        _view.ShowTravelerHealingSkillResult(new TravelerHealingSkillViewData(
            travelerTurnContext.Traveler.Name,
            "First Aid",
            new[] { new TravelerHealingTargetViewData(selectedTarget.Unit.Name, recoveredAmount, targetState.CurrentHp) }));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveRevive(TravelerTurnContext travelerTurnContext)
    {
        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, 50))
        {
            return false;
        }

        var casterReference = travelerTurnContext.TravelerTurn.UnitReference;
        var deadTargets = GetOrderedTravelerTargets(travelerTurnContext.CombatState, casterReference, includeDead: true)
            .Where(target => target is not null)
            .Cast<UnitReference>()
            .ToList();

        var reviveResults = new List<TravelerReviveTargetViewData>();
        foreach (var target in deadTargets)
        {
            var targetState = travelerTurnContext.CombatState.GetUnitState(target);
            if (targetState.IsAlive)
            {
                continue;
            }

            targetState.IsAlive = true;
            targetState.CurrentHp = 1;
            targetState.CanActThisRound = false;
            targetState.CanActNextRound = true;
            ResetRevivedTravelerRoundFlags(targetState);
            reviveResults.Add(new TravelerReviveTargetViewData(target.Unit.Name, IsRevived: true, RecoveredHp: 0, TargetCurrentHp: 1));
        }

        _view.ShowTravelerReviveSkillResult(new TravelerReviveSkillViewData(
            travelerTurnContext.Traveler.Name,
            "Revive",
            reviveResults));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveVivify(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = TrySelectAllyTarget(travelerTurnContext, includeDead: true);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, 16))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var wasDead = !targetState.IsAlive;
        if (wasDead)
        {
            targetState.IsAlive = true;
            targetState.CurrentHp = 1;
            targetState.CanActThisRound = false;
            targetState.CanActNextRound = true;
            ResetRevivedTravelerRoundFlags(targetState);
        }

        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * 1.5));
        targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);

        _view.ShowTravelerReviveSkillResult(new TravelerReviveSkillViewData(
            travelerTurnContext.Traveler.Name,
            "Vivify",
            new[]
            {
                new TravelerReviveTargetViewData(
                    selectedTarget.Unit.Name,
                    IsRevived: wasDead,
                    RecoveredHp: recoveredAmount,
                    TargetCurrentHp: targetState.CurrentHp)
            }));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private UnitReference? TrySelectAllyTarget(TravelerTurnContext travelerTurnContext, bool includeDead)
    {
        var allTravelers = travelerTurnContext.CombatState.UnitStates
            .Where(state => state.UnitReference.Kind == CombatantKind.Traveler)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .ToList();

        var candidates = allTravelers
            .Where(state => includeDead ? !state.IsAlive : state.IsAlive)
            .Select(state => state.UnitReference)
            .ToList();

        if (candidates.Count == 0)
        {
            _ = _view.AskAllyTarget(travelerTurnContext.Traveler.Name, Array.Empty<UnitDisplaySnapshot>());
            return null;
        }

        var snapshots = candidates
            .Select(reference => BuildEnemySnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTarget = _view.AskAllyTarget(travelerTurnContext.Traveler.Name, snapshots);
        if (selectedTarget == snapshots.Count + 1)
        {
            return null;
        }

        return candidates[selectedTarget - 1];
    }

    private static IReadOnlyList<UnitReference?> GetOrderedTravelerTargets(
        CombatFlowState combatState,
        UnitReference casterReference,
        bool includeDead)
    {
        var allTravelers = combatState.UnitStates
            .Where(state => state.UnitReference.Kind == CombatantKind.Traveler)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .ToList();

        var filtered = allTravelers
            .Where(state => includeDead ? !state.IsAlive : state.IsAlive)
            .Select(state => state.UnitReference)
            .ToList();

        var nonCaster = filtered.Where(reference => !ReferenceEquals(reference.Unit, casterReference.Unit)).ToList();
        var caster = filtered.FirstOrDefault(reference => ReferenceEquals(reference.Unit, casterReference.Unit));
        if (caster is not null)
        {
            nonCaster.Add(caster);
        }

        return nonCaster;
    }

    private bool TryResolveEnemiesTargetOffensiveSkill(TravelerTurnContext travelerTurnContext, string skillName)
    {
        if (!TryGetEnemiesTargetOffensiveSkill(skillName, out var skill))
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, skill.SpCost))
        {
            return false;
        }

        var areaResults = ExecuteEnemiesTargetOffensiveSkill(travelerTurnContext, skillName, skill);
        _view.ShowTravelerSkillAreaAttackResult(new TravelerSkillAreaAttackViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            skill.DamageType,
            areaResults));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private IReadOnlyList<TravelerSkillAreaTargetViewData> ExecuteEnemiesTargetOffensiveSkill(
        TravelerTurnContext travelerTurnContext,
        string skillName,
        OffensiveSkillSpec skill)
    {
        var results = new List<TravelerSkillAreaTargetViewData>();
        var combatState = travelerTurnContext.CombatState;
        var targets = combatState.GetAliveBeasts();

        foreach (var target in targets)
        {
            var targetState = combatState.GetUnitState(target);
            var hasWeakness = HasWeaknessAgainstAttackType(target, skill.DamageType);
            var wasInBreakingPoint = targetState.IsInBreakingPoint;

            var baseDamage = skill.IsElemental
                ? CalculateElementalDamageRaw(travelerTurnContext.Traveler.Stats.ElementalAttack, target.Unit.Stats.ElementalDefense, skill.Modifier)
                : CalculatePhysicalDamageRaw(travelerTurnContext.Traveler.Stats.PhysicalAttack, target.Unit.Stats.PhysicalDefense, skill.Modifier);

            if (string.Equals(skillName, "Last Stand", StringComparison.Ordinal))
            {
                baseDamage = CalculateLastStandRawDamage(travelerTurnContext, target, skill.Modifier);
            }

            var dealtDamage = ApplyMultiplier(baseDamage, GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint));
            var enteredBreakingPoint = false;
            if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
            {
                enteredBreakingPoint = combatState.TryConsumeBeastShield(target);
            }

            var targetCurrentHp = combatState.ApplyDamage(target, dealtDamage);
            results.Add(new TravelerSkillAreaTargetViewData(
                target.Unit.Name,
                dealtDamage,
                hasWeakness,
                enteredBreakingPoint,
                targetCurrentHp));
        }

        return results;
    }

    private bool TryResolveSingleTargetOffensiveSkill(TravelerTurnContext travelerTurnContext, string skillName)
    {
        if (!TryGetSingleTargetOffensiveSkill(skillName, out var skill))
        {
            return false;
        }

        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, skill.SpCost))
        {
            return false;
        }

        ExecuteSingleTargetOffensiveSkill(travelerTurnContext, selectedTarget, skillName, skill);
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveNightmareChimera(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = _view.AskWeaponSelection(PhysicalSkillWeapons);
        if (selectedWeapon == PhysicalSkillWeapons.Count + 1)
        {
            return false;
        }

        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, 35))
        {
            return false;
        }

        var damageType = PhysicalSkillWeapons[selectedWeapon - 1];
        ExecuteSingleTargetOffensiveSkill(
            travelerTurnContext,
            selectedTarget,
            NightmareChimeraSkillName,
            new OffensiveSkillSpec(35, 1.9, damageType, IsElemental: false, IsMercyStrike: false));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveShootingStars(TravelerTurnContext travelerTurnContext)
    {
        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, 35))
        {
            return false;
        }

        var combatState = travelerTurnContext.CombatState;
        var targets = combatState.GetAliveBeasts();
        var hitTypes = new[] { "Wind", "Light", "Dark" };
        var hitResults = new List<TravelerSkillAreaHitViewData>();

        foreach (var target in targets)
        {
            foreach (var hitType in hitTypes)
            {
                var targetState = combatState.GetUnitState(target);
                var hasWeakness = HasWeaknessAgainstAttackType(target, hitType);
                var wasInBreakingPoint = targetState.IsInBreakingPoint;
                var baseDamage = CalculateElementalDamageRaw(
                    travelerTurnContext.Traveler.Stats.ElementalAttack,
                    target.Unit.Stats.ElementalDefense,
                    0.9);

                var dealtDamage = ApplyMultiplier(baseDamage, GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint));
                var enteredBreakingPoint = false;
                if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
                {
                    enteredBreakingPoint = combatState.TryConsumeBeastShield(target);
                }

                _ = combatState.ApplyDamage(target, dealtDamage);
                hitResults.Add(new TravelerSkillAreaHitViewData(
                    target.Unit.Name,
                    dealtDamage,
                    hitType,
                    hasWeakness,
                    enteredBreakingPoint));
            }
        }

        var finalHpByTarget = targets
            .Select(target => new TravelerSkillFinalHpViewData(target.Unit.Name, combatState.GetUnitState(target).CurrentHp))
            .ToList();

        _view.ShowTravelerSkillMultiHitAreaAttackResult(new TravelerSkillMultiHitAreaAttackViewData(
            travelerTurnContext.Traveler.Name,
            ShootingStarsSkillName,
            hitResults,
            finalHpByTarget));

        return CompleteTravelerTurn(travelerTurnContext);
    }

    private void ExecuteSingleTargetOffensiveSkill(
        TravelerTurnContext travelerTurnContext,
        UnitReference target,
        string skillName,
        OffensiveSkillSpec skill)
    {
        var combatState = travelerTurnContext.CombatState;
        var targetState = combatState.GetUnitState(target);
        var hasWeakness = HasWeaknessAgainstAttackType(target, skill.DamageType);
        var wasInBreakingPoint = targetState.IsInBreakingPoint;

        var baseDamage = skill.IsElemental
            ? CalculateElementalDamageRaw(travelerTurnContext.Traveler.Stats.ElementalAttack, target.Unit.Stats.ElementalDefense, skill.Modifier)
            : CalculatePhysicalDamageRaw(travelerTurnContext.Traveler.Stats.PhysicalAttack, target.Unit.Stats.PhysicalDefense, skill.Modifier);

        var dealtDamage = ApplyMultiplier(baseDamage, GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint));
        if (skill.IsMercyStrike)
        {
            dealtDamage = Math.Min(dealtDamage, Math.Max(0, targetState.CurrentHp - 1));
        }

        var enteredBreakingPoint = false;
        if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
        {
            enteredBreakingPoint = combatState.TryConsumeBeastShield(target);
        }

        var targetCurrentHp = combatState.ApplyDamage(target, dealtDamage);
        _view.ShowTravelerSkillAttackResult(new TravelerSkillAttackViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            target.Unit.Name,
            skill.DamageType,
            dealtDamage,
            hasWeakness,
            enteredBreakingPoint,
            targetCurrentHp));
    }

    private static bool TryGetSingleTargetOffensiveSkill(string skillName, out OffensiveSkillSpec skill)
    {
        switch (skillName)
        {
            case "Holy Light":
                skill = new OffensiveSkillSpec(6, 1.5, "Light", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Tradewinds":
                skill = new OffensiveSkillSpec(7, 1.5, "Wind", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Cross Strike":
                skill = new OffensiveSkillSpec(12, 1.7, "Sword", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Moonlight Waltz":
                skill = new OffensiveSkillSpec(7, 1.6, "Dark", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Icicle":
                skill = new OffensiveSkillSpec(7, 1.5, "Ice", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Amputation":
                skill = new OffensiveSkillSpec(8, 1.7, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Wildfire":
                skill = new OffensiveSkillSpec(7, 1.6, "Fire", IsElemental: true, IsMercyStrike: false);
                return true;
            case "True Strike":
                skill = new OffensiveSkillSpec(10, 2.0, "Bow", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Thunderbird":
                skill = new OffensiveSkillSpec(7, 1.6, "Lightning", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Mercy Strike":
                skill = new OffensiveSkillSpec(4, 1.5, "Bow", IsElemental: false, IsMercyStrike: true);
                return true;
            case "Qilin's Horn":
                skill = new OffensiveSkillSpec(35, 2.1, "Spear", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Phoenix Storm":
                skill = new OffensiveSkillSpec(35, 2.1, "Bow", IsElemental: false, IsMercyStrike: false);
                return true;
            default:
                skill = new OffensiveSkillSpec(0, 0, string.Empty, IsElemental: false, IsMercyStrike: false);
                return false;
        }
    }

    private static bool TryGetEnemiesTargetOffensiveSkill(string skillName, out OffensiveSkillSpec skill)
    {
        switch (skillName)
        {
            case "Fireball":
                skill = new OffensiveSkillSpec(8, 1.5, "Fire", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Icewind":
                skill = new OffensiveSkillSpec(8, 1.5, "Ice", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Lightning Bolt":
                skill = new OffensiveSkillSpec(8, 1.5, "Lightning", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Luminescence":
                skill = new OffensiveSkillSpec(9, 1.5, "Light", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Trade Tempest":
                skill = new OffensiveSkillSpec(10, 1.5, "Wind", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Level Slash":
                skill = new OffensiveSkillSpec(9, 1.5, "Sword", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Night Ode":
                skill = new OffensiveSkillSpec(10, 1.6, "Dark", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Tiger Rage":
                skill = new OffensiveSkillSpec(35, 1.9, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Yatagarasu":
                skill = new OffensiveSkillSpec(35, 1.9, "Dagger", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Fox Spirit":
                skill = new OffensiveSkillSpec(35, 1.9, "Stave", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Last Stand":
                skill = new OffensiveSkillSpec(16, 1.4, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            default:
                skill = new OffensiveSkillSpec(0, 0, string.Empty, IsElemental: false, IsMercyStrike: false);
                return false;
        }
    }

    private bool TryResolveSpearhead(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, SpearheadSpCost))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var hasWeakness = HasWeaknessAgainstAttackType(selectedTarget, "Spear");
        var wasInBreakingPoint = targetState.IsInBreakingPoint;
        var baseDamage = CalculatePhysicalDamageRaw(
            travelerTurnContext.Traveler.Stats.PhysicalAttack,
            selectedTarget.Unit.Stats.PhysicalDefense,
            SpearheadModifier);

        var damageMultiplier = GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint);
        var dealtDamage = ApplyMultiplier(baseDamage, damageMultiplier);
        var enteredBreakingPoint = false;
        if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
        {
            enteredBreakingPoint = travelerTurnContext.CombatState.TryConsumeBeastShield(selectedTarget);
        }

        var targetCurrentHp = travelerTurnContext.CombatState.ApplyDamage(selectedTarget, dealtDamage);
        _view.ShowTravelerSkillAttackResult(new TravelerSkillAttackViewData(
            travelerTurnContext.Traveler.Name,
            SpearheadSkillName,
            selectedTarget.Unit.Name,
            "Spear",
            dealtDamage,
            hasWeakness,
            enteredBreakingPoint,
            targetCurrentHp));

        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        travelerState.IncreasedPriorityRoundsRemaining = Math.Max(travelerState.IncreasedPriorityRoundsRemaining, 1);
        travelerState.PriorityModifierNextRound = 1;
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private bool TryResolveLegholdTrap(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        if (!TryConsumeTravelerSp(travelerTurnContext, LegholdTrapSpCost))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var targetHasPendingTurnThisRound = HasPendingTurnThisRound(travelerTurnContext.CombatState, selectedTarget);
        var consumesCurrentRound = !targetState.HasDecreasedPriorityThisRound;

        targetState.HasDecreasedPriorityThisRound = true;

        if (targetHasPendingTurnThisRound)
        {
            // If the target has not acted yet, apply lower priority immediately for the remaining current queue.
            ReorderRemainingCurrentRoundQueue(travelerTurnContext.CombatState, selectedTarget);
        }

        // Base duration is 2 rounds; if this round was not consumed yet, one round is spent immediately.
        targetState.DecreasedPriorityRoundsRemaining += consumesCurrentRound ? 1 : 2;
        targetState.PriorityModifierNextRound = -1;
        _view.ShowLegholdTrapResult(new LegholdTrapViewData(travelerTurnContext.Traveler.Name, selectedTarget.Unit.Name));
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private static bool HasPendingTurnThisRound(CombatFlowState combatState, UnitReference target)
    {
        return combatState.CurrentRound is not null
            && combatState.CurrentRound.CurrentQueue.Entries.Any(entry => ReferenceEquals(entry.UnitReference.Unit, target.Unit));
    }

    private static void ReorderRemainingCurrentRoundQueue(CombatFlowState combatState, UnitReference target)
    {
        if (combatState.CurrentRound is null)
        {
            return;
        }

        var currentQueue = combatState.CurrentRound.CurrentQueue;
        var targetEntry = currentQueue.Entries
            .FirstOrDefault(entry => ReferenceEquals(entry.UnitReference.Unit, target.Unit));
        if (targetEntry is null)
        {
            return;
        }

        var reorderedEntries = currentQueue.Entries
            .Where(entry => !ReferenceEquals(entry.UnitReference.Unit, target.Unit))
            .Concat(new[] { targetEntry })
            .ToList();

        while (!currentQueue.IsEmpty)
        {
            _ = currentQueue.PopFirst();
        }

        foreach (var entry in reorderedEntries)
        {
            currentQueue.Add(entry);
        }
    }

    private static bool TryConsumeTravelerSp(TravelerTurnContext travelerTurnContext, int cost)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        if (travelerState.CurrentSp < cost)
        {
            return false;
        }

        travelerState.CurrentSp -= cost;
        return true;
    }

    private static void ResetRevivedTravelerRoundFlags(CombatUnitState revivedTravelerState)
    {
        revivedTravelerState.IsDefending = false;
        revivedTravelerState.HasDefenderPriorityNextRound = false;
        revivedTravelerState.HasBreakingRecoveryPriorityThisRound = false;
        revivedTravelerState.HasBreakingRecoveryPriorityNextRound = false;
        revivedTravelerState.HasIncreasedPriorityThisRound = false;
        revivedTravelerState.HasDecreasedPriorityThisRound = false;
    }

    private static bool DefendTraveler(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        travelerState.IsDefending = true;
        travelerState.HasDefenderPriorityNextRound = true;
        return CompleteTravelerTurn(travelerTurnContext);
    }

    private static bool CompleteTravelerTurn(TravelerTurnContext travelerTurnContext)
    {
        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private bool TravelerFlees(TravelerTurnContext travelerTurnContext)
    {
        _view.ShowFleeMessage();
        travelerTurnContext.CombatState.FinishBattle(BattleResult.PlayerDefeat);
        _view.ShowEnemyWinMessage();
        return true;
    }

    private bool TryResolveTravelerBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = TrySelectWeapon(travelerTurnContext);
        if (selectedWeapon is null)
        {
            return false;
        }

        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        var basicAttackContext = new TravelerBasicAttackContext(travelerTurnContext, selectedWeapon, selectedTarget);
        ExecuteTravelerBasicAttack(basicAttackContext);
        return true;
    }

    private string? TrySelectWeapon(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = _view.AskWeaponSelection(travelerTurnContext.Traveler.Weapons);
        if (selectedWeapon == travelerTurnContext.Traveler.Weapons.Count + 1)
        {
            return null;
        }

        return travelerTurnContext.Traveler.Weapons[selectedWeapon - 1];
    }

    private UnitReference? TrySelectTravelerTarget(TravelerTurnContext travelerTurnContext)
    {
        var aliveBeasts = travelerTurnContext.CombatState.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildEnemySnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTarget = _view.AskTravelerTarget(travelerTurnContext.Traveler.Name, enemySnapshots);
        if (selectedTarget == enemySnapshots.Count + 1)
        {
            return null;
        }

        return aliveBeasts[selectedTarget - 1];
    }

    private void AskBoostPointsIfAvailable(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        if (travelerState.CurrentBp > 0)
        {
            _ = _view.AskBoostPointsToUse();
        }
    }

    private void ExecuteTravelerBasicAttack(TravelerBasicAttackContext basicAttackContext)
    {
        var combatState = basicAttackContext.TravelerTurnContext.CombatState;
        var targetState = combatState.GetUnitState(basicAttackContext.TargetBeast);
        var hasWeakness = HasWeaknessAgainstAttackType(basicAttackContext.TargetBeast, basicAttackContext.WeaponType);
        var wasInBreakingPoint = targetState.IsInBreakingPoint;

        var baseDamage = CalculateTravelerAttackRawDamage(basicAttackContext);
        var damageMultiplier = GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint);
        var dealtDamage = ApplyMultiplier(baseDamage, damageMultiplier);

        var enteredBreakingPoint = false;
        if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
        {
            enteredBreakingPoint = combatState.TryConsumeBeastShield(basicAttackContext.TargetBeast);
        }

        var targetCurrentHp = combatState.ApplyDamage(basicAttackContext.TargetBeast, dealtDamage);
        _view.ShowTravelerAttackResult(new TravelerAttackViewData(
            basicAttackContext.TravelerTurnContext.Traveler.Name,
            basicAttackContext.TargetBeast.Unit.Name,
            basicAttackContext.WeaponType,
            dealtDamage,
            hasWeakness,
            enteredBreakingPoint,
            targetCurrentHp));
    }

    private static double CalculateTravelerAttackRawDamage(TravelerBasicAttackContext basicAttackContext)
    {
        return CalculatePhysicalDamageRaw(
            basicAttackContext.TravelerTurnContext.Traveler.Stats.PhysicalAttack,
            basicAttackContext.TargetBeast.Unit.Stats.PhysicalDefense,
            BasicAttackModifier);
    }

    private void ResolveBeastTurn(CombatFlowState combatState, TurnEntry beastTurn)
    {
        var actingBeast = GetBeastFromTurn(beastTurn);
        var beastSkill = _beastSkillCatalog.GetByName(actingBeast.Skill);

        if (beastSkill.IsArea)
        {
            ExecuteAreaBeastSkill(combatState, actingBeast, beastSkill);
        }
        else
        {
            ExecuteSingleTargetBeastSkill(combatState, actingBeast, beastSkill);
        }

        combatState.CompleteTurn();
    }

    private static Beast GetBeastFromTurn(TurnEntry beastTurn)
    {
        return beastTurn.UnitReference.Unit as Beast
            ?? throw new InvalidOperationException("Beast turn expected a beast unit.");
    }

    private void ExecuteSingleTargetBeastSkill(CombatFlowState combatState, Beast beast, BeastSkillSpec beastSkill)
    {
        var targetTraveler = SelectBeastTarget(combatState, beastSkill.TargetRule);
        var attackResult = ResolveBeastAttackAgainstTarget(combatState, beast, beastSkill, targetTraveler);

        _view.ShowBeastAttackResult(new BeastAttackViewData(
            beast.Name,
            beastSkill.Name,
            targetTraveler.Unit.Name,
            ResolveBeastDamageLabel(beastSkill.AttackKind),
            attackResult.Damage,
            attackResult.TargetWasDefending,
            attackResult.TargetCurrentHp));
    }

    private void ExecuteAreaBeastSkill(CombatFlowState combatState, Beast beast, BeastSkillSpec beastSkill)
    {
        var targets = combatState.GetAliveTravelers();
        var results = new List<BeastAreaTargetViewData>();

        foreach (var target in targets)
        {
            var targetResult = ResolveBeastAttackAgainstTarget(combatState, beast, beastSkill, target);
            results.Add(new BeastAreaTargetViewData(
                target.Unit.Name,
                targetResult.Damage,
                targetResult.TargetWasDefending,
                targetResult.TargetCurrentHp));
        }

        _view.ShowBeastAreaAttackResult(new BeastAreaAttackViewData(
            beast.Name,
            beastSkill.Name,
            ResolveBeastDamageLabel(beastSkill.AttackKind),
            results));
    }

    private static BeastAttackResolution ResolveBeastAttackAgainstTarget(
        CombatFlowState combatState,
        Beast beast,
        BeastSkillSpec beastSkill,
        UnitReference targetTraveler)
    {
        var targetState = combatState.GetUnitState(targetTraveler);
        var targetWasDefending = targetState.IsDefending && !beastSkill.IgnoresDefend;

        var totalDamage = 0;
        var hitCount = Math.Max(1, beastSkill.Hits);

        for (var hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            var hitDamage = CalculateBeastHitDamage(beast, targetTraveler, targetState, beastSkill, targetWasDefending);
            if (hitDamage <= 0)
            {
                continue;
            }

            totalDamage += hitDamage;
            _ = combatState.ApplyDamage(targetTraveler, hitDamage);

            if (targetState.CurrentHp <= 0)
            {
                break;
            }
        }

        return new BeastAttackResolution(totalDamage, targetWasDefending, targetState.CurrentHp);
    }

    private static int CalculateBeastHitDamage(
        Beast beast,
        UnitReference targetTraveler,
        CombatUnitState targetState,
        BeastSkillSpec beastSkill,
        bool targetWasDefending)
    {
        if (beastSkill.AttackKind == BeastAttackKind.HalveCurrentHp)
        {
            var remainingHp = targetState.CurrentHp / 2;
            return Math.Max(0, targetState.CurrentHp - remainingHp);
        }

        var rawDamage = beastSkill.AttackKind == BeastAttackKind.Elemental
            ? CalculateElementalDamageRaw(beast.Stats.ElementalAttack, targetTraveler.Unit.Stats.ElementalDefense, beastSkill.Modifier)
            : CalculatePhysicalDamageRaw(beast.Stats.PhysicalAttack, targetTraveler.Unit.Stats.PhysicalDefense, beastSkill.Modifier);

        return ApplyDefenderMitigation(rawDamage, targetWasDefending);
    }

    private static string ResolveBeastDamageLabel(BeastAttackKind attackKind)
    {
        if (attackKind == BeastAttackKind.HalveCurrentHp)
        {
            return string.Empty;
        }

        return attackKind == BeastAttackKind.Elemental ? "elemental" : "físico";
    }

    private static bool HasWeaknessAgainstAttackType(UnitReference beastReference, string attackType)
    {
        if (beastReference.Unit is not Beast beast)
        {
            return false;
        }

        return beast.Weaknesses.Any(weakness => string.Equals(weakness, attackType, StringComparison.OrdinalIgnoreCase));
    }

    private static double GetTravelerDamageMultiplier(bool hasWeakness, bool isTargetInBreakingPoint)
    {
        if (hasWeakness && isTargetInBreakingPoint)
        {
            return 2.0;
        }

        if (hasWeakness || isTargetInBreakingPoint)
        {
            return 1.5;
        }

        return 1.0;
    }

    private static int ApplyMultiplier(double baseValue, double multiplier)
    {
        return Math.Max(0, Convert.ToInt32(Math.Floor(baseValue * multiplier)));
    }

    private static int ApplyDefenderMitigation(double damage, bool isDefending)
    {
        var mitigatedDamage = isDefending ? damage * 0.5 : damage;
        return Math.Max(0, Convert.ToInt32(Math.Floor(mitigatedDamage)));
    }

    private static double CalculatePhysicalDamageRaw(int attackerPhysicalAttack, int targetPhysicalDefense, double modifier)
    {
        var rawDamage = attackerPhysicalAttack * modifier - targetPhysicalDefense;
        return Math.Max(0, rawDamage);
    }

    private static void RefreshProjectedNextRoundQueue(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is null)
        {
            return;
        }

        var projectedNextQueue = TurnQueueFactory.BuildNextRoundQueue(combatState);
        combatState.CurrentRound.ReplaceNextQueue(projectedNextQueue);
    }

    private static UnitReference SelectBeastTarget(CombatFlowState combatState, BeastTargetRule? targetRule)
    {
        var travelers = combatState.GetAliveTravelers();
        var selectedRule = targetRule ?? BeastTargetRule.HighestCurrentHp;

        return selectedRule switch
        {
            BeastTargetRule.HighestElementalAttack => travelers
                .OrderByDescending(reference => reference.Unit.Stats.ElementalAttack)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.LowestPhysicalDefense => travelers
                .OrderBy(reference => reference.Unit.Stats.PhysicalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestSpeed => travelers
                .OrderByDescending(reference => reference.Unit.Stats.Speed)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.LowestElementalDefense => travelers
                .OrderBy(reference => reference.Unit.Stats.ElementalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestPhysicalAttack => travelers
                .OrderByDescending(reference => reference.Unit.Stats.PhysicalAttack)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestPhysicalDefense => travelers
                .OrderByDescending(reference => reference.Unit.Stats.PhysicalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            _ => travelers
                .OrderByDescending(reference => combatState.GetUnitState(reference).CurrentHp)
                .ThenBy(reference => reference.BoardPosition)
                .First()
        };
    }

    private static BattleResult EvaluateBattleResult(CombatFlowState combatState)
    {
        if (!combatState.GetAliveBeasts().Any())
        {
            return BattleResult.PlayerVictory;
        }

        if (!combatState.GetAliveTravelers().Any())
        {
            return BattleResult.PlayerDefeat;
        }

        return BattleResult.Ongoing;
    }

    private static int CalculatePhysicalDamage(int attackerPhysicalAttack, int targetPhysicalDefense, double modifier)
    {
        var rawDamage = attackerPhysicalAttack * modifier - targetPhysicalDefense;
        return Math.Max(0, Convert.ToInt32(Math.Floor(rawDamage)));
    }

    private static int CalculateElementalDamage(int attackerElementalAttack, int targetElementalDefense, double modifier)
    {
        return Math.Max(0, Convert.ToInt32(Math.Floor(CalculateElementalDamageRaw(attackerElementalAttack, targetElementalDefense, modifier))));
    }

    private static double CalculateElementalDamageRaw(int attackerElementalAttack, int targetElementalDefense, double modifier)
    {
        var rawDamage = attackerElementalAttack * modifier - targetElementalDefense;
        return Math.Max(0, rawDamage);
    }

    private static double CalculateLastStandRawDamage(TravelerTurnContext travelerTurnContext, UnitReference target, double baseModifier)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        var missingHpRatio = (travelerState.MaxHp - travelerState.CurrentHp) / (double)travelerState.MaxHp;
        var baseDamage = CalculatePhysicalDamageRaw(
            travelerTurnContext.Traveler.Stats.PhysicalAttack,
            target.Unit.Stats.PhysicalDefense,
            baseModifier);

        var missingHpPercent = Math.Floor(missingHpRatio * 100);
        var scalingFactor = 1 + 0.03 * missingHpPercent;
        return baseDamage * scalingFactor;
    }

    private static UnitDisplaySnapshot BuildEnemySnapshot(CombatFlowState combatState, UnitReference reference)
    {
        var state = combatState.GetUnitState(reference);
        var slot = ((char)('A' + reference.BoardPosition)).ToString();

        return new UnitDisplaySnapshot(
            slot,
            reference.Unit.Name,
            reference.Kind,
            state.CurrentHp,
            state.MaxHp,
            state.CurrentSp,
            state.MaxSp,
            state.CurrentBp,
            state.CurrentShields);
    }

    private static void RechargeBpForAliveTravelers(CombatFlowState combatState)
    {
        foreach (var travelerState in combatState.UnitStates.Where(s => s.UnitReference.Kind == CombatantKind.Traveler && s.IsAlive))
        {
            travelerState.CurrentBp = Math.Min(5, travelerState.CurrentBp + 1);
        }
    }

    private static void UpdateBreakingPointStates(CombatFlowState combatState)
    {
        foreach (var beastState in combatState.UnitStates.Where(s => s.UnitReference.Kind == CombatantKind.Beast && s.IsAlive))
        {
            beastState.HasBreakingRecoveryPriorityThisRound = false;
            beastState.HasBreakingRecoveryPriorityNextRound = false;

            if (beastState.BreakingRoundsRemaining <= 0)
            {
                beastState.CanActThisRound = true;
                beastState.CanActNextRound = true;
                continue;
            }

            beastState.BreakingRoundsRemaining--;
            if (beastState.BreakingRoundsRemaining == 0)
            {
                beastState.CurrentShields = beastState.MaxShields;
                beastState.CanActThisRound = true;
                beastState.CanActNextRound = true;
                beastState.HasBreakingRecoveryPriorityThisRound = true;
                continue;
            }

            beastState.CanActThisRound = false;
            beastState.CanActNextRound = true;
            beastState.HasBreakingRecoveryPriorityNextRound = true;
        }
    }

    private static void UpdatePriorityModifiers(CombatFlowState combatState)
    {
        foreach (var unitState in combatState.UnitStates.Where(state => state.IsAlive))
        {
            unitState.HasIncreasedPriorityThisRound = false;
            unitState.HasDecreasedPriorityThisRound = false;
            unitState.PriorityModifierNextRound = 0;

            if (unitState.IncreasedPriorityRoundsRemaining > 0)
            {
                unitState.HasIncreasedPriorityThisRound = true;
                unitState.IncreasedPriorityRoundsRemaining--;
            }

            if (unitState.DecreasedPriorityRoundsRemaining > 0)
            {
                unitState.HasDecreasedPriorityThisRound = true;
                unitState.DecreasedPriorityRoundsRemaining--;
            }

            if (unitState.IncreasedPriorityRoundsRemaining > 0)
            {
                unitState.PriorityModifierNextRound = 1;
            }
            else if (unitState.DecreasedPriorityRoundsRemaining > 0)
            {
                unitState.PriorityModifierNextRound = -1;
            }
        }
    }

    private static void ConsumeRoundPriorityFlags(CombatFlowState combatState)
    {
        foreach (var unitState in combatState.UnitStates.Where(state => state.IsAlive))
        {
            unitState.HasBreakingRecoveryPriorityThisRound = false;
            unitState.HasDefenderPriorityNextRound = false;
        }
    }

    private void ShowWinner(BattleResult result)
    {
        if (result == BattleResult.PlayerVictory)
        {
            _view.ShowPlayerWinMessage();
            return;
        }

        _view.ShowEnemyWinMessage();
    }

    private sealed record BeastAttackResolution(int Damage, bool TargetWasDefending, int TargetCurrentHp);

    private sealed record OffensiveSkillSpec(
        int SpCost,
        double Modifier,
        string DamageType,
        bool IsElemental,
        bool IsMercyStrike);
}

