using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;

// This class handles communication with the Gemini API for generating content based on user prompts.
public class MyGeminiAPI : MonoBehaviour
{
    #region Enums & Constants
    // Enum to define the types of responses we can expect from the API.
    public enum ResponseMimeType
    {
        PlainText, // Plain text response
        Json       // JSON formatted response
    }

    // Base URL for the Gemini API
    private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/";
    #endregion

    #region Serialized Fields
    // Serialized fields to allow configuration in the Unity Inspector
    [SerializeField] private string _modelName = "gemini-2.0-flash"; // Name of the model to use
    [SerializeField] private string _apiKey; // API key for authentication
    public string InputPrompt; // Input field for user prompts
    [SerializeField] public string ResponseAction;
    [SerializeField] private Text _responseText; // Text field to display the response
    [SerializeField, TextArea(3, 10)] private string _systemInstructions; // Instructions for the system
    [SerializeField] private ResponseMimeType _responseMimeType = ResponseMimeType.Json; // Expected response type
    [SerializeField] private bool _enableChatHistory = true; // Flag to enable chat history
    [SerializeField] public List<Content> _chatHistory = new List<Content>(); // List to store chat history
    #endregion

    #region Chat History Management
    // Initializes or resets the chat history with system instructions if provided
    private void InitializeChatHistory()
    {
        _chatHistory.Clear(); // Clear existing chat history
    }

    // Creates a message object for the chat history
    private Content CreateMessageObject(string role, string text)
    {
        return new Content
        {
            role = role, // Role of the message sender (user or model)
            parts = new List<Part> { new Part { text = text } } // Message content
        };
    }

    // Public method to clear chat history
    public void ClearChatHistory()
    {
        InitializeChatHistory(); // Call to initialize chat history
    }
    #endregion

    #region Unity Lifecycle Methods
    // Unity method called when the script instance is being loaded
    private void Awake()
    {
        InitializeChatHistory(); // Initialize chat history on awake
    }

    // Unity method called when the object is being destroyed
    private void OnDestroy()
    {
    }
    #endregion

    #region UI Interaction
    // Handles the send button click event
    public async Task SendPrompt()
    {


        _responseText.text = "Generating response..."; // Indicate processing

        // Generate content asynchronously
        string response = await GenerateContentAsync(InputPrompt);
    }
    #endregion

    #region API Communication
    // Structure for the request body sent to the API
    [Serializable]
    private struct GeminiRequestBody
    {
        public List<Content> contents; // List of content messages
        public Content systemInstruction; // System instruction content
        public GenerationConfig generationConfig; // Configuration for generation
    }

    // Structure representing a message in the chat
    [Serializable]
    public struct Content
    {
        public string role; // Role of the message sender
        public List<Part> parts; // Parts of the message
    }

    // Structure representing a part of a message
    [Serializable]
    public struct Part
    {
        public string text; // Text content of the part
    }

    // Structure for generation configuration
    [Serializable]
    public struct GenerationConfig
    {
        public string responseMimeType; // Expected response MIME type
    }

    // Creates the request body for the API call based on the prompt and chat history
    private GeminiRequestBody CreateRequestBody(string prompt)
    {
        var requestBody = new GeminiRequestBody
        {
            contents = new List<Content>(), // Initialize contents list
            generationConfig = new GenerationConfig() // Initialize generation config
        };

        // Add system instructions if provided
        if (!string.IsNullOrEmpty(_systemInstructions))
        {
            requestBody.systemInstruction = new Content
            {
                role = "system", // Role for system instructions
                parts = new List<Part> { new Part { text = _systemInstructions } } // Add instructions
            };
        }

        // Add chat history if enabled
        if (_enableChatHistory)
        {
            requestBody.contents = _chatHistory.Select(msg => new Content
            {
                role = msg.role, // Role from chat history
                parts = new List<Part> { new Part { text = msg.parts[0].text } } // Add message part
            }).ToList();

            // Add the user's current prompt
            requestBody.contents.Add(new Content
            {
                role = "user", // Role for the user
                parts = new List<Part> { new Part { text = prompt } } // Add user prompt
            });
        }
        else
        {
            // If chat history is not enabled, just add the user prompt
            requestBody.contents.Add(new Content
            {
                role = "user", // Role for the user
                parts = new List<Part> { new Part { text = prompt } } // Add user prompt
            });
        }

        // Set the response MIME type based on user selection
        if (_responseMimeType == ResponseMimeType.Json)
        {
            requestBody.generationConfig = new GenerationConfig
            {
                responseMimeType = "application/json" // Set to JSON
            };
        }

        return requestBody; // Return the constructed request body
    }

    // Asynchronously generates content based on the provided prompt
    public async Task<string> GenerateContentAsync(string prompt)
    {
        string url = $"{BASE_URL}{_modelName}:generateContent?key={_apiKey}"; // Construct the API URL

        var requestBody = CreateRequestBody(prompt); // Create the request body
        string jsonData = JsonUtility.ToJson(requestBody); // Convert request body to JSON
        Debug.Log($"Sending request: {jsonData}"); // Log the request

        using UnityWebRequest request = new UnityWebRequest(url, "POST"); // Create a new web request
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData); // Convert JSON to byte array
        request.uploadHandler = new UploadHandlerRaw(bodyRaw); // Set the upload handler
        request.downloadHandler = new DownloadHandlerBuffer(); // Set the download handler
        request.SetRequestHeader("Content-Type", "application/json"); // Set content type header

        try
        {
            await request.SendWebRequest(); // Send the request asynchronously

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Request failed: {request.error}\nResponse: {request.downloadHandler.text}"); // Log error
                return null; // Return null on failure
            }

            // Parse the response from the API
            var responseJObject = JObject.Parse(request.downloadHandler.text);
            string aiResponse = responseJObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString(); // Extract AI response
            Debug.Log("AI Response: " + aiResponse);

            // Log the AI response
            actionResponse = JsonConvert.DeserializeObject<ActionResponse>(aiResponse);
            _responseText.text = actionResponse.response;

            Debug.Log("res: " + actionResponse.response);
            Debug.Log("action: " + actionResponse.action);
            Debug.Log("target: " + actionResponse.target);
            // If chat history is enabled and response is valid, add to chat history
            if (_enableChatHistory && !string.IsNullOrEmpty(aiResponse))
            {
                _chatHistory.Add(CreateMessageObject("model", aiResponse)); // Add AI response to history
            }

            return aiResponse; // Return the AI response
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during API request: {e.Message}"); // Log any exceptions
            return null; // Return null on exception
        }
    }
    #endregion
    public ActionResponse actionResponse;
    public class ActionResponse
    {
        public string response;
        public string action;
        public string target;
    }
}