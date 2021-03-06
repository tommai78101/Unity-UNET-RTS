﻿using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using Simulation;

namespace Analytics {
	public struct TeamMetric {
		public int levelDifficulty;
		public int numberOfDeaths;
		public int numberOfSplits;
		public int numberOfMerges;
		public int numberOfKills;
		public int numberOfAttacks;
		public int winCount;
		public int lossCount;
		public float totalGameTimeSinceEpoch;
		public float totalGameTime;
		public float totalAttackTime;
		public float totalBattleEngagementTime;
		public string teamName;
		public string difficultyEquations;
	}

	[RequireComponent(typeof(CanvasGroup))]
	public class SimulationMetricsLogger : MonoBehaviour {
		public KeyCode triggerKey;
		public InputField outputField;
		public CanvasGroup gameMetricsLogGroup;
		public StringBuilder stringBuilder;
		public SimulationManager simulationManager;

		public static SimulationMetricsLogger instance;

		//Game Metrics for each team
		public List<TeamMetric> teamMetrics;

		//Flags
		public bool gameStartFlag
		{
			get; set;
		}
		public bool simulationMetricsLoggerStart
		{
			get; set;
		}
		public bool isInputEnabled
		{
			get; set;
		}
		public bool isShownToScreen
		{
			get; set;
		}

		public void Start() {
			Initialization();

			SimulationMetricsLogger.instance = this;
		}

		public void Update() {
			this.outputField.GetComponentInChildren<Text>().text = this.outputField.text.ToString();

			if (Input.GetKeyUp(this.triggerKey)) {
				ToggleCanvasGroup();
				SimulationMetricsLogger.PrintLog();
			}

			for (int i = 0; i < this.teamMetrics.Count; i++) {
				if (this.simulationMetricsLoggerStart) {
					TeamMetric temp = this.teamMetrics[i];
					temp.totalGameTimeSinceEpoch += Time.deltaTime;
					if (this.gameStartFlag) {
						temp.totalGameTime += Time.deltaTime;
					}
					this.teamMetrics[i] = temp;
				}
			}
		}

