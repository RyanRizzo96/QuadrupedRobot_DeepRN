using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour {

    private Button b;

    private void Start()
    {
        b = GetComponent<Button>();
        b.onClick.AddListener(() => Application.Quit());
    }
}
