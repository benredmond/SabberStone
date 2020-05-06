using System;
using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using SabberStoneCore.Tasks.PlayerTasks;
using System.Linq;


namespace SabberStoneCoreAi.Agent
{
    class NNAgent : AbstractAgent
    {
        public static string HERO_HEALTH_REDUCED = "HERO_HEALTH_REDUCED";
        public static string HERO_ATTACK_REDUCED = "HERO_ATTACK_REDUCED";
        public static string WEAPON_DURABILITY_REDUCED = "WEAPON_DURABILITY_REDUCED";
        public static string MINION_HEALTH_REDUCED = "MINION_HEALTH_REDUCED";
        public static string MINION_ATTACK_REDUCED = "MINION_ATTACK_REDUCED";
        public static string MINION_KILLED = "MINION_KILLED";
        public static string MINION_APPEARED = "MINION_APPEARED";
        public static string SECRET_REMOVED = "SECRET_REMOVED";
        public static string MANA_REDUCED = "MANA_REDUCED";
        public static string M_HEALTH = "M_HEALTH";
        public static string M_ATTACK = "M_ATTACK";
        public static string M_HAS_CHARGE = "M_HAS_CHARGE";
        public static string M_HAS_DEAHTRATTLE = "M_HAS_DEAHTRATTLE";
        public static string M_HAS_DIVINE_SHIELD = "M_HAS_DIVINE_SHIELD";
        public static string M_HAS_LIFE_STEAL = "M_HAS_DIVINE_SHIELD";
        public static string M_HAS_STEALTH = "M_HAS_STEALTH";
        public static string M_HAS_TAUNT = "M_HAS_TAUNT";
        public static string M_HAS_WINDFURY = "M_HAS_WINDFURY";
        public static string M_IS_RUSH = "M_IS_RUSH";
        public static string M_MANA_COST = "M_MANA_COST";
        public static string M_POISONOUS = "M_POISONOUS";	
        public static string M_SILENCED = "M_SILENCED";
        public static string M_SUMMONED = "M_SUMMONED";
        public static string M_CANT_BE_TARGETED_BY_SPELLS = "M_CANT_BE_TARGETED_BY_SPELLS";
		public Dictionary<string, double> weights = new Dictionary<string, double>();

        public NNAgent(List<double> initWeights)
        {
            weights[HERO_HEALTH_REDUCED] = initWeights[0];
            weights[HERO_ATTACK_REDUCED] = initWeights[1];
            weights[WEAPON_DURABILITY_REDUCED] = initWeights[2];
            weights[MINION_HEALTH_REDUCED] = initWeights[3];
            weights[MINION_ATTACK_REDUCED] = initWeights[4];
            weights[MINION_KILLED] = initWeights[5];
            weights[MINION_APPEARED] = initWeights[6];
            weights[SECRET_REMOVED] = initWeights[7];
            weights[MANA_REDUCED] = initWeights[8];
            weights[M_HEALTH] = initWeights[9];
            weights[M_ATTACK] = initWeights[10];
            weights[M_HAS_CHARGE] = initWeights[11];
            weights[M_HAS_DEAHTRATTLE] = initWeights[12];
            weights[M_HAS_DIVINE_SHIELD] = initWeights[13];
            weights[M_HAS_LIFE_STEAL] = initWeights[14];
            weights[M_HAS_STEALTH] = initWeights[15];
            weights[M_HAS_TAUNT] = initWeights[16];
            weights[M_HAS_WINDFURY] = initWeights[17];
            weights[M_IS_RUSH] = initWeights[18];
            weights[M_MANA_COST] = initWeights[19];
            weights[M_POISONOUS] = initWeights[20];
            weights[M_SILENCED] = initWeights[21];
            weights[M_SUMMONED] = initWeights[22];
            weights[M_CANT_BE_TARGETED_BY_SPELLS] = initWeights[23];
        }

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
            return getBestTask(poGame);
		}

