using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

public class OpenAIChat : MonoBehaviour
{
    private string apiKey = ""; //deleted my api key as it reached the limit;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    private void Start()
    {
        if(apiKey == "")
        {
            Debug.LogError("Please enter your OpenAI API key in the ChatGptRespond script.");
        }
    }
    public void SendToChatGPT(string userInput)
    {
        StartCoroutine(SendRequest(userInput));
    }

    IEnumerator SendRequest(string inputText)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";

        var requestData = new
        {
            model = "gpt-3.5-turbo",
            messages = new object[]
            {
            new { role = "system", content = "You are an AI assistant." },
            new { role = "user", content = inputText }
            },
            max_tokens = 150
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log("Sending Request: " + jsonData);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else if (request.responseCode == 429)
            {
                Debug.LogError("Too Many Requests! Waiting before retrying...");
                yield return new WaitForSeconds(5); // Wait 5 seconds
                StartCoroutine(SendRequest(inputText)); // Retry
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode + " - " + request.error);
            }
        }
    }

    void ProcessResponse(string jsonResponse)
    {
        var response = JsonConvert.DeserializeObject<ChatGPTResponse>(jsonResponse);
        string aiReply = response.choices[0].message.content;
        Debug.Log("ChatGPT Reply: " + aiReply);
    }

    // Class for JSON parsing
    [System.Serializable]
    public class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string content;
    }

}
