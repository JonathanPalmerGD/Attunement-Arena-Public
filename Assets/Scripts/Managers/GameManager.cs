using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	public Player[] players;

	public enum GamePhase { Intro, Tutorial, EndTutorial, MainPlay, BeginAccuse, Accusation, EndGame };
	public GamePhase currentPhase = GamePhase.Intro;
	public GamePhase CurrentPhase
	{
		get 
		{
			return currentPhase;
		}
		set 
		{
			currentPhase = value;
		}
	}

	void Start()
	{
	}

	public void AddPlayer()
	{

	}

	void Update()
	{
#if UNITY_EDITOR
		if(Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.K))
			{
				
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				CurrentPhase = GamePhase.MainPlay;
				
			}
		}
#endif
	}

	public void ProcessNPCRelocation()
	{
		
	}

	public void DoNothing()
	{

	}
}