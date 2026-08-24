using UnityEngine;

// Sits on a trigger zone near a doorway. Slides the referenced door panel
// up out of the way while a player is nearby, and closes it behind them.
public class AutoDoor : MonoBehaviour
{
    public Transform doorPanel;
    public Vector3 openOffset = new Vector3(0f, 2.4f, 0f);
    public float speed = 4f;

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private int playersInRange = 0;

    void Start()
    {
        if (doorPanel == null) return;
        closedLocalPos = doorPanel.localPosition;
        openLocalPos = closedLocalPos + openOffset;
    }

    void Update()
    {
        if (doorPanel == null) return;
        Vector3 target = playersInRange > 0 ? openLocalPos : closedLocalPos;
        doorPanel.localPosition = Vector3.MoveTowards(doorPanel.localPosition, target, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null) playersInRange++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null) playersInRange = Mathf.Max(0, playersInRange - 1);
    }
}
