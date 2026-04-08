using UnityEngine;
using System.Collections.Generic;

public class antlion_behavior : MonoBehaviour
{
    List<GameObject> ants_in_detection = new List<GameObject>(), ants_in_collision = new List<GameObject>();
    List<Collider2D> colliders_in_detection = new List<Collider2D>(), colliders_in_collision = new List<Collider2D>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ContactFilter2D ants;
    [SerializeField] float detection_radius = 1f, collision_radius = 1f, checktimer = 0.3f;
    private float countdown;
    public GameObject anttarget = null;
    private bool selectedant = false;
    void Start()
    {
        countdown = checktimer;
    }
    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            countdown = checktimer;
            checkants();
        }
        suck_ants();
        destroyants();
    }
    void suck_ants()
    {
        for(int i = ants_in_detection.Count-1; i>=0; i--)
        {
            if(ants_in_detection[i] == null)
            {
                ants_in_detection.RemoveAt(i);
                continue;
            }
            AntBehavior ant = ants_in_detection[i].GetComponent<AntBehavior>();
            ant.intiate_antlion_death(transform.position);
        }
    }
    void destroyants()
    {
        for(int i = ants_in_collision.Count -1; i >= 0; i--)
        {
            if(ants_in_collision[i] == null)
            {
                ants_in_collision.RemoveAt(i);
                continue;
            }
            Destroy(ants_in_collision[i]);  
        }
    }
    void checkants()
    {
        Vector2 center = transform.position;
        ants_in_collision.Clear();
        ants_in_detection.Clear();
        colliders_in_collision.Clear();
        colliders_in_detection.Clear();
        Physics2D.OverlapCircle(center, collision_radius, ants, colliders_in_collision);
        Physics2D.OverlapCircle(center, detection_radius, ants, colliders_in_detection);
        for (int j = 0; j < colliders_in_detection.Count; j++)
        {
            GameObject log = colliders_in_detection[j].gameObject;
            if(log == null) continue;
            ants_in_detection.Add(log);
        }
        for (int j = 0; j < colliders_in_collision.Count; j++)
        {
            GameObject log = colliders_in_collision[j].gameObject;
            if(log == null) continue;
            ants_in_collision.Add(log);
        }
    }
}
