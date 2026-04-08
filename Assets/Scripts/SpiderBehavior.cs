using UnityEngine;
using System.Collections.Generic;

public class SpiderBehavior : MonoBehaviour
{
    List<GameObject> ants_in_detection = new List<GameObject>(), ants_in_collision = new List<GameObject>();
    List<Collider2D> colliders_in_detection = new List<Collider2D>(), colliders_in_collision = new List<Collider2D>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ContactFilter2D ants;
    [SerializeField] float maxspeed = 3f, maxforce = 20f, wanderstrength = 0.07f, detection_radius = 1f, collision_radius = 1f, checktimer = 0.3f, countdown, x, y;
    Vector2 position, velocity, desired_direction, desired_velocity, desired_force, acceleration;
    public GameObject anttarget = null;
    private bool selectedant = false;
    void Start()
    {
        transform.position = new Vector2(x, y);
        position = new Vector2(x, y);
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
        if(!selectedant && ants_in_detection.Count > 0)
        {
            selectedant = true;
            anttarget = ants_in_detection[0];
        }
        destroyants();
        directiondecision();
        movementPhysics();
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
    void directiondecision()
    {
        if(anttarget == null) selectedant = false;//safeguard in case another ant destroys food;
        if(selectedant) //heading directly to the ant
        {
            desired_direction = ((Vector2)anttarget.transform.position - (Vector2)transform.position).normalized;
        }
        else //randomly wandering around finding ants
        {
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            desired_direction += newdirection * wanderstrength;
            desired_direction = desired_direction.normalized;
        }
    }
    void movementPhysics()
    {
        desired_velocity = desired_direction * maxspeed;
        desired_force = desired_velocity - velocity;
        acceleration = Vector2.ClampMagnitude(desired_force, maxforce);
        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxspeed);
        position += velocity * Time.deltaTime;
        if(velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, velocity);
            transform.SetPositionAndRotation(position, toRotation);
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            Wall tempwall = collision.gameObject.GetComponent<Wall>();
            velocity = Vector2.Reflect(velocity, tempwall.normal);
            desired_direction = Vector2.Reflect(desired_direction, tempwall.normal);
        }
    }
}
