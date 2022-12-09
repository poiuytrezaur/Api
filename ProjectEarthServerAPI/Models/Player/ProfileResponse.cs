using ProjectEarthServerAPI.Util;
using System.Collections.Generic;

namespace ProjectEarthServerAPI.Models
{
	public class ProfileResponse
	{
		public static Dictionary<string, ProfileLevel> levels { get; private set; }

		// Setup the level distribution dict when the class is first called
		static ProfileResponse()
		{
			levels = StateSingleton.Instance.levels;
		}

		public ProfileResult result { get; set; }
		public object continuationToken { get; set; }
		public object expiration { get; set; }
		public Updates updates { get; set; }

		public ProfileResponse(ProfileData profileData)
		{
			result = ProfileResult.of(profileData);
		}
	}

	public class ProfileResult : ProfileData
	{
		public Dictionary<string, ProfileLevel> levelDistribution { get; set; }

		public static ProfileResult of(ProfileData profileData)
		{
			return new ProfileResult
			{
				totalExperience = profileData.totalExperience,
				level = profileData.level,
				health = profileData.health,
				healthPercentage = profileData.healthPercentage,
				levelDistribution = ProfileResponse.levels
			};
		}
	}

	public class ProfileLevel
	{
		public int experienceRequired { get; set; }
		public Rewards rewards { get; set; }

		public ProfileLevel()
		{
			rewards = new Rewards();
		}
	}

	public class ProfileData
	{
		public int totalExperience { get; set; }
		public int level { get; set; }

		public int currentLevelExperience
		{
			get
			{
				ProfileLevel profileLevel;
				if (ProfileResponse.levels.TryGetValue(level.ToString(), out profileLevel))
				{
					return totalExperience - profileLevel.experienceRequired;
				}

				return totalExperience;
			}
		}

		public int experienceRemaining
		{
			get
			{
				ProfileLevel profileLevel;
				if (ProfileResponse.levels.TryGetValue((level + 1).ToString(), out profileLevel))
				{
					return profileLevel.experienceRequired - currentLevelExperience;
				}

				return 0;
			}
		}

		public int? health { get; set; }
		public float healthPercentage { get; set; }

		public ProfileData()
		{
			totalExperience = 0;
			level = 1;
			health = 20;
			healthPercentage = 100f;
		}
	}
}
