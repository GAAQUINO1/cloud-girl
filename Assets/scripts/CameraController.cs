using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform startPosition, defaultPosition, leftMonitorPos, rightMonitorPos;
    public float moveDuration = 2f;
    private bool isInteracting = false;
    private bool allowExit = true;
    private bool isViewingMonitor = false; // Track if we are already looking at the monitor

    public GameObject leftMonitorUI;
    public GameObject rightMonitorUI;

    void Start()
    {
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;

        LeanTween.move(gameObject, defaultPosition.position, moveDuration).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(gameObject, defaultPosition.rotation.eulerAngles, moveDuration).setEase(LeanTweenType.easeInOutQuad);

        leftMonitorUI.SetActive(false);
        rightMonitorUI.SetActive(false);
    }

    void Update()
    {
        if (!allowExit) return;

        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("LeftMonitor"))
                    HandleLeftMonitorClick();

                if (hit.collider.CompareTag("RightMonitor"))
                    FocusOnMonitor("right");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // Press ESC to exit the monitor
        {
            ReturnToDefault();
        }
    }

    void HandleLeftMonitorClick()
    {
        if (!isViewingMonitor) // 🔥 First click: Move camera to the monitor
        {
            FocusOnMonitor("left");
            isViewingMonitor = true;
        }
        else // 🔥 Second click: Start or resume the chat
        {
            ChatController chatController = FindFirstObjectByType<ChatController>();
            if (chatController != null)
            {
                chatController.ToggleChat();
            }
        }
    }

    void FocusOnMonitor(string monitor)
    {
        if (isInteracting) return;
        isInteracting = true;

        Transform target = null;

        if (monitor == "left")
        {
            target = leftMonitorPos;
            leftMonitorUI.SetActive(true);
            rightMonitorUI.SetActive(false);
        }
        else if (monitor == "right")
        {
            target = rightMonitorPos;
            rightMonitorUI.SetActive(true);
            leftMonitorUI.SetActive(false);
        }

        if (target != null)
        {
            LeanTween.move(gameObject, target.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.rotate(gameObject, target.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => isInteracting = false);
        }
    }

    void ReturnToDefault()
    {
        if (!allowExit) return;

        LeanTween.move(gameObject, defaultPosition.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(gameObject, defaultPosition.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad);

        leftMonitorUI.SetActive(false);
        rightMonitorUI.SetActive(false);
        isViewingMonitor = false;

        ChatController chatController = FindFirstObjectByType<ChatController>();
        if (chatController != null)
        {
            chatController.PauseChat();
        }
    }

    public void AllowExit()
    {
        allowExit = true;
    }

    public void PreventExit()
    {
        allowExit = false;
    }
}
