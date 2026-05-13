using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;

public class AntBehavior : MonoBehaviour
{
    
    public float maxspeed = 1f, maxforce = 1f, wanderstrength = 1f,rotationspeed = 1f, mass = 1f, attack_speed_multiplier = 1f;
    public float time_between_pheromone = 1f, countdown = 1f, detection_displacement = 1.5f, detection_radius = 1.5f;
    [SerializeField] float home_attraction = 1f, wobblefrequency = 1f, time_in_alert = 10f;
    [SerializeField] Vector2 massrange = new Vector2(0.5f, 1.5f);
    public GameObject foodtarget = null, anthill, tracking_pheromone, carriedfood, alarm_pheromone;
    public Transform head;
    Vector2 position, velocity, desired_direction, desired_velocity, desired_force, acceleration, antlion_pos, panicvector, lastpanic, attackvector, lastattack;
    [SerializeField] bool selectedfood = false;
    public bool carryingfood = false, panicmode = false, attackmode = false;
    List<GameObject> to_food_pheromone_in_range = new List<GameObject>(), to_home_pheromone_in_range = new List<GameObject>();
    List<Collider2D> to_food_pheromone_colliders = new List<Collider2D>(), to_home_pheromone_colliders = new List<Collider2D>();
    AntBehavior ant;
    private bool foundbase = false, eaten_by_antlion = false;
    private readonly Guid id = Guid.NewGuid();
    public Guid Id => id;
    public ContactFilter2D tohome, tofood;
    [SerializeField] float check_pheromone = 1f, wobble_amplitude = 1f;
    private float check_countdown = 1f, time_since_food = 0f, time_since_home = 0f, time_alive = 0f, alarm_pheromone_time = 1f,panic_countdown,attack_countdown;
    private Transform pheromone_placeholder;
    [SerializeField] float anttype;
    public float percent_worker = 0.60f, percent_soldier = 0.40f;