		public void ShowPrintLog() {
			this.EnableCanvasGroup();
			this.stringBuilder.Length = 0;
			RectTransform rect = this.outputField.GetComponent<RectTransform>();
			Vector3 pos = rect.sizeDelta;
			pos.y += 10;
			rect.sizeDelta = pos;
			this.Print();
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

		//---------------------   STATIC METHODS   --------------------------------

		public static void Increment(GameMetricOptions options, int index) {
			if (SimulationMetricsLogger.instance == null) {
				return;
			}

			if (!(SimulationMetricsLogger.instance.simulationMetricsLoggerStart || SimulationMetricsLogger.instance.gameStartFlag)) {
				Debug.LogWarning("Cannot increment. Game Metrics Logger isn't completely enabled.");
				return;
			}

			TeamMetric metric = SimulationMetricsLogger.instance.teamMetrics[index];
			switch (options) {
				case GameMetricOptions.Attacks:
					metric.numberOfAttacks++;
					break;
				case GameMetricOptions.Death:
					metric.numberOfDeaths++;
					break;
				case GameMetricOptions.Kills:
					metric.numberOfKills++;
					break;
				case GameMetricOptions.Merges:
					metric.numberOfMerges++;
					break;
				case GameMetricOptions.Splits:
					metric.numberOfSplits++;
					break;
				case GameMetricOptions.AttackTime:
					metric.totalAttackTime += Time.deltaTime;
					break;
				case GameMetricOptions.BattleEngagementTime:
					metric.totalBattleEngagementTime += Time.deltaTime;
					break;
				case GameMetricOptions.Wins:
					metric.winCount++;
					break;
				case GameMetricOptions.Losses:
					metric.lossCount++;
					break;
				default:
					Debug.LogError("Increment(): Invalid Game Metric Options. Please double check. Value: " + options.ToString());
					break;
			}
			SimulationMetricsLogger.instance.teamMetrics[index] = metric;
		}

		public static void Decrement(GameMetricOptions options, int index) {
			if (SimulationMetricsLogger.instance == null) {
				return;
			}

			//Check if logger is activated.
			if (!(SimulationMetricsLogger.instance.simulationMetricsLoggerStart || SimulationMetricsLogger.instance.gameStartFlag)) {
				Debug.LogWarning("Cannot decrement. Game Metrics Logger isn't completely enabled.");
				return;
			}

			TeamMetric metric = SimulationMetricsLogger.instance.teamMetrics[index];
			//This method call should only be used very rarely. But it's worth putting it in for completeness.
			switch (options) {
				case GameMetricOptions.Attacks:
					metric.numberOfAttacks--;
					break;
				case GameMetricOptions.Death:
					metric.numberOfDeaths--;
					break;
				case GameMetricOptions.Kills:
					metric.numberOfKills--;
					break;
				case GameMetricOptions.Merges:
					metric.numberOfMerges--;
					break;
				case GameMetricOptions.Splits:
					metric.numberOfSplits--;
					break;
				case GameMetricOptions.AttackTime:
					metric.totalAttackTime -= Time.deltaTime;
					break;
				case GameMetricOptions.BattleEngagementTime:
					metric.totalBattleEngagementTime -= Time.deltaTime;
					break;
				case GameMetricOptions.Wins:
					metric.winCount--;
					break;
				case GameMetricOptions.Losses:
					metric.lossCount--;
					break;
				default:
					Debug.LogError("Decrement(): Invalid Game Metric Options. Please double check. Value: " + options.ToString());
					break;
			}
			SimulationMetricsLogger.instance.teamMetrics[index] = metric;
		}

		public static void SetPlayerName(string name, int index) {
			TeamMetric metric = SimulationMetricsLogger.instance.teamMetrics[index];
			metric.teamName = name;
			SimulationMetricsLogger.instance.teamMetrics[index] = metric;
		}

		public static void SetDifficultyEquation(string equation, int index) {
			TeamMetric metric = SimulationMetricsLogger.instance.teamMetrics[index];
			metric.difficultyEquations = equation;
			SimulationMetricsLogger.instance.teamMetrics[index] = metric;
		}

		public static void SetGameLogger(GameLoggerOptions options) {
			switch (options) {
				case GameLoggerOptions.StartGameMetrics:
					SimulationMetricsLogger.instance.simulationMetricsLoggerStart = true;
					break;
				case GameLoggerOptions.StopGameMetrics:
					SimulationMetricsLogger.instance.simulationMetricsLoggerStart = false;
					break;
				case GameLoggerOptions.GameIsPlaying:
					SimulationMetricsLogger.instance.gameStartFlag = true;
					break;
				case GameLoggerOptions.GameIsOver:
					SimulationMetricsLogger.instance.gameStartFlag = false;
					break;
				default:
					Debug.LogError("Invalid Game Logger Option : " + options.ToString());
					break;
			}
		}

		public static void ResetLogger() {
			SimulationMetricsLogger.instance.Initialization();
		}

		public static void PrintLog() {
			if (SimulationMetricsLogger.instance.stringBuilder == null) {
				SimulationMetricsLogger log = SimulationMetricsLogger.instance;
				Debug.LogError("Print(): Game metrics logger cannot output anything. Please double check.");
				log.outputField.text = "No game metrics report generated.";
			}
			else {
				SimulationMetricsLogger.instance.stringBuilder.Length = 0;
				SimulationMetricsLogger.instance.Print();
			}
		}

		public static void EnableLoggerHotkey() {
			SimulationMetricsLogger.instance.isInputEnabled = true;
		}

		public static void DisableLoggerHotkey() {
			SimulationMetricsLogger.instance.isInputEnabled = false;
		}

		// ------------   Private variables  ------------------------------

		private static string GetLevelDifficulty(int index) {
			TeamMetric metric = SimulationMetricsLogger.instance.teamMetrics[index];
			switch (metric.levelDifficulty) {
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
			string[] teamLabels = {
				"Yellow Team", "Blue Team"
			};
			if (this.teamMetrics == null) {
				this.teamMetrics = new List<TeamMetric>();
			}
			for (int i = 0; i < teamLabels.Length; i++) {
				if (this.teamMetrics.Count < teamLabels.Length) {
					TeamMetric metric = new TeamMetric();

					//Integers
					metric.levelDifficulty = -1;
					metric.numberOfAttacks = 0;
					metric.numberOfDeaths = 0;
					metric.numberOfKills = 0;
					metric.numberOfMerges = 0;
					metric.numberOfSplits = 0;
					metric.winCount = 0;
					metric.lossCount = 0;

					//Floats
					metric.totalGameTimeSinceEpoch = 0f;
					metric.totalGameTime = 0f;
					metric.totalBattleEngagementTime = 0f;
					metric.totalAttackTime = 0f;

					//Strings
					metric.teamName = teamLabels[i];
					metric.difficultyEquations = "N/A (Not used.)";

					this.teamMetrics.Add(metric);
				}
				else {
					ResetMetric(teamLabels[i], i);
				}
			}

			//Flags and class members
			this.outputField = this.GetComponentInChildren<InputField>();
			this.outputField.readOnly = true;
			if (this.stringBuilder == null) {
				this.stringBuilder = new StringBuilder();
			}
			else {
				this.stringBuilder.Length = 0;
			}
			this.gameMetricsLogGroup = this.GetComponent<CanvasGroup>();
			this.simulationMetricsLoggerStart = false;
			this.gameStartFlag = false;
			this.isInputEnabled = false;
			this.simulationManager = GameObject.FindObjectOfType<SimulationManager>();
			if (this.simulationManager == null) {
				Debug.LogError("Couldn't find simulation manager.");
			}

			//Canvas Group
			DisableCanvasGroup();
		}

		private void ResetMetric(string teamLabel, int index) {
			TeamMetric metric = this.teamMetrics[index];

			//Integers
			metric.levelDifficulty = -1;
			metric.numberOfAttacks = 0;
			metric.numberOfDeaths = 0;
			metric.numberOfKills = 0;
			metric.numberOfMerges = 0;
			metric.numberOfSplits = 0;
			metric.winCount = 0;
			metric.lossCount = 0;

			//Floats
			metric.totalGameTimeSinceEpoch = 0f;
			metric.totalGameTime = 0f;
			metric.totalBattleEngagementTime = 0f;
			metric.totalAttackTime = 0f;

			//Strings
			metric.teamName = teamLabel;
			metric.difficultyEquations = "N/A (Not used.)";

			this.teamMetrics[index] = metric;
		}

		private void ToggleCanvasGroup() {
			this.gameMetricsLogGroup.alpha = this.gameMetricsLogGroup.alpha > 0f ? 0f : 1f;
			this.gameMetricsLogGroup.interactable = !this.gameMetricsLogGroup.interactable;
			this.gameMetricsLogGroup.blocksRaycasts = !this.gameMetricsLogGroup.blocksRaycasts;
		}

		private void Print() {
			StringBuilder sB = this.stringBuilder;

			sB.AppendLine();
			sB.AppendLine("Simulation Game Metrics Report");
			sB.AppendLine("(Please copy and paste the report to any text editor in order to save the report.)");
			sB.AppendLine("------------------------------------------------------------------");
			sB.AppendLine();
			sB.AppendLine("Total Sessions: " + this.simulationManager.simulationStarter.sessionNumberText.text);
			sB.AppendLine();

			Debug.Log("Team Metrics Count: " + this.teamMetrics.Count);
			for (int index = 0; index < this.teamMetrics.Count; index++) {
				sB.AppendLine("------------------------------------------------------------------");
				sB.AppendLine();
				TeamMetric log = SimulationMetricsLogger.instance.teamMetrics[index];
				sB.AppendLine("Team Name: " + log.teamName);
				sB.AppendLine("Total Game Time Since Report Is Generated: " + log.totalGameTimeSinceEpoch.ToString("0.000") + " seconds");
				sB.AppendLine();
				sB.AppendLine("Level Difficulty: " + GetLevelDifficulty(index));
				sB.AppendLine("Unit Attribute Equation Used: " + log.difficultyEquations);
				sB.AppendLine();
				sB.AppendLine("Total Time Played: " + log.totalGameTime.ToString("0.000") + " seconds");
				sB.AppendLine("Total Death: " + log.numberOfDeaths);
				sB.AppendLine("Total Kills: " + log.numberOfKills);
				sB.AppendLine("Total Attacks: " + log.numberOfAttacks);
				sB.AppendLine("Total Splits: " + log.numberOfSplits);
				sB.AppendLine("Total Merges: " + log.numberOfMerges);
				sB.AppendLine();
				sB.AppendLine("Wins: " + log.winCount);
				sB.AppendLine("Losses: " + log.lossCount);
				sB.AppendLine();
				sB.AppendLine("Total Time Accumulated When Attacking: " + log.totalAttackTime.ToString("0.000") + " seconds");
				sB.AppendLine("Total Time Accumulated Under Attack: " + log.totalBattleEngagementTime.ToString("0.000") + " seconds");
				sB.AppendLine();
			}

			this.outputField.text = sB.ToString();
			this.outputField.Rebuild(CanvasUpdate.MaxUpdateValue);
			Canvas.ForceUpdateCanvases();
		}
	}
}