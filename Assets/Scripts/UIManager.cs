using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Managers")]
    public LevelManager levelManager;
    private SnakeController _snakeController;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject inGameHUD;

    [Header("Main Menu Components")]
    public CanvasGroup mainMenuCanvasGroup;
    public RectTransform titleImageTransform;
    public RectTransform playButtonTransform;
    public RectTransform decorativeMapTransform;

    [Header("How To Play Panel Components")]
    public CanvasGroup howToPlayPanelCanvasGroup;
    public RectTransform howToPlayPanelRectTransform;
    
    private Vector2 _titleInitialPos;
    private Vector2 _playButtonInitialPos;
    private Vector2 _decorativeMapInitialPos;
    private Vector2 _howToPlayInitialPos;

    void Awake()
    {
        _titleInitialPos = titleImageTransform.anchoredPosition;
        _playButtonInitialPos = playButtonTransform.anchoredPosition;
        _decorativeMapInitialPos = decorativeMapTransform.anchoredPosition;
        _howToPlayInitialPos = howToPlayPanelRectTransform.anchoredPosition;
    }

    void Start()
    {
        AudioManager.Instance.PlayMusic("MainMenuMusic");
        ShowMainMenu(false);
    }
    
    public void SetSnakeController(SnakeController controller)
    {
        _snakeController = controller;
    }

    public void OnPlayButton()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        mainMenuCanvasGroup.interactable = false;

        Vector2 titleEndPos = new Vector2(_titleInitialPos.x, _titleInitialPos.y + 600f);
        Vector2 playButtonEndPos = new Vector2(_playButtonInitialPos.x, _playButtonInitialPos.y - 400f);
        Vector2 mapEndPos = new Vector2(_decorativeMapInitialPos.x, _decorativeMapInitialPos.y - 800f);

        titleImageTransform.DOAnchorPos(titleEndPos, 0.5f).SetEase(Ease.InBack);
        decorativeMapTransform.DOAnchorPos(mapEndPos, 0.5f).SetEase(Ease.InBack);
        playButtonTransform.DOAnchorPos(playButtonEndPos, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            ShowGameHUD();
        });
    }

    public void OnHomeButton()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        ShowMainMenu(true);
    }

    public void OnRestartButton()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        levelManager.GenerateLevel();
    }

    public void OnUndoButton()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        if (_snakeController == null) return;
        _snakeController.Undo();
    }
    
    public void OnUpButton()
    {
        if (_snakeController == null) return;
        _snakeController.AttemptMove(Direction.Up);
    }

    public void OnDownButton()
    {
        if (_snakeController == null) return;
        _snakeController.AttemptMove(Direction.Down);
    }

    public void OnLeftButton()
    {
        if (_snakeController == null) return;
        _snakeController.AttemptMove(Direction.Left);
    }

    public void OnRightButton()
    {
        if (_snakeController == null) return;
        _snakeController.AttemptMove(Direction.Right);
    }

    public void ToggleHowToPlayPanel(bool show)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        if (show)
        {
            howToPlayPanel.SetActive(true);
            howToPlayPanelCanvasGroup.alpha = 1;
            howToPlayPanelCanvasGroup.blocksRaycasts = true;
            howToPlayPanelCanvasGroup.interactable = true;
            
            Vector2 offScreenPos = new Vector2(_howToPlayInitialPos.x, _howToPlayInitialPos.y + 1200f);
            howToPlayPanelRectTransform.anchoredPosition = offScreenPos;
            howToPlayPanelRectTransform.DOAnchorPos(_howToPlayInitialPos, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            howToPlayPanelCanvasGroup.interactable = false;
            Vector2 offScreenPos = new Vector2(_howToPlayInitialPos.x, _howToPlayInitialPos.y + 1200f);
            howToPlayPanelRectTransform.DOAnchorPos(offScreenPos, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
                howToPlayPanel.SetActive(false);
            });
        }
    }

    public void ShowMainMenu(bool withAnimation)
    {
        mainMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        inGameHUD.SetActive(false);

        mainMenuCanvasGroup.alpha = 1;
        mainMenuCanvasGroup.interactable = true;

        if (withAnimation)
        {
            titleImageTransform.DOAnchorPos(_titleInitialPos, 0.5f).SetEase(Ease.OutBack);
            playButtonTransform.DOAnchorPos(_playButtonInitialPos, 0.5f).SetEase(Ease.OutBack);
            decorativeMapTransform.DOAnchorPos(_decorativeMapInitialPos, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            titleImageTransform.anchoredPosition = _titleInitialPos;
            playButtonTransform.anchoredPosition = _playButtonInitialPos;
            decorativeMapTransform.anchoredPosition = _decorativeMapInitialPos;
        }
    }
    
    public void ShowGameHUD()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        inGameHUD.SetActive(true);
        
        levelManager.GenerateLevel();
    }
}