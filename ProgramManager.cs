﻿using Game;
using IO.Render;
using System.Diagnostics;

class ProgramManager
{
	#region TIMING_FIELDS
	public const int GLOBAL_UPDATE_RATE = 60;
	public const double FRAMETIME_MILISEC = 1000.0 / GLOBAL_UPDATE_RATE;
	public const long FRAMETIME_TICKS = (long)(TimeSpan.TicksPerMillisecond * FRAMETIME_MILISEC);
	public readonly TimeSpan FRAMETIME = new(FRAMETIME_TICKS);
	private DateTime _startTime;
	private DateTime _logicFinishTime;
	private DateTime _renderFinishTime;
	#endregion
	#region CORE_COMPONENTS
	private readonly GameManager _gameManager;
	private ConsoleRenderer _consoleRenderer;
	private ulong _currentTick;
	#endregion

	#region TIMING_PROPERTIES
	private TimeSpan LogicUpdateTime
	{ get => _logicFinishTime - _startTime; }
	private TimeSpan RenderUpdateTime
	{ get => _renderFinishTime - _logicFinishTime; }
	private TimeSpan TotalUpdateTime
	{ get => _renderFinishTime - _startTime; }
	private TimeSpan TimeUntilNextFrame
	{ get => FRAMETIME > TotalUpdateTime ? FRAMETIME - TotalUpdateTime : new TimeSpan(0); }
	#endregion
	#region META_FLAGS
	private bool Exit
	{ get => _gameManager.PendingExit; }
	#endregion

	public ProgramManager()
	{
		_currentTick = 0;
		_gameManager = new GameManager();
		_gameManager.Start();
		_consoleRenderer = new ConsoleRenderer(new GameManagerRenderer(_gameManager));
	}

	public void Run()
	{
		while (!Exit)
		{
			UpdateLogic();
			UpdateRender();
			PrintFrameTimes();
			SleepUntilNextFrame();
			++_currentTick;
		}
	}

	private void UpdateLogic()
	{
		_startTime = DateTime.Now;
		_gameManager.Update(_currentTick);
		_logicFinishTime = DateTime.Now;
	}

	private void UpdateRender()
	{
		try
		{
			_consoleRenderer.RenderFrame(_currentTick);
		}
		catch (IndexOutOfRangeException)
		{
			Debug.WriteLine("ConsoleRenderer has crashed due to faulty buffer size. Restarting renderer...");
			_consoleRenderer = new ConsoleRenderer(new GameManagerRenderer(_gameManager));
		}
		_renderFinishTime = DateTime.Now;
	}

	private void SleepUntilNextFrame()
	{
		Thread.Sleep(TimeUntilNextFrame);
	}

	[Conditional("DEBUG")]
	private void PrintFrameTimes()
	{
		Debug.WriteLine($"Logic Time: {TimeSpanToMilliseconds(LogicUpdateTime)}ms\nRender Time: {TimeSpanToMilliseconds(RenderUpdateTime)}ms\nTotal update time: {TimeSpanToMilliseconds(TotalUpdateTime)}ms");
	}

	private static float TimeSpanToMilliseconds(TimeSpan ts)
	{
		return (float)ts.Ticks/TimeSpan.TicksPerMillisecond;
	}
}
