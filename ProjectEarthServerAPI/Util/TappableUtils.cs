using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProjectEarthServerAPI.Models;
using ProjectEarthServerAPI.Models.Features;
using ProjectEarthServerAPI.Models.Player;
using Serilog;
using Uma.Uuid;

namespace ProjectEarthServerAPI.Util
{
	/// <summary>
	/// Some simple utilities to interface with generated files from Tappy
	/// </summary>
	public class TappableUtils
	{
		private static Version4Generator version4Generator = new Version4Generator();

		// TODO: Consider turning this into a dictionary (or pull it out to a separate file) and building out a spawn-weight system? 
		public static string[] TappableTypes = new[]
		{
			"genoa:stone_mound_a_tappable_map", "genoa:stone_mound_b_tappable_map",
			"genoa:stone_mound_c_tappable_map", "genoa:grass_mound_a_tappable_map",
			"genoa:grass_mound_b_tappable_map", "genoa:grass_mound_c_tappable_map", "genoa:tree_oak_a_tappable_map",
			"genoa:tree_oak_b_tappable_map", "genoa:tree_oak_c_tappable_map", "genoa:tree_birch_a_tappable_map",
			"genoa:tree_spruce_a_tappable_map", "genoa:chest_tappable_map", "genoa:sheep_tappable_map",
			"genoa:cow_tappable_map", "genoa:pig_tappable_map", "genoa:chicken_tappable_map"
		};

		private static Random random = new Random();

		// For json deserialization
		public class ItemDrop {
			public float chance { get; set; }
			public int min { get; set; }
			public int max { get; set; }
		}
		public class TappableLootTable
		{
			public string tappableID { get; set; }
			[JsonConverter(typeof(StringEnumConverter))]
		    public Item.Rarity rarity { get; set; }
			public Dictionary<Guid, ItemDrop> dropTable { get; set; }
		}

		public static Dictionary<string, TappableLootTable> loadAllTappableSets()
		{
			Log.Information("[Tappables] Loading tappable data.");
			Dictionary<string, TappableLootTable> tappableData = new();
			string[] files = Directory.GetFiles("./data/tappable", "*.json");
			foreach (var file in files)
			{
				TappableLootTable table = JsonConvert.DeserializeObject<TappableLootTable>(File.ReadAllText(file));
				tappableData.Add(table.tappableID, table);
				Log.Information($"Loaded {table.dropTable.Count} drops for tappable ID {table.tappableID} | Path: {file}");
			}

			return tappableData;
		}

		/// <summary>
		/// Generate a new tappable in a given radius of a given cord set
		/// </summary>
		/// <param name="longitude"></param>
		/// <param name="latitude"></param>
		/// <param name="radius">Optional. Spawn Radius if not provided, will default to value specified in config</param>
		/// <param name="type">Optional. If not provided, a random type will be picked from TappableUtils.TappableTypes</param>
		/// <returns></returns>
		//double is default set to negative because its *extremely unlikely* someone will set a negative value intentionally, and I can't set it to null.
		public static LocationResponse.ActiveLocation createTappableInRadiusOfCoordinates(double latitude, double longitude, double radius = -1.0, string type = null)
		{
			//if null we do random
			type ??= TappableUtils.TappableTypes[random.Next(0, TappableUtils.TappableTypes.Length)];
			if (radius == -1.0)
			{
				radius = StateSingleton.Instance.config.tappableSpawnRadius;
			}
			Item.Rarity rarity;

			try
			{
				rarity = StateSingleton.Instance.tappableData[type].rarity;
			}
			catch (Exception e)
			{
				Log.Error("[Tappables] Tappable rarity was not found for tappable type " + type + ". Using common");
				rarity = Item.Rarity.Common;
			}

			var currentTime = DateTime.UtcNow;

			//Nab tile loc
			string tileId = Tile.getTileForCoordinates(latitude, longitude);
			LocationResponse.ActiveLocation tappable = new LocationResponse.ActiveLocation
			{
				id = Guid.NewGuid(), // Generate a random GUID for the tappable
				tileId = tileId,
				coordinate = new Coordinate
				{
					latitude = Math.Round(latitude + (random.NextDouble() * 2 - 1) * radius, 6), // Round off for the client to be happy
					longitude = Math.Round(longitude + (random.NextDouble() * 2 - 1) * radius, 6)
				},
				spawnTime = currentTime,
				expirationTime = currentTime.AddMinutes(10), //Packet captures show that typically earth keeps Tappables around for 10 minutes
				type = "Tappable", // who wouldve guessed?
				icon = type,
				metadata = new LocationResponse.Metadata
				{
					rarity = rarity,
					rewardId = version4Generator.NewUuid().ToString() // Seems to always be uuidv4 from official responses so generate one
				},
				encounterMetadata = null, //working captured responses have this, its fine
				tappableMetadata = new LocationResponse.TappableMetadata
				{
					rarity = rarity //assuming this and the above need to allign. Why have 2 occurances? who knows.
				}
			};

			var rewards = GenerateRewardsForTappable(tappable.icon);

			var storage = new LocationResponse.ActiveLocationStorage {location = tappable, rewards = rewards};

			StateSingleton.Instance.activeTappables.Add(tappable.id, storage);

			return tappable;
		}

