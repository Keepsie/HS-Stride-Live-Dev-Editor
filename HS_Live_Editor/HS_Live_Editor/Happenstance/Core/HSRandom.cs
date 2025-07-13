// HS Stride Engine Core (c) 2025 Happenstance Games LLC - MIT License

using System;
using System.Collections.Generic;
using System.Linq;

namespace Happenstance.SE.Core
{
    public static class HSRandom
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Returns a random float number between min [inclusive] and max [inclusive]
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        public static float Range(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Returns a random integer between min [inclusive] and max [exclusive]
        /// Example: Range(1,10) returns values 1-9
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive)</param>
        public static int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Returns a random float between 0.0 [inclusive] and 1.0 [exclusive]
        /// </summary>
        public static float Value => (float)_random.NextDouble();

        /// <summary>
        /// Returns a random item from the list. Returns default if list is null or empty
        /// </summary>
        /// <param name="list">The list to select from</param>
        /// <typeparam name="T">Type of items in the list</typeparam>
        public static T GetRandom<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            return list[_random.Next(list.Count)];
        }

        /// <summary>
        /// Returns true based on percentage chance (0-100)
        /// Example: DiceRoll(25f) has a 25% chance to return true
        /// </summary>
        /// <param name="percentage">Chance to succeed (0-100)</param>
        public static bool DiceRoll(float percentage)
        {
            float diceRoll = Range(0f, 101f);
            return diceRoll <= percentage;
        }

        /// <summary>
        /// Performs multiple dice rolls and checks if enough succeeded
        /// Example: MultiDiceRoll(50f, 3, 2) rolls three times at 50% chance each, needs 2 successes
        /// </summary>
        /// <param name="percentage">Chance for each roll to succeed (0-100)</param>
        /// <param name="totalRolls">Number of rolls to make</param>
        /// <param name="neededSuccesses">Number of rolls that need to succeed</param>
        /// <returns>True if enough rolls succeeded</returns>
        public static bool MultiDiceRoll(float percentage, int totalRolls, int neededSuccesses)
        {
            if (totalRolls < neededSuccesses) return false;

            int successes = 0;
            for (int i = 0; i < totalRolls; i++)
            {
                if (DiceRoll(percentage)) successes++;
                if (successes >= neededSuccesses) return true;
                if (successes + (totalRolls - i - 1) < neededSuccesses) return false;
            }
            return successes >= neededSuccesses;
        }

        /// <summary>
        /// Returns a random item from the list based on weights
        /// Example: WeightedRandom(["Common", "Rare"], [75, 25]) has 75% chance for "Common"
        /// </summary>
        /// <param name="items">List of items to choose from</param>
        /// <param name="weights">Weight of each item (must match items count)</param>
        /// <typeparam name="T">Type of items in the list</typeparam>
        /// <returns>Selected item or default if invalid input</returns>
        public static T WeightedRandom<T>(List<T> items, List<float> weights)
        {
            if (items == null || weights == null || items.Count != weights.Count || items.Count == 0)
                return default;

            float totalWeight = weights.Sum();
            float randomPoint = Range(0f, totalWeight);

            float currentWeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                currentWeight += weights[i];
                if (randomPoint <= currentWeight)
                    return items[i];
            }

            return items[items.Count - 1];
        }

        /// <summary>
        /// Returns a list of unique random items from the source list
        /// Example: GetRandomUnique(["A","B","C","D"], 2) might return ["C","A"]
        /// </summary>
        /// <param name="items">Source list to pick from</param>
        /// <param name="count">Number of items to select</param>
        /// <typeparam name="T">Type of items in the list</typeparam>
        /// <returns>List of randomly selected unique items</returns>
        public static List<T> GetRandomUnique<T>(List<T> items, int count)
        {
            if (items == null || items.Count == 0) return new List<T>();
            if (count > items.Count) count = items.Count;

            List<T> result = new List<T>();
            List<T> tempList = new List<T>(items);

            while (result.Count < count)
            {
                int index = Range(0, tempList.Count);
                result.Add(tempList[index]);
                tempList.RemoveAt(index);
            }

            return result;
        }

        /// <summary>
        /// Returns a random value with bell curve distribution (values near the middle are more likely)
        /// More iterations = stronger curve toward middle
        /// Example: BellCurve(0, 100, 3) is more likely to return values around 50
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        /// <param name="iterations">Number of averaged rolls (minimum 1)</param>
        /// <returns>Random value with bell curve distribution</returns>
        public static float BellCurve(float min, float max, int iterations = 3)
        {
            if (iterations <= 0) iterations = 1;

            float value = 0;
            for (int i = 0; i < iterations; i++)
            {
                value += Range(min, max);
            }
            return value / iterations;
        }
    }
}