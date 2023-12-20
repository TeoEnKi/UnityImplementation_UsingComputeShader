using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boundary : MonoBehaviour
{
    public Vector3 boxSize;
    public Vector3 boxSpawn;

    //creating the box
    private void OnDrawGizmos()
    {
        boxSpawn = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxSpawn, boxSize);
    }
    public void ChangeBoundarySize()
    {
        TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
        if (inputField == null || !float.TryParse(inputField.text, out float sizeDimValue)) return;

        switch (inputField.gameObject.name)
        {
            case ("VecX"):
                boxSize.x = Mathf.Abs(float.Parse(inputField.text));
                break;

            case ("VecY"):
                boxSize.y = Mathf.Abs(float.Parse(inputField.text));
                break;

            case ("VecZ"):
                boxSize.z = Mathf.Abs(float.Parse(inputField.text));
                break;
        }
    }

}
