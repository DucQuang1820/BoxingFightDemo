using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode
{
    OneVOne,
    OneVMul,
    MulVMul
}

public class GameController : MonoBehaviour
{
    private static readonly int IsKo = Animator.StringToHash("isKO");
    private static readonly int IsVictory = Animator.StringToHash("isVictory");
    public GameObject gameOverUI;
    public GameObject gameWinUI;
    public GameObject countdownUI; 
    public Text countdownText; 
    public GameMode gameMode;
    public static GameController Instance { get; private set; }

    public List<AIController> aiControllers = new List<AIController>();
    public List<PlayerController> playerControllers = new List<PlayerController>();
    public List<AllyAIController> allyAIController = new List<AllyAIController>();


    private bool gameEnded = false; 
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeControllers();
        gameOverUI.SetActive(false);
        gameWinUI.SetActive(false);
        DisableGameplay();

        StartCoroutine(StartCountdown());
    }

    private void Update()
    {
        CheckGameEnd();
    }

    private void InitializeControllers()
    {
        aiControllers.Clear();
        playerControllers.Clear();
        allyAIController.Clear();
        aiControllers.AddRange(FindObjectsOfType<AIController>());
        playerControllers.AddRange(FindObjectsOfType<PlayerController>());
        allyAIController.AddRange(FindObjectsOfType<AllyAIController>());
    }

    private IEnumerator StartCountdown()
    {
        countdownUI.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownUI.SetActive(false);
        EnableGameplay();
    }

    private void EnableGameplay()
    {
        foreach (var ai in aiControllers)
        {
            ai.enabled = true; 
        }

        foreach (var player in playerControllers)
        {
            player.enabled = true; 
        }

        foreach (var al in allyAIController)
        {
            al.enabled = true; 
        }
    }

    private void DisableGameplay()
    {
        foreach (var ai in aiControllers)
        {
            ai.enabled = false; 
        }

        foreach (var player in playerControllers)
        {
            player.enabled = false; 
        }

        foreach (var al in allyAIController)
        {
            al.enabled = false; 
        }
    }

    private void TriggerGameOver()
    {
        gameOverUI.SetActive(true);
        foreach (var pl in playerControllers)
        {
            pl.animator.SetTrigger(IsKo);
        }

        foreach (var ai in aiControllers.Where(ai => !ai.IsDead))
        {
            ai.animator.SetTrigger(IsVictory);
        }
    }

    private void TriggerGameWin()
    {
        gameWinUI.SetActive(true);
        foreach (var pl in playerControllers)
        {
            pl.animator.SetTrigger(IsVictory);
        }

        foreach (var ai in aiControllers.Where(ai => !ai.IsDead))
        {
            ai.animator.SetTrigger(IsKo);
        }
    }

    private void CheckGameEnd()
    {
        if (gameEnded) return; 

        switch (gameMode)
        {
            case GameMode.OneVOne:
                if (playerControllers.Count > 0 && playerControllers[0].IsDead)
                {
                    gameEnded = true;
                    TriggerGameOver();
                }
                else if (aiControllers.Count > 0 && aiControllers[0].IsDead)
                {
                    gameEnded = true;
                    TriggerGameWin();
                }
                break;

            case GameMode.OneVMul:
                if (playerControllers.Count > 0 && playerControllers[0].IsDead)
                {
                    gameEnded = true;
                    TriggerGameOver();
                }
                else if (aiControllers.TrueForAll(ai => ai.IsDead))
                {
                    gameEnded = true;
                    TriggerGameWin();
                }
                break;

            case GameMode.MulVMul:
                bool allPlayersDead = playerControllers.All(p => p == null || p.IsDead);
                bool allAlliesDead = allyAIController.All(a => a == null || a.IsDead);
                bool allEnemiesDead = aiControllers.All(ai => ai == null || ai.IsDead);

                if (allPlayersDead && allAlliesDead)
                {
                    gameEnded = true;
                    TriggerGameOver();
                }
                else if (allEnemiesDead)
                {
                    gameEnded = true;
                    TriggerGameWin();
                }
                break;
        }
    }
    
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeControllers();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
}