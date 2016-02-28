﻿using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;

namespace Analytics {
	public enum GameMetricOptions {
		Death, Splits, Merges, Kills, Attacks, AttackTime, BattleEngagementTime
	}

	public enum GameLoggerOptions {
		StartGameMetrics, StopGameMetrics, GameIsPlaying, GameIsOver
	}

	[RequireComponent(typeof(CanvasGroup))]
	public class GameMetricLogger : MonoBehaviour {
		public KeyCode triggerKey;
		public InputField outputField;
		public CanvasGroup gameMetricsLogGroup;
		public StringBuilder stringBuilder;
		public static GameMetricLogger instance;

		//Game Metrics
		public int levelDifficulty;
		public int numberOfDeaths;
		public int numberOfSplits;
		public int numberOfMerges;
		public int numberOfKills;
		public int numberOfAttacks;
		public float totalGameTimeSinceEpoch;
		public float totalGameTime;
		public float totalAttackTime;
		public float totalBattleEngagementTime;
		public string playerName;
		public string difficultyEquations;

		//Flags
		public bool gameStartFlag {
			get; set;
		}
		public bool gameMetricLoggerStop {
			get; set;
		}

		public void Start() {
			this.outputField = this.GetComponentInChildren<InputField>();
			this.outputField.readOnly = true;
			this.stringBuilder = new StringBuilder();
			this.gameMetricsLogGroup = this.GetComponent<CanvasGroup>();
			Initialization();
			GameMetricLogger.instance = this;
		}

		public void Update() {
			this.outputField.GetComponentInChildren<Text>().text = this.outputField.text.ToString();

			if (Input.GetKeyUp(this.triggerKey)) {
				this.gameMetricsLogGroup.alpha = this.gameMetricsLogGroup.alpha > 0f ? 0f : 1f;
				this.gameMetricsLogGroup.interactable = !this.gameMetricsLogGroup.interactable;
				this.gameMetricsLogGroup.blocksRaycasts = !this.gameMetricsLogGroup.blocksRaycasts;
				GameMetricLogger.PrintLog();
			}

			if (!this.gameMetricLoggerStop) {
				this.totalGameTimeSinceEpoch += Time.deltaTime;
				if (this.gameStartFlag) {
					this.totalGameTime += Time.deltaTime;
				}
			}
		}

		public static void Increment(GameMetricOptions options) {
			switch (options) {
				case GameMetricOptions.Attacks:
					GameMetricLogger.instance.numberOfAttacks++;
					break;
				case GameMetricOptions.Death:
					GameMetricLogger.instance.numberOfDeaths++;
					break;
				case GameMetricOptions.Kills:
					GameMetricLogger.instance.numberOfKills++;
					break;
				case GameMetricOptions.Merges:
					GameMetricLogger.instance.numberOfMerges++;
					break;
				case GameMetricOptions.Splits:
					GameMetricLogger.instance.numberOfSplits++;
					break;
				case GameMetricOptions.AttackTime:
					GameMetricLogger.instance.totalAttackTime += Time.deltaTime;
					break;
				case GameMetricOptions.BattleEngagementTime:
					GameMetricLogger.instance.totalBattleEngagementTime += Time.deltaTime;
					break;
				default:
					Debug.LogError("Increment(): Invalid Game Metric Options. Please double check. Value: " + options.ToString());
					break;
			}
		}

		public static void Decrement(GameMetricOptions options) {
			//This method call should only be used very rarely. But it's worth putting it in for completeness.
			switch (options) {
				case GameMetricOptions.Attacks:
					GameMetricLogger.instance.numberOfAttacks--;
					break;
				case GameMetricOptions.Death:
					GameMetricLogger.instance.numberOfDeaths--;
					break;
				case GameMetricOptions.Kills:
					GameMetricLogger.instance.numberOfKills--;
					break;
				case GameMetricOptions.Merges:
					GameMetricLogger.instance.numberOfMerges--;
					break;
				case GameMetricOptions.Splits:
					GameMetricLogger.instance.numberOfSplits--;
					break;
				case GameMetricOptions.AttackTime:
					GameMetricLogger.instance.totalAttackTime -= Time.deltaTime;
					break;
				case GameMetricOptions.BattleEngagementTime:
					GameMetricLogger.instance.totalBattleEngagementTime -= Time.deltaTime;
					break;
				default:
					Debug.LogError("Decrement(): Invalid Game Metric Options. Please double check. Value: " + options.ToString());
					break;
			}
		}

