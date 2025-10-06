using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public TextMeshProUGUI woodtext;
    public TextMeshProUGUI stonetext;

    public static int wood;
    public static int stone;

    Farmables script;

    private void Update()
    {
        UpdateText();   
    }
    void Start()
    {
        wood = 0;
        stone = 0;
        UpdateText();
    }

    void UpdateText()
    {
        woodtext.text = "Wood: " + wood;
        stonetext.text = "Stone: " + stone;
    }
}
