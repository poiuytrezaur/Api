﻿using System;
using System.Collections.Generic;

namespace ProjectEarthServerAPI.Models.Features
{

    public class CraftingUpdates
    {
        public Dictionary<string, int> updates { get; set; }
    }

    public class CraftingPrice
    {
        public int cost { get; set; }
        public int discount { get; set; }
        public TimeSpan validTime { get; set; }
    }

    public class CraftingPriceResponse
    {
        public CraftingPrice result { get; set; }
        public Dictionary<string,int> updates { get; set; }
    }

    public class CraftingSlotResponse
    {
        public CraftingSlotInfo result { get; set; }
        public Dictionary<string,int> updates { get; set; } 
    }

    public class CollectItemsResponse
    {
        public Rewards rewards { get; set; }
        public Dictionary<string,int> updates { get; set; }
    }

    public class CraftingSlotInfo // crafting/slot, crafting/1, crafting/2 - Also used in utilityBlocks
    {
        public int available { get; set; } // 0 - Currently not available or working, 1 - Available for collecting
        public BoostState boostState { get; set; } // See class below
        public int completed { get; set; } // Completed - maybe the same as available above, or just unused
        public InputItem[] escrow { get; set; } // Input Items for process, empty when process is finished
        public DateTime? nextCompletionUtc { get; set; } // Time when process is complete
        public RecipeOutput output { get; set; } // Output items of process
        public string recipeId { get; set; } // ID of Recipe in use
        public string sessionId { get; set; } // ID of Session for Job
        public string state { get; set; } // State: Active, Completed or Locked
        public int streamVersion { get; set; } // StreamVersion with changes, unused in our version
        public int total { get; set; } // Total number of output items
        public DateTime? totalCompletionUtc { get; set; } // Time of completion
        public UnlockPrice unlockPrice { get; set; } // Price to unlock item
    }

    public class BoostState
    {
        // TODO: Use boost to find out how to structure this
        public int multiplier { get; set; }
        public ItemInstance boostItem { get; set; } // Hope
    }

    public class UnlockPrice
    {
        public int cost { get; set; } // Cost in rubies of unlocking
        public int discount { get; set; } // Discount to give
    }

    public class InputItem
    {
        public string itemId { get; set; } // Item ID
        public ItemInstance[] itemInstanceIds { get; set; } // Only used in unstackable items (tools, etc.)
        public int quantity { get; set; } // Quantity of item
    }

    public class ItemInstance
    {
        public string id { get; set; } // ID of instance
        public double health { get; set; } // Durability of instanced item
    }
}