		public static void SetPlayerName(string name) {
			GameMetricLogger.instance.playerName = name;
		}

		public static void SetDifficultyEquation(string equation) {
			GameMetricLogger.instance.difficultyEquations = equation;
		}

		public static void SetGameLogger(GameLoggerOptions options) {
			switch (options) {
				case GameLoggerOptions.StartGameMetrics:
					GameMetricLogger.instance.gameMetricLoggerStop = false;
					break;
				case GameLoggerOptions.StopGameMetrics:
					GameMetricLogger.instance.gameMetricLoggerStop = true;
					break;
				case GameLoggerOptions.GameIsPlaying:
					GameMetricLogger.instance.gameStartFlag = true;
					break;
				case GameLoggerOptions.GameIsOver:
					GameMetricLogger.instance.gameStartFlag = false;
					break;
				default:
					Debug.LogError("Invalid Game Logger Option : " + options.ToString());
					break;
			}
		}

		public static void ResetLogger() {
			GameMetricLogger.instance.Initialization();
		}

		public static void PrintLog() {
			GameMetricLogger log = GameMetricLogger.instance;
			if (GameMetricLogger.instance.stringBuilder == null) {
				Debug.LogError("Print(): Game metrics logger cannot output anything. Please double check.");
				log.outputField.text = "No game metrics report generated.";
			}
			else {
				StringBuilder sB = log.stringBuilder;
				sB.Length = 0;

				sB.AppendLine("Game Metrics Report");
				sB.AppendLine("(Please copy and paste this report somewhere else.)");
				sB.AppendLine();
				sB.AppendLine("Total Game Time Since Report Is Generated: " + log.totalGameTimeSinceEpoch.ToString("0.000") + " seconds");
				sB.AppendLine();
				sB.AppendLine("Player Name: " + log.playerName);
				sB.AppendLine("Level Difficulty: " + GetLevelDifficulty());
				sB.AppendLine("Unit Attribute Equation Used: " + log.difficultyEquations);
				sB.AppendLine();
				sB.AppendLine("Total Time Played: " + log.totalGameTime.ToString("0.000") + " seconds");
				sB.AppendLine("Total Death: " + log.numberOfDeaths);
				sB.AppendLine("Total Kills: " + log.numberOfKills);
				sB.AppendLine("Total Attacks: " + log.numberOfAttacks);
				sB.AppendLine("Total Splits: " + log.numberOfSplits);
				sB.AppendLine("Total Merges: " + log.numberOfMerges);
				sB.AppendLine();
				sB.AppendLine("Total Time Accumulated When Attacking: " + log.totalAttackTime.ToString("0.000") + " seconds");
				sB.AppendLine("Total Time Accumulated Under Attack: " + log.totalBattleEngagementTime.ToString("0.000") + " seconds");
				sB.AppendLine();

				log.outputField.text = sB.ToString();
				log.outputField.Rebuild(CanvasUpdate.MaxUpdateValue);
				Canvas.ForceUpdateCanvases();
			}
		}

		// ------------   Private variables  ------------------------------

		private static string GetLevelDifficulty() {
			switch (GameMetricLogger.instance.levelDifficulty) {
				case 0:
					return "Easy Difficulty";
				case 1:
					return "Normal Difficulty";
				case 2:
					return "Hard Difficulty";
				case 3:
					return "Custom Difficulty";
				default:
					return "UNKNOWN LEVEL DIFFICULTY";
			}
		}

		private void Initialization() {
			//Integers
			this.levelDifficulty = -1;
			this.numberOfAttacks = 0;
			this.numberOfDeaths = 0;
			this.numberOfKills = 0;
			this.numberOfMerges = 0;
			this.numberOfSplits = 0;

			//Floats
			this.totalGameTimeSinceEpoch = 0f;
			this.totalGameTime = 0f;
			this.totalBattleEngagementTime = 0f;
			this.totalAttackTime = 0f;

			//Strings
			this.playerName = "Player";
			this.difficultyEquations = "N/A (Not used.)";

			//Flags
			this.gameMetricLoggerStop = false;
			this.gameStartFlag = false;

			//Canvas Group
			this.gameMetricsLogGroup.alpha = 0f;
			this.gameMetricsLogGroup.interactable = false;
			this.gameMetricsLogGroup.blocksRaycasts = false;
		}
	}
}