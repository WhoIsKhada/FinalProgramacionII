using UnityEngine;

public class LookAt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main; 
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_camera.transform);
    }
}
