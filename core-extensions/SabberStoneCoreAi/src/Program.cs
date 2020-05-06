#region copyright
// SabberStone, Hearthstone Simulator in C# .NET Core
// Copyright (C) 2017-2019 SabberStone Team, darkfriend77 & rnilva
//
// SabberStone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License.
// SabberStone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Agent.ExampleAgents;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.Meta;
using SabberStoneCore.Model;
using System.IO;
using System.Text;

namespace SabberStoneCoreAi
{
	internal class Program
	{
		private static void Main()
		{

			Dictionary<string, CardClass> classMap = new Dictionary<string, CardClass>();
			Dictionary<string, List<Card>> deckMap = new Dictionary<string, List<Card>>();

			classMap.Add("RenoKazakusMage", CardClass.MAGE);
			classMap.Add("MiraclePirateRogue", CardClass.ROGUE);
			classMap.Add("ZooDiscardWarlock", CardClass.WARLOCK);
			classMap.Add("RenoKazakusDragonPriest", CardClass.PRIEST);
			classMap.Add("MidrangeBuffPaladin", CardClass.PALADIN);
			classMap.Add("MidrangeJadeShaman", CardClass.SHAMAN);
			classMap.Add("AggroPirateWarrior", CardClass.WARRIOR);

			deckMap.Add("RenoKazakusMage", Decks.RenoKazakusMage);
			deckMap.Add("MiraclePirateRogue", Decks.MiraclePirateRogue);
			deckMap.Add("ZooDiscardWarlock", Decks.ZooDiscardWarlock);
			deckMap.Add("RenoKazakusDragonPriest", Decks.RenoKazakusDragonPriest);
			deckMap.Add("MidrangeBuffPaladin", Decks.MidrangeBuffPaladin);
			deckMap.Add("MidrangeJadeShaman", Decks.MidrangeJadeShaman);
			deckMap.Add("AggroPirateWarrior", Decks.AggroPirateWarrior);

			List<string> deckList = new List<String>(deckMap.Keys);

			bool genData = false;
			if (genData) {
				runSimulation(classMap, deckMap, deckList);
			} else {
				List<Double> weights = new List<double>{0.02978996187448501587,-0.00318665639497339725,0.00348940002731978893,-0.00400655437260866165,0.04111846536397933960,0.11956916004419326782,-0.01585331372916698456,0.01084957178682088852,-0.12021212279796600342,-0.00146640429738909006,0.00578224333003163338,0.00287493830546736717,0.02130169607698917389,-0.00203602435067296028,0.01017116382718086243,-0.00217816513031721115,0.01499202288687229156,0.01702820882201194763,-0.00332036474719643593,0.42425787448883056641,0.00005312222856446169,0.00092798698460683227,0.46059826016426086426,-0.00093383796047419310};
				testWeights(classMap, deckMap, deckList, weights);
			}
			
		}

		private static void testWeights(Dictionary<string, CardClass> classMap, Dictionary<string, List<Card>> deckMap, List<string> deckList, List<double> weights) {
			Console.WriteLine("Simulate Games");
			Random r = new Random();	
			double wins = 0.0;
			double losses = 0.0;
			double numGames = 3000;

			for (int i = 0; i < numGames; i++) {
	
				string p1Deck = deckList[r.Next(deckList.Count)];
				string p2Deck = deckList[r.Next(deckList.Count)];

				var gameConfig = new GameConfig()
				{
					StartPlayer = r.Next(2) + 1,
					Player1HeroClass = classMap.GetValueOrDefault(p1Deck, CardClass.MAGE),
					Player2HeroClass = classMap.GetValueOrDefault(p2Deck, CardClass.ROGUE),
					Player1Deck = deckMap.GetValueOrDefault(p1Deck, Decks.RenoKazakusMage),
					Player2Deck = deckMap.GetValueOrDefault(p2Deck, Decks.MiraclePirateRogue),
					FillDecks = true,
					Shuffle = true,
					Logging = false
				};

				AbstractAgent player1 = new GreedyAgent();
				AbstractAgent player2 = new NNAgent(weights);
				Console.WriteLine("Game " + i + " " + p1Deck + " " + p2Deck);		
				var gameHandler = new POGameHandler(gameConfig, player1, player2, repeatDraws:false);
				gameHandler.PlayGames(nr_of_games:1, addResultToGameStats:true, debug:false);
				GameStats gameStats = gameHandler.getGameStats();
				wins += gameStats.PlayerB_Wins;
				losses += gameStats.PlayerA_Wins;
			}

			Console.WriteLine("Bot winrate: " + (wins/(wins + losses)));
			Console.WriteLine("Wins: " + wins);
			Console.WriteLine("Losses: " + losses);
			Console.WriteLine("Draws: " + (numGames - (wins + losses)));
		}

