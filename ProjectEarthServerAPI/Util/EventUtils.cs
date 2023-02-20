using System;
using ProjectEarthServerAPI.Models;
using ProjectEarthServerAPI.Models.Player;
using ProjectEarthServerAPI.Models.Features;
using Serilog;

namespace ProjectEarthServerAPI.Util
{
	public class EventUtils
	{
		public static void HandleEvents(string playerId, BaseEvent genoaEvent)
		{
			switch (genoaEvent)
			{
				case ItemEvent ev:
					Log.Debug("[System] Item Event dispatched!");
					Log.Debug(ev.action.ToString());
					ChallengeUtils.ProgressChallenge(playerId, genoaEvent);
					if (ev.action == ItemEventAction.ItemSmelted)
						JournalUtils.AddActivityLogEntry(playerId, DateTime.UtcNow, Scenario.SmeltingJobCompleted,
							new Rewards { Inventory = new RewardComponent[] { new RewardComponent { Id = ev.eventId, Amount = (int)ev.amount } } },
							ChallengeDuration.Career, null, null, null, null, null);

					if (ev.action == ItemEventAction.ItemCrafted)
						JournalUtils.AddActivityLogEntry(playerId, DateTime.UtcNow, Scenario.CraftingJobCompleted,
							new Rewards { Inventory = new RewardComponent[] { new RewardComponent { Id = ev.eventId, Amount = (int)ev.amount } } },
							ChallengeDuration.Career, null, null, null, null, null);
							
					if (ev.action == ItemEventAction.ItemJournalEntryUnlocked)
						JournalUtils.AddActivityLogEntry(playerId, DateTime.UtcNow, Scenario.JournalContentCollected,
							new Rewards { Inventory = new RewardComponent[] { new RewardComponent { Id = ev.eventId, Amount = 0 } } },
							ChallengeDuration.Career, null, null, null, null, null);
					break;

				case MultiplayerEvent ev:
					Log.Debug("[System] Multiplayer Event dispatched!");
					break;

				case ChallengeEvent ev:
					Log.Debug("[System] Challenge Event dispatched!");
					ChallengeUtils.ProgressChallenge(playerId, genoaEvent);
					break;

				case MobEvent ev:
					Log.Debug("[System] Mob Event dispatched!");
					break;

				case TappableEvent ev:
					Log.Debug("[System] Tappable Event dispatched!");

					var tappable = StateSingleton.Instance.activeTappables[ev.eventId];
					Log.Debug($"[System] Tappable Type: {tappable.location.type}");
					Log.Debug($"[System] Tappable Location: {tappable.location.tileId}");

					ChallengeUtils.ProgressChallenge(playerId, genoaEvent);
					JournalUtils.AddActivityLogEntry(playerId, DateTime.UtcNow, Scenario.TappableCollected, tappable.rewards, 
						ChallengeDuration.Career, ActiveLocationType.Tappable, null, null, null, null);

					break;

				default:
					Log.Error("Error: Something tried to fire a normal BaseEvent!");
					break;
			}
		}
	}
}