        public void initWeights() 
        {
            weights[HERO_HEALTH_REDUCED] = 1;
            weights[HERO_ATTACK_REDUCED] = 1;
            weights[WEAPON_DURABILITY_REDUCED] = 1;
            weights[MINION_HEALTH_REDUCED] = 1;
            weights[MINION_ATTACK_REDUCED] = 1;
            weights[MINION_KILLED] = 1;
            weights[MINION_APPEARED] = 1;
            weights[SECRET_REMOVED] = 1;
            weights[MANA_REDUCED] = 1;
            weights[M_HEALTH] = 1;
            weights[M_ATTACK] = 1;
            weights[M_SUMMONED] = 1;
            weights[M_HAS_CHARGE] = 1;
            weights[M_HAS_DEAHTRATTLE] = 1;
            weights[M_HAS_DIVINE_SHIELD] = 1;
            weights[M_HAS_LIFE_STEAL] = 1;
            weights[M_HAS_STEALTH] = 1;
            weights[M_HAS_TAUNT] = 1;
            weights[M_HAS_WINDFURY] = 1;
            weights[M_IS_RUSH] = 1;
            weights[M_MANA_COST] = 1;
            weights[M_POISONOUS] = 1;
            // switch (poGame.)
        } 

        public PlayerTask getBestTask(SabberStoneCoreAi.POGame.POGame poGame) {
            double bestScore = Double.MinValue;
            PlayerTask bestTask = SabberStoneCore.Tasks.PlayerTasks.EndTurnTask.Any(poGame.CurrentPlayer);
            List<PlayerTask> tasksToSimulate = poGame.CurrentPlayer.Options();
            Dictionary<PlayerTask, SabberStoneCoreAi.POGame.POGame> simulation = poGame.Simulate(tasksToSimulate);
            
            foreach (PlayerTask task in tasksToSimulate) {
                double score = scoreTask(poGame, simulation[task]);
                if (task.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					score = 0;
				}

                if (score > bestScore) {
                    bestScore = score;
                    bestTask = task;
                }
            }

            return bestTask;
        }

        public double scoreTask(SabberStoneCoreAi.POGame.POGame currentState, SabberStoneCoreAi.POGame.POGame nextState) {
            if (nextState == null) return Double.MinValue;
            if (nextState.CurrentOpponent.Hero.Health <= 0) {
                return Double.MaxValue;
            }

            if (nextState.CurrentPlayer.Hero.Health <= 0) {
                return Double.MinValue;
            }

            double enemyHeroScore = scoreHero(currentState.CurrentOpponent, nextState.CurrentOpponent);
            double playerHeroScore = scoreHero(currentState.CurrentPlayer, nextState.CurrentPlayer);
            double enemyMinionsScore = scoreMinions(currentState.CurrentOpponent.BoardZone, nextState.CurrentOpponent.BoardZone);
            double playerMinionsScore = scoreMinions(currentState.CurrentPlayer.BoardZone, nextState.CurrentPlayer.BoardZone);
            double enemySecretsScore = scoreSecrets(currentState.CurrentOpponent, nextState.CurrentOpponent);
            double playerSecretsScore = scoreSecrets(currentState.CurrentPlayer, nextState.CurrentPlayer);
            double manaUsedScore = weights[MANA_REDUCED] * (currentState.CurrentPlayer.RemainingMana - nextState.CurrentPlayer.RemainingMana);
            
            return enemyHeroScore - playerHeroScore + enemyMinionsScore - playerMinionsScore + enemySecretsScore - playerSecretsScore - manaUsedScore;
        }

