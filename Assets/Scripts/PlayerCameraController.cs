using UnityEngine;

namespace Main
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float mouseSensitivity = 7.5f;
        
        private float cameraVerticalRotation = 0;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            cameraVerticalRotation = cameraTransform.rotation.eulerAngles.x;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

            cameraVerticalRotation += mouseY * mouseSensitivity;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90, 90);

            cameraTransform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
            playerTransform.Rotate(Vector3.up * mouseX * mouseSensitivity);
        }
    }
}
