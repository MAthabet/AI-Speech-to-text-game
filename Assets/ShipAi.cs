using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using LLMUnity;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static shipLogic;
using Color = UnityEngine.Color;

public class ShipAi : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public GameObject ship;

    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
    }
    string ConstructDirectionPrompt(string message)
    {
        string prompt = "Classify the player's command into one of the following actions:\n\n";

        prompt += "**1. Chat:** General conversation, not a game command.\n";
        prompt += "**2. Skill:** The player wants to use a skill (attack or shield to defend).\n";
        prompt += "**3. Enemy:** The player wants to target a specific enemy.(red or blue chiken)\n\n";

        prompt += "Choices:\n";
        prompt += "- Chat: (Reply with: chat)\n";
        prompt += "- Skill: (Reply with: Skill,attack OR Skill,shield)\n";
        prompt += "- Enemy Selection: (Reply with: Enemy,blue OR Enemy,red OR Enemy,none)\n\n";

        prompt += "Player Input: " + message + "\n\n";
        prompt += "Respond with only the category and choice in this format: category,choice";

        return prompt;
    }

    async void onInputFieldSubmit(string message)
    {
        // 1. Disable input field
        playerText.interactable = false;

        // 2. Get response from AI
        var response = await llmCharacter.Chat(ConstructDirectionPrompt(message));
        string[] responses = response.Split(',');

        if (responses.Length < 2)
        {
            Debug.LogError("Invalid AI Response: " + response);
            playerText.interactable = true;
            return;
        }

        string category = responses[0].Trim();
        string choice = responses[1].Trim();

        Debug.Log("Category: " + category + " | Choice: " + choice);

        // 3. Handle AI Response Correctly
        switch (category)
        {
            case "Chat":
                HandleChat(message); // Separate chat handling
                break;

            case "Skill Usage":
                if (choice == "shoot") ChangeStyle("attacking");
                else if (choice == "shield") ChangeStyle("deffending");
                break;

            case "Enemy Selection":
                TargetEnemy(choice); // blue, red, or none
                break;
            default:
                Debug.LogWarning("Unknown command: " + response);
                break;
        }

        // 4. Re-enable input field
        playerText.interactable = true;
        playerText.Select();
    }
    async void HandleChat(string message)
    {
        Debug.Log("Player Chat: " + message);
        // Send the chat input to the LLM character and await response
        string response = await llmCharacter.Chat(message);

        Debug.Log("LLM Chat Response: " + response);
    }
   
    void TargetEnemy(string target)
    {
        switch (target)
        {
            case "Blue":
            case "blue":
                ship.GetComponent<shipLogic>().FollowTarget(GetComponent<shipLogic>().blue);
                break;
            case "Red":
            case "red":
                ship.GetComponent<shipLogic>().FollowTarget(GetComponent<shipLogic>().red);
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

    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    bool onValidateWarning = true;
    void OnValidate()
    {
        if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
        {
            Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            onValidateWarning = false;
        }
    }
}