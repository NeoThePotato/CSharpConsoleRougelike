﻿using Game;
using Game.Combat;
using Game.Items;
using Game.Items.Equipment;

namespace IO.UI.Menus
{
	static class MenuFactory
	{
		public static SelectionMenu GetConfirmPortalMenu(LevelManager levelManager)
		{
			var parentUIManager = levelManager.UIManager;
			void onTrue() => levelManager.MoveToNextLevel();
			void onFalse() => levelManager.UIManager.ForceExitCurrentMenu();

			return GetConfirmMenu(levelManager.InputManager, parentUIManager, "Are you sure you want to proceed?", onTrue, onFalse);
		}

		public static SelectionMenu GetInGameMenu(LevelManager levelManager)
		{
			var parentUIManager = levelManager.UIManager;
			var playerUnit = (Unit)levelManager.PlayerEntity.Entity;
			void returnToGame() =>		parentUIManager.ForceExitCurrentMenu();
			void openStatsMenu() =>		levelManager.UIManager.StackNewMenu(GetPlayerStatsMenu(levelManager.InputManager, parentUIManager, playerUnit));
			void openInventoryMenu() =>	levelManager.UIManager.StackNewMenu(GetContainerMenu(levelManager.InputManager, parentUIManager, playerUnit, levelManager.PlayerInventory));
			void quitGame() =>			levelManager.Exit();

			var actions = new Dictionary<string, Action?>()
			{
				{ "Return", returnToGame},
				{ $"{playerUnit.Name}", openStatsMenu},
				{ "Inventory", openInventoryMenu},
				{ "Quit", quitGame}
			};

			return new SelectionMenu(levelManager.InputManager, parentUIManager, actions, 4, 1);
		}

		public static ContainerMenu GetContainerMenu(PlayerInputManager inputManager, GameUIManager parentUIManager, Unit playerUnit, params Container[] containers)
		{
			return new ContainerMenu(inputManager, parentUIManager, playerUnit, containers);
		}

		public static SelectionMenu GetEquipmentMenu(PlayerInputManager inputManager, GameUIManager parentUIManager, ContainerMenu parent, Equipment equipment, Unit playerUnit)
		{
			void equip() { parent.EquipSelectedOnUnit(playerUnit); parentUIManager.ForceExitCurrentMenu(); }
			void discard() { parent.RemoveItemAtSelection()!.DiscardThis(playerUnit, parentUIManager.DataLog); parentUIManager.ForceExitCurrentMenu(); }

			var actions = new Dictionary<string, Action?>()
			{
				{ "Equip", equip},
				{ "Discard", discard},
			};

			return new SelectionMenu(inputManager, parentUIManager, actions, 2, 1, equipment.Name);
		}

		public static SelectionMenu GetPlayerStatsMenu(PlayerInputManager inputManager, GameUIManager parentUIManager, Unit playerUnit)
		{
			var statsMenuText = playerUnit.GetStats();
			void restartStatsMenu()
			{
				parentUIManager.ForceExitCurrentMenu();
				parentUIManager.ForceExitCurrentMenu();
				parentUIManager.StackNewMenu(GetPlayerStatsMenu(inputManager, parentUIManager, playerUnit));
			}
			void back() => parentUIManager.ForceExitCurrentMenu();
			void heal()
			{
				playerUnit.HealSelf(parentUIManager.DataLog);
				restartStatsMenu();
			}
			void levelUp() => parentUIManager.StackNewMenu(GetPlayerLevelUpMenu(inputManager, parentUIManager, playerUnit));

			var actions = new Dictionary<string, Action?>()
			{
				{"Back", back},
				{"Heal", heal}
			};

			if (playerUnit.CanLevelUp)
				actions.Add("Level Up", levelUp);

			return new SelectionMenu(inputManager, parentUIManager, actions, 1, actions.Count, "Stats", statsMenuText);
		}

		public static SelectionMenu GetPlayerLevelUpMenu(PlayerInputManager inputManager, GameUIManager parentUIManager, Unit playerUnit)
		{
			var unitStats = playerUnit.Stats;
			var level = playerUnit.Level;
			void restartStatsMenu()
			{
				parentUIManager.ForceExitCurrentMenu();
				parentUIManager.ForceExitCurrentMenu();
				parentUIManager.StackNewMenu(GetPlayerStatsMenu(inputManager, parentUIManager, playerUnit));
			}
			void levelUpAndGoBackToStatsMenu(Stats.Stat stat) { playerUnit.LevelUp(stat); restartStatsMenu(); }
			void vitUp() => levelUpAndGoBackToStatsMenu(Stats.Stat.VIT);
			void strUp() => levelUpAndGoBackToStatsMenu(Stats.Stat.STR);
			void spdUp() => levelUpAndGoBackToStatsMenu(Stats.Stat.SPD);
			void intUp() => levelUpAndGoBackToStatsMenu(Stats.Stat.INT);
			void back() => parentUIManager.ForceExitCurrentMenu();

			var actions = new Dictionary<string, Action?>()
			{
				{ $"VIT: {unitStats.Vitality} -> {unitStats.Vitality+1}", vitUp},
				{ $"STR: {unitStats.Strength} -> {unitStats.Strength+1}", strUp},
				{ $"SPD: {unitStats.Speed} -> {unitStats.Speed+1}", spdUp},
				{ $"INT: {unitStats.Intelligence} -> {unitStats.Intelligence+1}", intUp},
				{"Back", back}
			};

			return new SelectionMenu(inputManager, parentUIManager, actions, actions.Count, 1, $"Lv {level} -> {level+1}");
		}

		private static SelectionMenu GetConfirmMenu(PlayerInputManager inputManager, GameUIManager parentUIManager, string query, Action onTrue, Action onFalse)
		{
			var actions = new Dictionary<string, Action?>()
			{
				{ "Yes", onTrue},
				{ "No", onFalse},
			};

			return new SelectionMenu(inputManager, parentUIManager, actions, 1, 2, query);
		}
	}
}
