
using UnityEngine;
using UnityEngine.UI;
using static shipLogic;

public class ShipManager : MonoBehaviour
{
    public InputField playerText;
    public GameObject ship;
    public GameObject geminiManager;
    private MyGeminiAPI geminiAPI;

    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
        geminiAPI = geminiManager.GetComponent<MyGeminiAPI>();
    }

    async void onInputFieldSubmit(string message)
    {
        // 1. Disable input field
        playerText.interactable = false;

        // 2. Get response from AI
        if (geminiAPI == null)
        {
            Debug.LogError("Gemini API is not set!");
            return;
        }
        geminiAPI.InputPrompt = message;
        await geminiAPI.SendPrompt();

        string category = geminiAPI.actionResponse.action;
        string choice = geminiAPI.actionResponse.target;

        // 3. Handle AI Response Correctly
        switch (category)
        {
            case "Chat":
                HandleChat(geminiAPI.actionResponse.response); // Separate chat handling
                break;
            case "Defend":
            case "defend":
                ship.GetComponent<shipLogic>().setState(ShipState.Defense);
                break;
            case "Attack":
            case "attack":
                ship.GetComponent<shipLogic>().setState(ShipState.Attack);
                TargetEnemy(choice);
                break;
            default:
                Debug.LogWarning("Unknown command");
                break;
        }

        // 4. Re-enable input field
        playerText.interactable = true;
        playerText.Select();
    }
    void HandleChat(string message)
    {
        //Debug.Log("Chat message: " + message);
    }
    void TargetEnemy(string target)
    {
        switch (target)
        {
            case "Blue":
            case "blue":
            case "Red":
            case "red":
                ship.GetComponent<shipLogic>().FollowTarget(target);
                break;
            case "none":
            case "None":
                break;
            default:
                Debug.Log("Invalid enemy choice");
                break;
        }
    }
    void ChangeStyle(string style)
    {
        switch (style)
        {
            case "attacking":
                ship.GetComponent<shipLogic>().setState(ShipState.Attack);
                break;
            case "deffending":
                ship.GetComponent<shipLogic>().setState(ShipState.Defense);
                break;
            case "none":
                break;
            default:
                Debug.Log("Invalid style choice");
                break;
        }
    }


    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }
}