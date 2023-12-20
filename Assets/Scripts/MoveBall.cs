using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveBall : MonoBehaviour
{
    [NonSerialized]
    public bool enableBall;

    float x = 0;
    float y = 0;
    float z = 0;

    private Camera mainCamera;
    private float CameraZDistance;

    SphereCollider sphereCollider;
    MeshRenderer meshRenderer;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float ySpeed = 5f;

    private void Awake()
    {
        mainCamera = Camera.main;
        CameraZDistance = mainCamera.WorldToScreenPoint(transform.position).z; //z axis of the game object for screen view

        sphereCollider = GetComponent<SphereCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

    }
    private void Start()
    {
        sphereCollider.enabled = enableBall;
        meshRenderer.enabled = enableBall;
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
        if (Input.GetMouseButton(0))
        {
            if (!enableBall) return;
            Vector3 ScreenPosition =
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraZDistance); //z axis added to screen point 
            Vector3 NewWorldPosition =
                mainCamera.ScreenToWorldPoint(ScreenPosition); //Screen point converted to world point

            transform.position = NewWorldPosition;
        }
        transform.position += new Vector3(x * moveSpeed, y * ySpeed, z * moveSpeed) * Time.deltaTime;

    }
    //https://gist.github.com/seferciogluecce/132e136ed71834143100f14b9b86b9fa
    public void OnClick()
    {
        enableBall = !enableBall;
        Debug.Log(enableBall);

        TMP_Text moveBallBtn = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>();
        if (enableBall)
        {
            moveBallBtn.text = "Disable Ball";
            sphereCollider.enabled = true;
            meshRenderer.enabled = true;
        }
        else
        {
            moveBallBtn.text = "Enable Ball";
            sphereCollider.enabled = false;
            meshRenderer.enabled = false;
        }
    }

}
