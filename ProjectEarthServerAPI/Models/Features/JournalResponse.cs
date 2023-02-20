using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProjectEarthServerAPI.Models.Player;
using ProjectEarthServerAPI.Util;

namespace ProjectEarthServerAPI.Models.Features
{
	public class JournalResponse
	{
		public PlayerJournal result { get; set; }
		public Updates updates { get; set; }

		public JournalResponse()
		{
			result = new PlayerJournal { activityLog = new List<Activity>(), inventoryJournal = new Dictionary<Guid, JournalEntry>(), };
		}
	}

	public class PlayerJournal
	{
		public List<Activity> activityLog { get; set; }
		public Dictionary<Guid, JournalEntry> inventoryJournal { get; set; }
	}

	public class Activity
	{
		[JsonConverter(typeof(DateTimeConverter))]
		public DateTime eventTime { get; set; }

		public ActivityProperties properties { get; set; }
		public Rewards? rewards { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public Scenario scenario { get; set; }
	}

	public enum ActiveLocationType {
		Tappable,
		Encounter,
		PlayerAdventure
	}

	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ActivityProperties
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ChallengeDuration? duration { get; set; }
		public string order { get => OrderUint?.ToString(); }
		public bool? isFinalSeasonChallenge { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ActiveLocationType? gameplayMode { get; set; } // Mojang :(
		public string boostId { get; set; }
		public Guid? referenceId { get; set; } // Condition: Duration == SignIn, ReferenceId == ChallengeId

		[JsonIgnore]
		internal uint? OrderUint { get; set; }


		public ActivityProperties()
		{
			duration = ChallengeDuration.Career;
		}
	}

	public class JournalEntry
	{
		public uint amountCollected { get; set; }

		[JsonConverter(typeof(DateTimeConverter))]
		public DateTime firstSeen { get; set; }

		[JsonConverter(typeof(DateTimeConverter))]
		public DateTime lastSeen { get; set; }
	}

	public enum Scenario
	{
		TappableCollected = 1,
		LevelUp,
		CraftingJobCompleted,
		SmeltingJobCompleted,
		AdventurePlayed,
		PlayerDied,
		ChallengeCompleted,
		JournalContentCollected,
		CollectionGoalCompleted,
		BoostActivated,
		BoostMiniActivated,
		AdventureCrystalActivated,
		BuildplateShared,
		SharedBuildplateViewed
	}
}
