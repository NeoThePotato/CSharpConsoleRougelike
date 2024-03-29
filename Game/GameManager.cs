﻿using IO;
using IO.UI;
using Assets.Templates;

namespace Game
{
	class GameManager
	{
		public const int PLAYER_INVENTORY_SIZE = 10;

		public ulong CurrentTick
		{ get; private set; }
		public PlayerInputManager InputManager
		{ get; private set; }
		public GameUIManager UIManager
		{ get; private set; }
		public DataLog DataLog
		{ get => UIManager.DataLog; private set => UIManager.DataLog = value; }
		public LevelManager LevelManager
		{ get; private set; }
		public bool PendingExit
		{ get; private set; }

		public GameManager()
		{
			InputManager = new PlayerInputManager();
			UIManager = new GameUIManager(this);
			LevelManager = new LevelManager(this, DifficultyProfileTemplates.normalDifficulty);
		}

		public void Start()
		{
			LevelManager.Start();
		}

		public void Update(ulong currentTick)
		{
			CurrentTick = currentTick;
			InputManager.PollKeyBoard();
			LevelManager.Update(CurrentTick);
		}

		public void Exit()
		{
			PendingExit = true;
		}
	}
}
