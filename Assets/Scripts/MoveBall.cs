using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveBall : MonoBehaviour
{
    public bool enableBall;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float ySpeed = 5f;

    float x = 0;
    [SerializeField] float y = 0;
    float z = 0;

    private Camera mainCamera;
    private float CameraZDistance;

    private float counter = 0;

    Vector3 oldPos;

    void Start()
    {
        mainCamera = Camera.main;
        CameraZDistance =
            mainCamera.WorldToScreenPoint(transform.position).z; //z axis of the game object for screen view
    }
    private void Update()
    {
        if (!enableBall) return;

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Q) && y < 1)
        {
            y += 0.1f;
        }
        else if (Input.GetKey(KeyCode.E) && y > -1)
        {
            y -= 0.1f;
        }
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
        {
            y = 0;
        }

        transform.position += new Vector3(x, y * ySpeed, z) * moveSpeed * Time.deltaTime;

    }
    private void LateUpdate()
    {
        oldPos = transform.position;
    }
    //https://gist.github.com/seferciogluecce/132e136ed71834143100f14b9b86b9fa
    void OnMouseDrag()
    {
        if (!enableBall) return;
        Vector3 ScreenPosition =
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraZDistance); //z axis added to screen point 
        Vector3 NewWorldPosition =
            mainCamera.ScreenToWorldPoint(ScreenPosition); //Screen point converted to world point

        transform.position = NewWorldPosition;
    }
    public void OnClick()
    {
        enableBall = !enableBall;

        TMP_Text moveBallBtn = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>();
        if (enableBall)
        {
            moveBallBtn.text = "Disable Ball";
        }
        else
        {
            moveBallBtn.text = "Enable Ball";
        }
    }
    public Vector3 moveDir()
    {
        return transform.position - oldPos;
    }
}
