using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public GameObject scoreboard;
    public GameObject exitMenu;
    public GameObject errorMessage;
    public InputField usernameField;
    public ToggleGroup RadioOptions;

    public string team;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitMenu();
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer()
    {
        if (usernameField.text == "")
        {
            errorMessage.SetActive(true);
            return;
        }

        startMenu.SetActive(false);
        usernameField.interactable = false;

        scoreboard.SetActive(true);

        var toggles = RadioOptions.GetComponentsInChildren<Toggle>();
        team = GetSelectedTeam(toggles);

        Client.instance.ConnectToServer();
    }

    private string GetSelectedTeam(Toggle[] toggles)
    {
        var team = "";
        foreach (var t in toggles)
            if (t.name == "ToggleBlue" && t.isOn)
                team = "blue";
            else if (t.name == "ToggleRed" && t.isOn)
                team = "red";
        return team;
    }

    public void ExitTheGame()
    {
        Application.Quit();
    }

    private void ExitMenu()
    {
        if (!exitMenu.activeSelf)
            exitMenu.SetActive(true);
        else
            exitMenu.SetActive(false);
    }
}