		private static void runSimulation(Dictionary<string, CardClass> classMap, Dictionary<string, List<Card>> deckMap, List<string> deckList) {
			int numGames = 3000;
			Random r = new Random();
			Console.WriteLine("Simulate Games");

			List<List<double>> games = new List<List<double>>();
			for (int i = 0; i < numGames; i++) {
				double max = 10;
				double min = -10;
				List<double> randWeights = 
					Enumerable.Range(0, 24) // create sequence of 100 elements
					.Select(_ =>r.NextDouble() * (max - min) + min) // for each element select random value
					.ToList(); // convert to array.

				string p1Deck = deckList[r.Next(deckList.Count)];
				string p2Deck = deckList[r.Next(deckList.Count)];

				var gameConfig = new GameConfig()
				{
					StartPlayer = r.Next(2) + 1,
					Player1HeroClass = classMap.GetValueOrDefault(p1Deck, CardClass.MAGE),
					Player2HeroClass = classMap.GetValueOrDefault(p2Deck, CardClass.ROGUE),
					Player1Deck = deckMap.GetValueOrDefault(p1Deck, Decks.RenoKazakusMage),
					Player2Deck = deckMap.GetValueOrDefault(p2Deck, Decks.MiraclePirateRogue),
					FillDecks = true,
					Shuffle = true,
					Logging = false
				};

				AbstractAgent player1 = new RandomAgent();
				AbstractAgent player2 = new NNAgent(randWeights);
				Console.WriteLine("Game " + i + " " + p1Deck + " " + p2Deck);		
				var gameHandler = new POGameHandler(gameConfig, player1, player2, repeatDraws:false);
				gameHandler.PlayGames(nr_of_games:1, addResultToGameStats:true, debug:false);
				GameStats gameStats = gameHandler.getGameStats();
				// gameStats.printResults();
				randWeights.Add(gameStats.PlayerB_Wins);
				games.Add(randWeights);
				// Console.WriteLine(String.Join(",", randWeights.Select(p=>p.ToString()).ToArray()));
			}

			StringBuilder sb = new StringBuilder();
			if(!File.Exists(@"hsdata.csv")) {
				sb.Append("HERO_HEALTH_REDUCED,HERO_ATTACK_REDUCED,WEAPON_DURABILITY_REDUCED,MINION_HEALTH_REDUCED,MINION_ATTACK_REDUCED,MINION_KILLED,MINION_APPEARED,SECRET_REMOVED,MANA_REDUCED,M_HEALTH,M_ATTACK,M_HAS_CHARGE,M_HAS_DEAHTRATTLE,M_HAS_DIVINE_SHIELD,M_HAS_LIFE_STEAL,M_HAS_STEALTH,M_HAS_TAUNT,M_HAS_WINDFURY,M_IS_RUSH,M_MANA_COST,M_POISONOUS,M_SILENCED,M_SUMMONED,M_CANT_BE_TARGETED_BY_SPELLS,RESULT\r\n");
			}

			for (int i = 0; i < games.Count(); i++)
			{
				for (int j = 0; j < games[0].Count(); j++)
				{
					sb.Append((j==0 ? "" : ",") + games[i][j]);
				}
				sb.AppendLine();
			}
			
			File.AppendAllText(@"hsdata.csv", sb.ToString());
			Console.WriteLine("Simulation successful");
			Console.ReadLine();
		}
	}
}
