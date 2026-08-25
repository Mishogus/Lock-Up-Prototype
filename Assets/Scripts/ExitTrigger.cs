using UnityEngine;

// Sits on the exit point marker. When the player reaches it, freezes
// movement/look, releases the mouse, and shows a simple win message.
public class ExitTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        triggered = true;
        pc.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (!triggered) return;

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = Color.green;
        GUI.Label(new Rect(0, Screen.height / 2f - 60, Screen.width, 120), "YOU ESCAPED!", title);

        GUIStyle sub = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };
        sub.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height / 2f + 40, Screen.width, 40), "Stop Play to try again", sub);
    }
}
