using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

internal delegate bool TravelerTargetSelector(TravelerTurnContext travelerTurnContext, out UnitReference selectedTarget);
