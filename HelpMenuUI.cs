using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HelpMenuUI : MonoBehaviour
{
    [Header("Adatok")]
    // Ide húzogatod be a ScriptableObject fájlokat az Editorban
    public List<HelpTopic> helpTopics; 

    [Header("Bal Oldal (Lista)")]
    public Transform listContent;
    public GameObject listButtonPrefab; // A QuestItem_Prefab tökéletes ide is!

    [Header("Jobb Oldal (Részletek)")]
    public TextMeshProUGUI titleText;
    public Image contentImage;
    public TextMeshProUGUI descriptionText;
    public ScrollRect contentScrollRect; // Hogy fel tudjuk görgetni a tetejére váltáskor

    private Button selectedButton;

    private void Start()
    {
        GenerateTopicList();
        
        // Alapból nyissuk meg az elsőt, ha van
        if (helpTopics.Count > 0)
        {
            ShowTopic(helpTopics[0]);
        }
    }

    private void GenerateTopicList()
    {
        // Töröljük a régi gombokat (biztonság kedvéért)
        foreach (Transform child in listContent) Destroy(child.gameObject);

        foreach (var topic in helpTopics)
        {
            GameObject btnObj = Instantiate(listButtonPrefab, listContent);
            
            // Szöveg beállítása
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = topic.topicName;

            // Gomb esemény
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => 
            {
                ShowTopic(topic);
                SetSelectedButton(btn);
            });
            
            // Ha ez az első, jelöljük ki alapból (opcionális, de szép)
            if (topic == helpTopics[0]) SetSelectedButton(btn);
        }
    }

    private void ShowTopic(HelpTopic topic)
    {
        titleText.text = topic.topicName;
        descriptionText.text = topic.description;

        // Kép kezelése: Ha nincs kép, kapcsoljuk ki az Image komponenst
        if (topic.illustration != null)
        {
            contentImage.gameObject.SetActive(true);
            contentImage.sprite = topic.illustration;
            contentImage.preserveAspect = true; // Hogy ne torzuljon a kép
        }
        else
        {
            contentImage.gameObject.SetActive(false);
        }

        // Visszaugrás a szöveg tetejére (ha hosszú volt az előző)
        if (contentScrollRect != null) contentScrollRect.verticalNormalizedPosition = 1f;
    }

    private void SetSelectedButton(Button newButton)
    {
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Normal;
        }

        selectedButton = newButton;
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Bold;
        }
    }
}