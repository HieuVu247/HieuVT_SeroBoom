using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameStatus
    {
        Playing,
        LevelWon,
        LevelLost
    }

    public GameStatus CurrentStatus { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartLevel()
    {
        CurrentStatus = GameStatus.Playing;
    }

    public void WinLevel()
    {
        if (CurrentStatus != GameStatus.Playing) return;
        
        CurrentStatus = GameStatus.LevelWon;
        Debug.Log("GAME MANAGER: Level Won!");
    }

    public void LoseLevel()
    {
        if (CurrentStatus != GameStatus.Playing) return;

        CurrentStatus = GameStatus.LevelLost;
        Debug.Log("GAME MANAGER: Level Lost! (GAME OVER)");
    }
}