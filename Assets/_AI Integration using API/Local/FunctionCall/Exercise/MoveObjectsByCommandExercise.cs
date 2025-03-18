using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using LLMUnity;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class MoveObjectsByCommandExercise : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public RectTransform blueSquare;
    public RectTransform redSquare;

    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
    }

    string[] GetFunctionNames<T>()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            functionNames.Add(function.Name);
        return functionNames.ToArray();
    }
    string ConstructDirectionPrompt(string message)
    {
        string prompt = "Pick the color and the direction from the upcoming Choices that fit the input?\n\n";
        prompt += "Input: " + message + "\n\n";
        prompt += "color Choices: \n" +
            "- Blue \n" +
            "- Red \n\n" +
            "Direction Choices: \n" +
            "- Up \n" +
            "- Down \n" +
            "- Left \n" +
            "- Right";

        prompt += "\nAnswer with the choice as Color,Dir";
        return prompt;
    }

    async void onInputFieldSubmit(string message)
    {
        // 1. Disable the input field
        playerText.interactable = false;

        // 2. Get direction and color from AI using llmCharacter.Chat
        var response = await llmCharacter.Chat(ConstructDirectionPrompt(message));
        string[] responses = response.Split(',');
        Debug.Log("Response: " + response);
        // 3. Convert AI responses to actual Vector3 and Color using reflection
        RectTransform square = GetObjectByColor(responses[0]);
        Vector2 direction = GetDir(responses[1]);

        // 4. Move the correct square in the specified direction
        if (square != null)
        {
            square.anchoredPosition += direction * 100;
        }

        // 5. Re-enable the input field
        playerText.interactable = true;
        playerText.Select();
    }
    Vector2 GetDir(string dir)
    {
        switch (dir)
        {
            case "Up":
                return new Vector2(0, 1);
            case "Down":
                return new Vector2(0, -1);
            case "Left":
                return new Vector2(-1, 0);
            case "Right":
                return new Vector2(1, 0);
                default:
                return Vector2.zero;
        }
    }
    private RectTransform GetObjectByColor(string color)
    {
        if (color == "Blue")
            return blueSquare;
        else if (color == "Red")
            return redSquare;

        return null;
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