﻿using Game.Combat;
using Game.Progression;
using Assets.Templates;

namespace Assets.Generators
{
	class UnitGenerator
	{
		private static readonly Dictionary<Unit, SpawnProfile> SPAWNABLE_UNITS = new()
		{
			{UnitTemplates.slime,			new SpawnProfile(1, 5)},
			{UnitTemplates.bandit,			new SpawnProfile(2, 5)},
			{UnitTemplates.imp,				new SpawnProfile(2, 6)},
			{UnitTemplates.fae,				new SpawnProfile(2, 6)},
			{UnitTemplates.mercenary,		new SpawnProfile(4, 2)},
			{UnitTemplates.antiHero,		new SpawnProfile(10, 2)},
			{UnitTemplates.archDemon,		new SpawnProfile(15, 1)}
		};

		public static Unit? MakeUnit(DifficultyProfile difficultyProfile)
		{
			var unit = EntityGenerator<Unit>.MakeEntity(SPAWNABLE_UNITS, difficultyProfile.Level);

			if (unit != null)
			{
				unit = new Unit(unit);
				GrowUnit(ref unit, difficultyProfile.Level);
			}

			return unit;
		}

		public static void GrowUnit(ref Unit unit, int levels)
		{
			while (levels > 0)
			{
				unit.LevelUp(Stats.GetRandomStat());
				levels--;
			}
		}
	}
}
