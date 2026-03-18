using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnStuff : MonoBehaviour
{
    public GameObject spawnAnt, spawnFood;
    public static GameObject latestFood;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("clicked");
            //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector3 offset = new Vector3(0,0,10);
            Vector3 mouse = Mouse.current.position.ReadValue();
            mouse.z = -Camera.main.transform.position.z;
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse);
            Instantiate(spawnAnt, pos, Quaternion.identity); 
        }
        if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("clicked");
            //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector3 offset = new Vector3(0,0,10);
            Vector3 mouse = Mouse.current.position.ReadValue();
            mouse.z = -Camera.main.transform.position.z;
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse);
            latestFood = Instantiate(spawnFood, pos, Quaternion.identity);

        }
    }
}
