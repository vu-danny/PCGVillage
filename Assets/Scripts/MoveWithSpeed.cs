using UnityEngine;

public class MoveWithSpeed : MonoBehaviour
{
    [SerializeField] private Vector3 _speed;
    private bool started;
    
    private void Start() 
    {
        started = false;    
    }

    void Update()
    {
        started |= Input.GetKeyDown(KeyCode.Space);
        if (started)
        {
            transform.Translate(_speed * Time.deltaTime, Space.World);
        }
    }
}
