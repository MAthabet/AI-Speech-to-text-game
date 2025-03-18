using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;

public class JSON_ToText : MonoBehaviour
{
    public TextMeshProUGUI displayText; 
    public UnityEngine.Object jsonFile; 

    public void DisplayJsonData() 
    { 
        displayText.text = jsonFile.ToString(); 
    } 
    public void DisplayJsonValue() 
    { 
        JObject json = JObject.Parse(jsonFile.ToString());  
        string firstName = json["user"]["firstName"].ToString(); 
        displayText.text = firstName; 
    } 
}
