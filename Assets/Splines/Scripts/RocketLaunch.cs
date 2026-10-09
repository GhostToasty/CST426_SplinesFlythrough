using System.Data;
using UnityEngine;

public class RocketLaunch : MonoBehaviour
{
    public float speed = 8;
    bool launched;
    float timer;

    public void Awake()
    {
        launched = false;
    }

    public void Update()
    {
        if (launched == false)
            return;
        
        if (transform.position. y < 1000)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (timer * speed * Time.deltaTime), transform.position.z);
            timer += Time.deltaTime;
        }
        
    }
    
    public void Launch()
    {
        if (launched == false)
        {
            launched = true;
            timer = 0f; 
        }
        
    }
}
