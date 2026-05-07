using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine.Analytics;
using UnityEngine.Rendering;

public class SpiderBehavior : MonoBehaviour
{
    List<GameObject> ants_in_detection = new List<GameObject>(), ants_in_collision = new List<GameObject>();
    List<Collider2D> colliders_in_detection = new List<Collider2D>(), colliders_in_collision = new List<Collider2D>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ContactFilter2D ants;
    [SerializeField] float maxforce = 20f, wanderstrength = 0.07f, collision_radius = 1f, checktimer = 0.3f, x, y, debugLength = 1f, ants_stack_timer = 30f, ants_stack_countdown;
    Vector2 position, velocity, desired_direction, desired_velocity, desired_force, acceleration;
    public GameObject anttarget = null;
    private bool selectedant = false;
    public bool full = false;

    [SerializeField] float mass = 1f, speed = 1f, sense = 1f, energy_loss_time = 1f, cur_energy, maxenergymult = 1f, ant_nutrition_value = 10f, HP, base_HP = 10f;
    private float countdown, energy_countdown, maxenergy, eloss_per_step, detection_radius, maxspeed, ants_stack;
    public SpiderGenome genome;
    [SerializeField] int ants_eaten = 0, ants_needed = 5;
    [SerializeField] private Color fullcolor = Color.yellow;
    private SpriteRenderer[] srs;

    void Start()
    {
        ants_stack = 0;
        ants_stack_countdown = ants_stack_timer;
        base_HP *= mass;
        HP = base_HP;
        transform.position = new Vector2(x, y);
        position = new Vector2(x, y);
        countdown = checktimer;
        energy_countdown = energy_loss_time;
        srs = GetComponentsInChildren<SpriteRenderer>();
    }
    
    public void apply_genome(SpiderGenome new_genome)
    {
        genome = new_genome;

        speed = genome.speed;
        sense = genome.sense;
        mass = genome.mass;

        maxenergy = maxenergymult;
        cur_energy = maxenergy;
        eloss_per_step = mass*Mathf.Pow(speed, 2f) + Mathf.Pow(sense, 2f);

        detection_radius = sense;
        maxspeed = speed;

        ants_eaten = 0;
    }
    void Update()
    {
        if(full && ants_stack_countdown <=0)
        {
            return;
        }
        energy_countdown -= Time.deltaTime;
        if(ants_eaten >= ants_needed)
        {
            become_full();
        }
        if(HP< 0)
        {
            Destroy(gameObject);
        }
        if(energy_countdown <= 0 && ants_eaten < ants_needed)
        {
            energy_countdown = energy_loss_time;
            cur_energy -= eloss_per_step;
            if(cur_energy <= 0)
            {
                Destroy(gameObject);
            }
        }
        ants_stack_countdown -= Time.deltaTime;
        if(ants_stack_countdown <= 0)
        {
            if(full) return;
            ants_stack_countdown = ants_stack_timer;
            ants_stack = 0;
        }
        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            countdown = checktimer;
            checkants();
            if(ants_in_detection.Count > 0)
            {
                anttarget = decide_ant_target();
            }
        }
        eatants();
        directiondecision();
        movementPhysics();
    }
    void become_full()
    {
        full = true;
        foreach (SpriteRenderer renderer in srs)
        {
            renderer.color = fullcolor;
        }
    }
    public void setlocation(Vector2 location)
    {
        x = location.x;
        y = location.y;
    }
    GameObject decide_ant_target()
    {
        GameObject bestTarget = null;
        float mindist = Mathf.Infinity;
        selectedant = true;
        for(int i = ants_in_detection.Count-1; i >= 0; i--)
        {
            if(ants_in_detection[i] == null)
            {
                ants_in_detection.RemoveAt(i);
                continue;
            }
            Vector2 distance = (Vector2)ants_in_detection[i].gameObject.transform.position - (Vector2)this.gameObject.transform.position;
            float dist = distance.sqrMagnitude;
            if(dist < mindist)
            {
                mindist = dist;
                bestTarget = ants_in_detection[i];
            }
        }
        return bestTarget;
    }
    void OnDrawGizmos()
    {
        //vector
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(desired_direction.normalized * debugLength);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.1f);

        //checkfood
        Vector2 center = transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, detection_radius);

        Vector2 center2 = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center2, collision_radius);
    }

    void eatants()
    {
        for(int i = ants_in_collision.Count -1; i >= 0; i--)
        {
            if(ants_in_collision[i] == null)
            {
                ants_in_collision.RemoveAt(i);
                continue;
            }
            ants_eaten++;
            cur_energy += ant_nutrition_value;
            AntBehavior ant = ants_in_collision[i].GetComponent<AntBehavior>();
            HP -= Mathf.Pow(2, ant.mass*5/this.mass)* Mathf.Pow(1.7f, ants_stack);
            ants_stack++;
            ants_stack_countdown = ants_stack_timer;
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
    public float getfitness()
    {
        return cur_energy / maxenergy;
    }
    
}
