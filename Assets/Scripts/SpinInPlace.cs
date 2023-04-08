using UnityEngine;

public class SpinInPlace : MonoBehaviour
{
    [Tooltip("Rotation Speed in Degrees")]
    [SerializeField] private Vector3 rotationSpeed;
    
    private void Update() 
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);           
    }
}
