﻿using IO.UI;

namespace Game.Combat
{
	class CombatManager
	{
		private Unit PlayerUnit
		{ get; set; }
		private Unit CPUUnit
		{ get; set; }
		private DataLog DataLog
		{ get; set; }
		private CPUAI AI
		{ get; set; }
		private bool BothAlive
		{ get => !PlayerUnit.Dead && !CPUUnit.Dead; }

		public CombatManager(Unit playerUnit, Unit cpuUnit, DataLog dataLog)
		{
			PlayerUnit = playerUnit;
			CPUUnit = cpuUnit;
			DataLog = dataLog;
			AI = new CPUAI(CPUUnit, PlayerUnit);
		}

		public Unit Combat()
		{
			CombatInitiate();
			CombatLoop();
			CombatEnd();

			return GetWinner();
		}

		private void CombatInitiate()
		{
			Console.Clear();
			Console.WriteLine($"{PlayerUnit} has encountered a {CPUUnit}.");
			Utility.BlockUntilKeyDown();
		}

		private void CombatLoop()
		{
			Unit actingUnit = PlayerUnit;
			Unit passiveUnit = CPUUnit;

			// Main combat loop
			while (BothAlive)
			{
				// Print combat status
				Console.Clear();
				Console.WriteLine(
					$"{PlayerUnit.GetCombatStats()}\n\n" +
					$"{CPUUnit.GetCombatStats()}\n\n" +
					$"{actingUnit}'s turn.\n");

				// Action phase
				UnitAction action = actingUnit == PlayerUnit ? GetPlayerAction() : GetCPUAction();
				DoAction(action, actingUnit, passiveUnit);

				// End of action phase
				Console.WriteLine(DataLog.Last()); // TODO Temporary until I have CombatManager as an updateable
				Utility.BlockUntilKeyDown();

				// Prepare for next turn
				Utility.Swap(ref actingUnit, ref passiveUnit);
			}
		}

		private void CombatEnd()
		{
			Unit winner = GetWinner();
			Unit loser = GetLoser();
			int xpGain = loser.GetExpOnDeath();
			DataLog.WriteLine($"{winner} has defeated {loser} and gained {xpGain} Exp");
			winner.AddExp(xpGain, DataLog);
			winner.ResetPostCombatTempStats();
		}

		private UnitAction GetPlayerAction()
		{
			Console.WriteLine(
				$"What will {PlayerUnit} do?\n" +
				$"{GetPlayerOptions()}");

			return GetPlayerInput() switch
			{
				1 => UnitAction.Attack,
				2 => UnitAction.Defend,
				3 => UnitAction.Heal,
				_ => UnitAction.Attack,
			};
		}

		private UnitAction GetCPUAction()
		{
			Thread.Sleep(300);

			return AI.GetCPUAction();
		}

		private void DoAction(UnitAction action, Unit actingUnit, Unit passiveUnit)
		{
			switch (action)
			{
				case UnitAction.Attack:
					actingUnit.AttackOther(passiveUnit, DataLog);
					break;
				case UnitAction.Defend:
					actingUnit.RaiseShield(DataLog);
					break;
				case UnitAction.Heal:
					actingUnit.HealSelf(DataLog);
					break;
			}
		}

		private static string GetPlayerOptions()
		{
			return	"1: Attack\n2: Defend\n3: Heal";
		}

		private static int GetPlayerInput()
		{
			while (true)
			{
				if (!int.TryParse(Console.ReadLine(), out int input) || 1 > input || input > 3)
				{
					(int left, int top) = Console.GetCursorPosition();
					Console.SetCursorPosition(left, top - 1);
					continue;
				}

				return input;
			}
		}

		private Unit GetWinner()
		{
			return CPUUnit.Dead ? PlayerUnit : CPUUnit;
		}

		private Unit GetLoser()
		{
			return PlayerUnit.Dead ? PlayerUnit : CPUUnit;
		}
	}

	enum UnitAction
	{
		Attack,
		Defend,
		Heal
	}

	readonly struct CPUAI
	{
		private readonly Unit PlayerUnit;
		private readonly Unit CPUUnit;

		// Flags useful for AI decision-making
		private readonly int DamageToPlayer
		{ get => PlayerUnit.CalculateTotalDamageFrom(CPUUnit); }
		private readonly int DamageFromPlayer
		{ get => CPUUnit.CalculateTotalDamageFrom(PlayerUnit); }
		private readonly bool PlayerDiesIfHit
		{ get => DamageToPlayer >= PlayerUnit.CurrentHP; }
		private readonly bool CPUDiesIfHit
		{ get => DamageFromPlayer >= CPUUnit.CurrentHP; }
		private readonly bool CPUCanHeal
		{ get => CPUUnit.EffectiveHealPower > 0; }
		private readonly bool CPUCanBlock
		{ get => !CPUUnit.Blocking; }
		private readonly bool CPUCanOutHealFatalAttack
		{ get => CPUCanHeal && CPUUnit.CurrentHP + CPUUnit.EffectiveHealPower - DamageFromPlayer > 0; }
		private readonly bool CPUCanBlockFatalAttack
		{ get => CPUCanBlock && CPUUnit.CurrentHP + CPUUnit.EffectiveDefense + CPUUnit.Shield.Defense - DamageFromPlayer > 0; }
		private readonly bool CPUShouldHeal
		{ get => CPUDiesIfHit && CPUCanOutHealFatalAttack; }
		private readonly bool CPUShouldAttack
		{ get => PlayerDiesIfHit || !CPUDiesIfHit; }
		private readonly bool CPUShouldBlock
		{ get => CPUDiesIfHit && CPUCanBlockFatalAttack; }

		public CPUAI(Unit cpuUnit, Unit playerUnit)
		{
			CPUUnit = cpuUnit;
			PlayerUnit = playerUnit;
		}

		public readonly UnitAction GetCPUAction()
		{
			if (CPUShouldHeal)
				return UnitAction.Heal; // Highest priority
			else if (CPUShouldAttack)
				return UnitAction.Attack; // Usually happens
			else if (CPUShouldBlock)
				return UnitAction.Defend; // Happens when healing runs out
			else
				return UnitAction.Attack; // Desperate efforts
		}
	}
}
