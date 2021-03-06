using UnityEngine.UI;
using UnityEngine;

public class Button_StartGame : MonoBehaviour
{
    Button m_Button;
    public GameObject LevelSelectPanel;
    private void Awake()
    {
        m_Button = GetComponent<Button>();
    }

    private void Start()
    {
        m_Button.onClick.AddListener(StartGame);
        m_Button.onClick.AddListener(OnClick);
    }
    private void StartGame()
    {
        LevelSelectPanel.SetActive(true);
    }
    private void OnClick()
    {
        gameObject.SetActive(false);
    }
}
