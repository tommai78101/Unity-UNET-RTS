﻿using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;

namespace Analytics {
	public enum GameMetricOptions {
		Death, Splits, Merges, Kills, Attacks, AttackTime, BattleEngagementTime, Wins, Losses
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

		public bool debugFlag;

		//Temporary memory space
		private string metricsReportText;

		//Flags
		[SerializeField]
		public bool gameStartFlag;
		[SerializeField]
		public bool gameMetricLoggerStart;
		[SerializeField]
		public bool isInputEnabled;
		[SerializeField]
		public bool isShownToScreen;

		public void Start() {
			Initialization();

			GameMetricLogger.instance = this;
		}

		public void Update() {
			if (this.debugFlag) {
				this.outputField.text = this.metricsReportText;
			}

			if (!this.isInputEnabled) {
				return;
			}

			if (Input.GetKeyUp(this.triggerKey)) {
				ToggleCanvasGroup();
				GameMetricLogger.PrintLog();
			}

			if (this.gameMetricLoggerStart) {
				this.totalGameTimeSinceEpoch += Time.deltaTime;
				if (this.gameStartFlag) {
					this.totalGameTime += Time.deltaTime;
				}
			}
		}

		public static void Increment(GameMetricOptions options) {
			if (!(GameMetricLogger.instance.gameMetricLoggerStart || GameMetricLogger.instance.gameStartFlag)){
				Debug.LogWarning("Cannot increment. Game Metrics Logger isn't completely enabled.");
				return;
			}

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
			//Check if logger is activated.
			if (!(GameMetricLogger.instance.gameMetricLoggerStart || GameMetricLogger.instance.gameStartFlag)) {
				Debug.LogWarning("Cannot decrement. Game Metrics Logger isn't completely enabled.");
				return;
			}

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
					GameMetricLogger.instance.gameMetricLoggerStart = true;
					break;
				case GameLoggerOptions.StopGameMetrics:
					GameMetricLogger.instance.gameMetricLoggerStart = false;
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

		public static void ShowPrintLog() {
			GameMetricLogger.instance.EnableCanvasGroup();
			GameMetricLogger.instance.Print();
		}

		public static void PrintLog() {
			if (GameMetricLogger.instance.stringBuilder == null) {
				GameMetricLogger log = GameMetricLogger.instance;
				Debug.LogError("Print(): Game metrics logger cannot output anything. Please double check.");
				log.outputField.text = "No game metrics report generated.";
			}
			else {
				GameMetricLogger.instance.Print();
			}
		}

		public static void EnableLoggerHotkey() {
			GameMetricLogger.instance.isInputEnabled = true;
		}

		public static void DisableLoggerHotkey() {
			GameMetricLogger.instance.isInputEnabled = false;
		}

		public void EnableCanvasGroup() {
			this.gameMetricsLogGroup.alpha = 1f;
			this.gameMetricsLogGroup.interactable = true;
			this.gameMetricsLogGroup.blocksRaycasts = true;
			this.isShownToScreen = true;
		}

		public void DisableCanvasGroup() {
			this.gameMetricsLogGroup.alpha = 0f;
			this.gameMetricsLogGroup.interactable = false;
			this.gameMetricsLogGroup.blocksRaycasts = false;
			this.isShownToScreen = false;
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

			//Flags and class members
			this.outputField = this.GetComponentInChildren<InputField>();
			this.outputField.readOnly = true;
			this.outputField.lineType = InputField.LineType.MultiLineNewline;
			this.outputField.characterLimit = 0;
			this.stringBuilder = new StringBuilder();
			this.gameMetricsLogGroup = this.GetComponent<CanvasGroup>();
			this.gameMetricLoggerStart = false;
			this.gameStartFlag = false;
			this.isInputEnabled = false;
			this.metricsReportText = "";

			//Check if name and difficulty is valid
			GameObject obj = GameObject.FindGameObjectWithTag("Name");
			if (obj != null) {
				InputField field = obj.GetComponent<InputField>();
				this.playerName = field.text;
			}
			obj = GameObject.FindGameObjectWithTag("Difficulty");
			if (obj != null) {
				Dropdown down = obj.GetComponent<Dropdown>();
				this.levelDifficulty = down.value;
				this.difficultyEquations = down.options[down.value].text;
			}

			//Canvas Group
			DisableCanvasGroup();
		}

		private void ToggleCanvasGroup() {
			this.gameMetricsLogGroup.alpha = this.gameMetricsLogGroup.alpha > 0f ? 0f : 1f;
			this.gameMetricsLogGroup.interactable = !this.gameMetricsLogGroup.interactable;
			this.gameMetricsLogGroup.blocksRaycasts = !this.gameMetricsLogGroup.blocksRaycasts;
		}

		private void Print() {
			GameMetricLogger log = GameMetricLogger.instance;
			StringBuilder sB = log.stringBuilder;
			sB.Length = 0;

			sB.AppendLine("Game Metrics Report");
			sB.AppendLine("(Please copy, paste, and SAVE this report! Research purposes only. Press M to show/hide Metrics Report.)");
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
			sB.AppendLine("Report Ends Here");
			sB.AppendLine();
			sB.AppendLine("--------------------EXCEL DATA (SYLK)---------------------");
			sB.AppendLine("-------------------DO NOT MODIFY THIS!--------------------");
			sB.AppendLine("------------------FOR RESEARCHERS ONLY!-------------------");
			sB.AppendLine();
			sB.AppendLine("ID;P");
			sB.AppendLine(CreateCell(1, 1, "Player Name"));
			sB.AppendLine(CreateCell(2, 1, log.playerName));
			sB.AppendLine(CreateCell(1, 2, "Level Difficulty"));
			sB.AppendLine(CreateCell(2, 2, GetLevelDifficulty()));
			sB.AppendLine(CreateCell(1, 3, "Equation Used"));
			sB.AppendLine(CreateCell(2, 3, log.difficultyEquations));
			sB.AppendLine(CreateCell(1, 4, "Total Game Time (seconds)"));
			sB.AppendLine(CreateCell(2, 4, log.totalGameTimeSinceEpoch.ToString("0.000")));
			sB.AppendLine(CreateCell(1, 5, "Total Time Played (seconds)"));
			sB.AppendLine(CreateCell(2, 5, log.totalGameTime.ToString("0.000")));
			sB.AppendLine(CreateCell(1, 6, "Total Deaths"));
			sB.AppendLine(CreateCell(2, 6, log.numberOfDeaths.ToString()));
			sB.AppendLine(CreateCell(1, 7, "Total Kills"));
			sB.AppendLine(CreateCell(2, 7, log.numberOfKills.ToString()));
			sB.AppendLine(CreateCell(1, 8, "Total Attacks"));
			sB.AppendLine(CreateCell(2, 8, log.numberOfAttacks.ToString()));
			sB.AppendLine(CreateCell(1, 9, "Total Splits"));
			sB.AppendLine(CreateCell(2, 9, log.numberOfSplits.ToString()));
			sB.AppendLine(CreateCell(1, 10, "Total Merges"));
			sB.AppendLine(CreateCell(2, 10, log.numberOfMerges.ToString()));
			sB.AppendLine(CreateCell(1, 11, "Total Attack Time Accumulated"));
			sB.AppendLine(CreateCell(2, 11, log.totalAttackTime.ToString()));
			sB.AppendLine(CreateCell(1, 11, "Total Battle Time Accumulated"));
			sB.AppendLine(CreateCell(2, 11, log.totalBattleEngagementTime.ToString()));
			sB.AppendLine("E");
			sB.AppendLine();

			this.metricsReportText = sB.ToString();
			this.outputField.GetComponentInChildren<Text>().text = sB.ToString();
			log.outputField.Rebuild(CanvasUpdate.Layout);
			Canvas.ForceUpdateCanvases();
		}

		private string CreateCell(int x, int y, string tag) {
			return "C;Y" + y + ";X" + x + ";K\"" + tag + "\"";
		}
	}
}