    void Start()
    {
        anttype = UnityEngine.Random.Range(0f, 1f);
        panicvector = new Vector2(0,0);
        attackvector = new Vector2(0,0);
        mass = UnityEngine.Random.Range(massrange.x, massrange.y);
        pheromone_placeholder = GameObject.Find("Spawned_pheromones").transform;
        check_countdown = UnityEngine.Random.Range(0f,1f);
        position = transform.position;
        velocity = new Vector2(0,0);
        maxspeed /= mass;//speed scales off of mass
    }
    void Update()
    {
        //Debug.Log("HI");
        time_alive += Time.deltaTime;
        if(eaten_by_antlion)
        {
            alarm_pheromone_time -= Time.deltaTime;
            if(alarm_pheromone_time < 0)
            {
                release_alarm_pheromone("avoid");
            }
            antlion_death_animation();
            return;
        }
        time_since_food += Time.deltaTime;
        time_since_home += Time.deltaTime;
        check_countdown -= Time.deltaTime;
        if(check_countdown <= 0)
        {
            check_countdown = check_pheromone;
            checkfood();
        }
        
        panic_countdown-=Time.deltaTime;
        if(panic_countdown<=0)
        {
            end_panic_mode();
        }

        attack_countdown-=Time.deltaTime;
        if(attack_countdown<=0)
        {
            end_attack_mode();
        }

        directionDecision();
        movementPhysics();
    }
    void checkfood()
    {
        Vector2 center;
        to_food_pheromone_in_range.Clear();
        to_food_pheromone_colliders.Clear();
        center = transform.position + transform.up*1f*detection_displacement;
        Physics2D.OverlapCircle(center, detection_radius, tofood, to_food_pheromone_colliders); 
        for (int j = 0; j < to_food_pheromone_colliders.Count; j++)
        {
            GameObject log = to_food_pheromone_colliders[j].gameObject;
            if(log == null) continue;
            //Debug.Log($"[checkfood] zone {i} detected: {log.name}", log); 
            to_food_pheromone_in_range.Add(log);
        }
        to_home_pheromone_in_range.Clear();
        to_home_pheromone_colliders.Clear();
        Physics2D.OverlapCircle(center, detection_radius, tohome, to_home_pheromone_colliders);  
        for (int j = 0; j < to_home_pheromone_colliders.Count; j++)
        {
            GameObject log = to_home_pheromone_colliders[j].gameObject;
            if(log == null) continue;
            //Debug.Log($"[checkhome] zone {i} detected: {log.name}", log); 
            to_home_pheromone_in_range.Add(log);
        }
    }
    void movementPhysics()
    {
        desired_velocity = desired_direction * maxspeed;
        desired_force = desired_velocity - velocity;
        acceleration = Vector2.ClampMagnitude(desired_force, maxforce);

        if(attackmode) 
        {
            velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxspeed * attack_speed_multiplier);
        }
        else 
        {
            velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxspeed);
        }

        position += velocity * Time.deltaTime;
        if(velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, velocity);
            transform.SetPositionAndRotation(position, toRotation);
        }
    }
    public float debugLength = 3f;

    // code to draw out the detection radius and desired direction
    /*
    void OnDrawGizmos()
    {
        //vector
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(desired_direction.normalized * debugLength);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.1f);

        //checkfood
        Vector2 center = transform.position + transform.up * 1f * detection_displacement;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, detection_radius);
    }*/
    
    
    private void run_from_danger()
    {
        if(panicvector == new Vector2(0,0)) panicvector = lastpanic;
        desired_direction = panicvector.normalized;
        lastpanic = desired_direction;
        panicvector = new Vector2(0,0);
        return;
    }
    private void run_into_danger()
    {
        if(attackvector == new Vector2(0,0)) attackvector = lastattack;
        desired_direction = attackvector.normalized;
        lastattack = desired_direction;
        attackvector = new Vector2(0,0);
        return; 
    }

    List<GameObject> spiders_in_detection = new List<GameObject>();
    List<Collider2D> colliders_in_detection = new List<Collider2D>();
    public ContactFilter2D spiders;
    void checkspiders()
    {
        Vector2 center = transform.position;
        spiders_in_detection.Clear();
        colliders_in_detection.Clear();
        Physics2D.OverlapCircle(center, detection_radius, spiders, colliders_in_detection);
        for (int j = 0; j < colliders_in_detection.Count; j++)
        {
            GameObject log = colliders_in_detection[j].gameObject;
            if(log == null) continue;
            spiders_in_detection.Add(log);
        }
    }
    [SerializeField] bool selectedspider = false;
    GameObject decide_spider_target()
    {
        GameObject bestTarget = null;
        float mindist = Mathf.Infinity;
        selectedspider = true;
        for(int i = spiders_in_detection.Count-1; i >= 0; i--)
        {
            if(spiders_in_detection[i] == null)
            {
                spiders_in_detection.RemoveAt(i);
                continue;
            }
            Vector2 distance = (Vector2)spiders_in_detection[i].gameObject.transform.position - (Vector2)this.gameObject.transform.position;
            float dist = distance.sqrMagnitude;
            if(dist < mindist)
            {
                mindist = dist;
                bestTarget = spiders_in_detection[i];
            }
        }
        return bestTarget;
    }

    void search_and_attack_spiders()
    {
        checkspiders();
        if(spiders_in_detection.Count > 0)
        {
            GameObject target = decide_spider_target();
            if(target != null)
            {
                attackvector = ((Vector2)target.transform.position - (Vector2)this.transform.position).normalized;
            }
        }
        run_into_danger();
    }
    void track_and_lay_tohome_pheromones()
    {
        Vector2 calculated_direction = calculate_pheromones(to_home_pheromone_in_range);
        if(calculated_direction != new Vector2(0,0)) desired_direction = calculated_direction; 
        // this is either += or just =; i'm not sure which is better;
        lay_pheromones("tofood", time_since_food);
    }
    void track_and_lay_tofood_pheromones()
    {
        Vector2 calculated_direction = calculate_pheromones(to_food_pheromone_in_range);
        if(calculated_direction != new Vector2(0,0)) desired_direction = calculated_direction; 
        //Debug.Log($"calculated direction: {calculated_direction}");
        lay_pheromones("tohome", time_since_home);
    }
    void directionDecision()
    {
        if(foodtarget == null) selectedfood = false;//safeguard in case another ant destroys food;
        
        if(attackmode) search_and_attack_spiders(); 
        else if(panicmode) run_from_danger();
        
        if(selectedfood) //heading directly to food
        {
            desired_direction = ((Vector2)foodtarget.transform.position - (Vector2)transform.position).normalized;
            if(anttype<percent_worker)lay_pheromones("tohome", time_since_home);
        }
        else if(!selectedfood && carryingfood && foundbase)//foundfood, not sure if !selectedfood is needed, but this is close to base.
        {
            desired_direction = ((Vector2)anthill.transform.position - (Vector2)transform.position).normalized;
            if(anttype<percent_worker)lay_pheromones("tofood", time_since_food);
        }
        else if(!selectedfood && carryingfood)//foundfood, heading back to base
        {
            if(anttype<percent_worker)track_and_lay_tohome_pheromones();
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            desired_direction += newdirection * wanderstrength;

            desired_direction = desired_direction.normalized;
        }
        else //randomly wander, with pheromones
        {
            if(anttype<percent_worker)track_and_lay_tofood_pheromones();
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            desired_direction += newdirection * wanderstrength;
            desired_direction = desired_direction.normalized;
        }
    }
    Vector2 calculate_pheromones(List<GameObject>usedlist)
    {
        Vector2 direction = new Vector2(0,0);
        GameObject temp, closest = null;
        float distance = Mathf.Infinity;
        for(int i = usedlist.Count-1; i >= 0; i--)
        {
            if(usedlist[i] == null)
            {
                usedlist.RemoveAt(i);
                continue;
            }
            temp = usedlist[i];
            PheromoneBehavior temppheromone = temp.GetComponent<PheromoneBehavior>();
            if(temppheromone.distance<distance)
            {
                distance = temppheromone.distance;
                closest = usedlist[i];
            }
        }
        if(closest != null)
        {
            direction = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;
        }
        direction = direction.normalized;
        return direction;
    }
    void lay_pheromones(String type, float time)
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            countdown = time_between_pheromone;
            GameObject gameObject = Instantiate(tracking_pheromone, transform.position, Quaternion.identity, pheromone_placeholder);
            PheromoneBehavior newPheromone = gameObject.GetComponent<PheromoneBehavior>();
            newPheromone.initialize(type, this.gameObject, time);//initialize it according to its type
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Food"))
        {
            FoodBehavior food = collision.GetComponent<FoodBehavior>();
            if(food != null && !food.istaken && !selectedfood && !carryingfood)
            {
                selectedfood = true;
                food.istaken = true;
                foodtarget = collision.gameObject;
                food.selectedcarrier = Id;
                //Debug.Log("selected food: " + foodtarget.name);
            }
        }
        if(collision.CompareTag("Anthole"))
        {
            foundbase = true;
            time_since_home = 0f;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Anthole"))
        {
            foundbase = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Anthole" && carryingfood)
        {
            revert_direction();
            carryingfood = false;
            Destroy(carriedfood);   
            carriedfood = null;
            foundbase = false;
            AntholeBehavior anthole = collision.gameObject.GetComponent<AntholeBehavior>();
            anthole.accumulate_food();
        }
        if(collision.gameObject.tag == "Wall")
        {
            Wall tempwall = collision.gameObject.GetComponent<Wall>();
            velocity = Vector2.Reflect(velocity, tempwall.normal);
            desired_direction = Vector2.Reflect(desired_direction, tempwall.normal);
        }
    }
    public void pickUpFood(GameObject food)
    {
        //Debug.Log("Picked up food: " + food.name);
        selectedfood = false;
        carriedfood = food;
        food.transform.SetParent(head, false);
        food.transform.localPosition = Vector2.zero;
        SpriteRenderer foodRenderer = food.GetComponent<SpriteRenderer>();
        SpriteRenderer antRenderer = GetComponent<SpriteRenderer>();
        foodRenderer.sortingOrder = antRenderer.sortingOrder + 1;
        time_since_food = 0f;
    }
    public void revert_direction()
    {
        velocity*=-1;
        desired_direction*=-1;
    }
    public void intiate_antlion_death(Vector2 antlion_position)
    {
        if(eaten_by_antlion)return;//first time; i haven't found better implementation than to have antlion do repeat calls
        eaten_by_antlion = true;
        antlion_pos = antlion_position;
        velocity = new Vector2(0,0);
    }
    void antlion_death_animation()
    {
        Vector2 desired_velocity = antlion_pos - (Vector2)transform.position;
        desired_velocity = desired_velocity.normalized;
        desired_force = desired_velocity - velocity;
        acceleration = Vector2.ClampMagnitude(desired_force, maxforce/3);
        velocity = Vector2.ClampMagnitude(velocity + acceleration*Time.deltaTime, maxspeed/3);
        position += velocity*Time.deltaTime;

        float wobble = Mathf.Cos(time_alive*wobblefrequency)*wobble_amplitude;
        Vector2 perp = new Vector2(-desired_velocity.y, desired_velocity.x);
        Vector2 tempvec = -desired_velocity + perp*wobble;
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, tempvec);
        transform.SetPositionAndRotation(position, toRotation);
    }
    public void initiate_panic_mode()
    {
        panic_countdown = time_in_alert;
        panicmode = true;
        desired_direction = desired_direction*(-1);
    }
    public void initiate_attack_mode()
    {
        attack_countdown = time_in_alert;
        attackmode = true;
        //maybe add something about desired direction?
    }
    void end_attack_mode()
    {
        attackmode = false;
    }
    void end_panic_mode()
    {
        panicmode = false;
    }
    public Alarm_pheromone_behavior release_alarm_pheromone(string type)
    {
        alarm_pheromone_time = Mathf.Infinity;
        GameObject gameObject = Instantiate(alarm_pheromone, transform.position, Quaternion.identity, pheromone_placeholder);
        Alarm_pheromone_behavior newPheromone = gameObject.GetComponent<Alarm_pheromone_behavior>();
        newPheromone.initialize(type);//initialize it according to its type
        return newPheromone;
    }
    public void modify_panicvector(Vector2 change)
    {
        panicvector += change;
    }
    
    public void modify_attackvector(Vector2 change)
    {
        attackvector += change;
    }
}