        public double scoreHero(SabberStoneCore.Model.Entities.Controller currentHero, SabberStoneCore.Model.Entities.Controller nextHero) {
            int healthDiff = (currentHero.Hero.Health + currentHero.Hero.Armor) - (nextHero.Hero.Health + nextHero.Hero.Armor);
            int attackDiff = currentHero.Hero.TotalAttackDamage - nextHero.Hero.TotalAttackDamage;
            int weaponDiff = (currentHero.Hero.Weapon!= null ? currentHero.Hero.Weapon.Durability : 0) - (nextHero.Hero.Weapon != null ? nextHero.Hero.Weapon.Durability : 0);
            return healthDiff * weights[HERO_HEALTH_REDUCED] + attackDiff * weights[HERO_ATTACK_REDUCED] + weaponDiff * weights[WEAPON_DURABILITY_REDUCED];
        }

        public double scoreMinions(SabberStoneCore.Model.Zones.BoardZone currentBoard, SabberStoneCore.Model.Zones.BoardZone nextBoard) {
            double scoreHealthReduced = 0;
			double scoreAttackReduced = 0;
            double scoreSilenced = 0;
			double scoreKilled = 0;
			double scoreSummoned = 0;
            //Add frozen, divine shield

            foreach (SabberStoneCore.Model.Entities.Minion currentMinion in currentBoard.GetAll()) {
                bool minionSurvived = false;

                foreach (SabberStoneCore.Model.Entities.Minion nextMinion in nextBoard.GetAll(minion => minion.Id == currentMinion.Id)) {
                    scoreHealthReduced += weights[MINION_HEALTH_REDUCED] * (currentMinion.Health - nextMinion.Health) * scoreMinion(currentMinion);
                    scoreAttackReduced += weights[MINION_ATTACK_REDUCED] * (currentMinion.AttackDamage - nextMinion.AttackDamage) * scoreMinion(currentMinion);
                    scoreSilenced += !currentMinion.IsSilenced && nextMinion.IsSilenced ? weights[M_SILENCED] * scoreMinion(currentMinion) : 0;
                    minionSurvived = true;
                }

                if (!minionSurvived) {
                    scoreKilled += weights[MINION_KILLED] * scoreMinion(currentMinion);
                }
            }

            foreach (SabberStoneCore.Model.Entities.Minion minion in nextBoard.GetAll().Except(currentBoard.GetAll())) {
                scoreSummoned += weights[M_SUMMONED] * scoreMinion(minion);
            }

            return scoreHealthReduced + scoreAttackReduced + scoreSilenced + scoreKilled - scoreSummoned;
        }

        public double scoreMinion(SabberStoneCore.Model.Entities.Minion minion) {
            double minionScore = 0;

            if (minion.HasCharge) minionScore += weights[M_HAS_CHARGE];
            if (minion.IsRush) minionScore += weights[M_IS_RUSH];
            if (minion.HasDeathrattle) minionScore += weights[M_HAS_DEAHTRATTLE];
            if (minion.HasDivineShield) minionScore += weights[M_HAS_DIVINE_SHIELD];
            if (minion.HasLifeSteal) minionScore += weights[M_HAS_LIFE_STEAL];
            if (minion.HasTaunt) minionScore += weights[M_HAS_TAUNT];
            if (minion.HasWindfury) minionScore += weights[M_HAS_WINDFURY];
            if (minion.HasStealth) minionScore += weights[M_HAS_STEALTH];
            if (minion.CantBeTargetedBySpells) minionScore += weights[M_CANT_BE_TARGETED_BY_SPELLS];
            if (minion.Poisonous) minionScore += weights[M_POISONOUS];
            minionScore += weights[M_MANA_COST] * minion.Card.Cost;
            return minionScore;
        }

        public double scoreSecrets(SabberStoneCore.Model.Entities.Controller currentHero, SabberStoneCore.Model.Entities.Controller nextHero) {
            return weights[SECRET_REMOVED] * (currentHero.SecretZone.Count - nextHero.SecretZone.Count);
        }

		public override void InitializeAgent()
		{
            // initWeights();
		}

		public override void InitializeGame()
		{
		}
    }
}