		public static TappableResponse RedeemTappableForPlayer(string playerId, TappableRequest request)
		{
			var tappable = StateSingleton.Instance.activeTappables[request.id];

			var response = new TappableResponse()
			{
				result = new TappableResponse.Result()
				{
					token = new Token()
					{
						clientProperties = new Dictionary<string, string>(),
						clientType = "redeemtappable",
						lifetime = "Persistent",
						rewards = tappable.rewards
					}
				},
				updates = RewardUtils.RedeemRewards(playerId, tappable.rewards, EventLocation.Tappable)
			};

			EventUtils.HandleEvents(playerId, new TappableEvent{eventId = tappable.location.id});
			StateSingleton.Instance.activeTappables.Remove(tappable.location.id);

			return response;
		}

		private static Guid GetRandomItemForTappable(string type) 
		{
			Dictionary<Guid, ItemDrop> DropTable = StateSingleton.Instance.tappableData[type].dropTable;
			float totalPercentage = (int)DropTable.Sum(item => item.Value.chance);
			float diceRoll = random.Next(0, (int)(totalPercentage*10))/10;
			foreach (Guid item in DropTable.Keys)
			{
				if (diceRoll >= DropTable[item].chance && (random.Next(0, 4) >= 3))
					return item;
				diceRoll -= DropTable[item].chance;
			}
			return Guid.Empty;
		}

		public static Rewards GenerateRewardsForTappable(string type)
		{
			var catalog = StateSingleton.Instance.catalog;
			Dictionary<Guid, ItemDrop> DropTable;
			var targetDropSet = new Dictionary<Guid, int> { };
			int experiencePoints = 0;

			try
			{
				DropTable = StateSingleton.Instance.tappableData[type].dropTable;
			}
			catch (Exception e)
			{
				Log.Error("[Tappables] no json file for tappable type " + type + " exists in data/tappables. Using backup of dirt (f0617d6a-c35a-5177-fcf2-95f67d79196d)");
				Guid dirtId = Guid.Parse("f0617d6a-c35a-5177-fcf2-95f67d79196d");
				var dirtReward = new Rewards { 
					Inventory = new RewardComponent[1] { new RewardComponent { Id = dirtId, Amount = 1 } }, 
					ExperiencePoints = catalog.result.items.Find(match => match.id == dirtId).experiencePoints.tappable, 
					Rubies = (random.Next(0, 4) >= 3) ? 1 : 0,
				};

				return dirtReward;
				//dirt for you... sorry :/
			}

			for (int i = 0; i < 3; i++)
			{
				Guid item = GetRandomItemForTappable(type);
				if (!targetDropSet.Keys.Contains(item) && item != Guid.Empty)
				{
					int amount = random.Next(DropTable[item].min, DropTable[item].max);
					targetDropSet.Add(item, amount);
					experiencePoints += catalog.result.items.Find(match => match.id == item).experiencePoints.tappable * amount;
				}
			}

			if (targetDropSet.Count == 0)
			{
				Guid item = DropTable.Aggregate((x, y) => x.Value.chance > y.Value.chance ? x : y).Key;
				int amount = random.Next(DropTable[item].min, DropTable[item].max);
				targetDropSet.Add(item, amount);
				experiencePoints += catalog.result.items.Find(match => match.id == item).experiencePoints.tappable * amount;
			}

			var itemRewards = new RewardComponent[targetDropSet.Count];
			for (int i = 0; i < targetDropSet.Count; i++)
			{
				itemRewards[i] = new RewardComponent() { 
					Amount = targetDropSet[targetDropSet.Keys.ToList()[i]], 
					Id = targetDropSet.Keys.ToList()[i] 
				};
			}

			var rewards = new Rewards { 
				Inventory = itemRewards, 
				ExperiencePoints = experiencePoints, 
				Rubies = (random.Next(0, 4) >= 3) ? 1 : 0 
			}; 

			return rewards;
		}

		public static LocationResponse.Root GetActiveLocations(double lat, double lon, double radius = -1.0)
		{
			if (radius == -1.0) radius = StateSingleton.Instance.config.tappableSpawnRadius;
			var maxCoordinates = new Coordinate {latitude = lat + radius, longitude = lon + radius};

			var tappables = StateSingleton.Instance.activeTappables
				.Where(pred =>
					(pred.Value.location.coordinate.latitude >= lat && pred.Value.location.coordinate.latitude <= maxCoordinates.latitude)
					&& (pred.Value.location.coordinate.longitude >= lon && pred.Value.location.coordinate.longitude <= maxCoordinates.longitude))
				.ToDictionary(pred => pred.Key, pred => pred.Value.location).Values.ToList();

			if (tappables.Count <= StateSingleton.Instance.config.maxTappableSpawnAmount)
			{
				var count = random.Next(StateSingleton.Instance.config.minTappableSpawnAmount,
					StateSingleton.Instance.config.maxTappableSpawnAmount);
				count -= tappables.Count;
				for (; count > 0; count--)
				{
					var tappable = createTappableInRadiusOfCoordinates(lat, lon);
					tappables.Add(tappable);
				}
			}

			var encounters = AdventureUtils.GetEncountersForLocation(lat, lon);
			tappables.AddRange(encounters.Where(pred => 
					(pred.coordinate.latitude >= lat && pred.coordinate.latitude <= maxCoordinates.latitude)
					&& (pred.coordinate.longitude >= lon && pred.coordinate.longitude <= maxCoordinates.longitude)).ToList());

			return new LocationResponse.Root
			{
				result = new LocationResponse.Result
				{
					killSwitchedTileIds = new List<object> { }, //havent seen this used. Debugging thing maybe?
					activeLocations = tappables,
				},
				expiration = null,
				continuationToken = null,
				updates = new Updates()
			};
		}
	}
}
