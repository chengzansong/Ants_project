using Unity.VisualScripting.FullSerializer;
using UnityEngine;
public class SpawnFood : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject spawn;
    [SerializeField] Transform parent;
    public float spacing = 1f;
    public float x_left, x_right, y_left, y_right;
    void Start()
    {
        for(float i = x_left; i <= x_right; i+= spacing)
        {
            for(float j = y_left; j<= y_right; j+= spacing)
            {
                Vector3 tempvec = new Vector3(i, j, 0);
                Instantiate(spawn, tempvec, Quaternion.identity, parent);
            }
        }
    }
    void Update()
    {
        
    }
}