using UnityEngine;
using UnityEngine.UI;

public class UIElementsCountDisplayer : MonoBehaviour
{
    [SerializeField] private Generator generator;
    [SerializeField] private Text mainElementsText;
    private string mainElementsBaseText;
    private int mainElementsLatestCount;
    [SerializeField] private Text secondaryElementsText;
    private string secondaryElementsBaseText;
    private int secondaryElementsLatestCount;

    private void Awake() 
    {
        mainElementsBaseText = mainElementsText.text;
        secondaryElementsBaseText = secondaryElementsText.text;   
        mainElementsLatestCount = -1;
        secondaryElementsLatestCount = -1; 
    }

    private void Update() 
    {
        UpdateCount(mainElementsText, mainElementsBaseText, mainElementsLatestCount, generator.MainSpawnersQueueLength);
        UpdateCount(secondaryElementsText, secondaryElementsBaseText, secondaryElementsLatestCount, generator.OptionalSpawnersQueueLength);
    }

    private void UpdateCount(Text textComponent, string baseText, int latestCount, int newCount)
    {
        if (newCount != latestCount)
        {
            textComponent.text = baseText + newCount;
            latestCount = newCount;
        }
    }
}
