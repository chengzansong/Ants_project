using UnityEngine;

public class AntholeBehavior : MonoBehaviour
{
    public GameObject spawn;
    [SerializeField] Transform parent;
    public float time_to_spawn = 1f, spawn_countdown = 1f;
    public int maxspawn = 50; 
    private int curfood = 0;
    [SerializeField] int ant_req = 10;
    int count = 0;
    void Start()
    {
    }
    void Update()
    {
        spawn_countdown -= Time.deltaTime;
        if(spawn_countdown <= 0 && count<maxspawn)
        {
            spawn_countdown = time_to_spawn;
            int random_rotation = Random.Range(0,360);
            Instantiate(spawn, transform.position, Quaternion.Euler(0,0,random_rotation), parent);
            count++;
        }
        if(curfood >= ant_req)
        {
            curfood = 0;
            int random_rotation = Random.Range(0,360);
            Instantiate(spawn, transform.position, Quaternion.Euler(0,0, random_rotation), parent);
        }
    }
    public void accumulate_food()
    {
        curfood++;
    }
}
