using UnityEngine;
using UnityEngine.UI;

public class MappingUIController : MonoBehaviour
{
    public InputField roomInput;
    public Dropdown floorDropdown;
    public Button captureButton;

    public string CurrentRoom => roomInput.text;
    public int CurrentFloor => floorDropdown.value;

    void Awake()
    {
        captureButton.interactable = true;
    }
}
