using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class AntBehavior : MonoBehaviour
{
    public float maxspeed = 1f, maxforce = 1f, wanderstrength = 1f,rotationspeed = 1f;
    public float time_between_pheromone = 1f, countdown = 1f, pheromone_strength = 0.2f;
    [SerializeField] float home_attraction = 1f;
    public GameObject foodtarget = null, anthill, pheromone;
    private GameObject carriedfood;
    public Transform head;
    Vector2 position, velocity, desired_direction, desired_velocity, desired_force, acceleration;
    [SerializeField] bool selectedfood = false;
    public bool carryingfood = false;
    List<GameObject> to_food_pheromone_in_range = new List<GameObject>();
    List<GameObject> to_home_pheromone_in_range = new List<GameObject>();
    AntBehavior ant;
    private bool foundbase = false;
    private readonly Guid id = Guid.NewGuid();
    public Guid Id => id;
    List<Collider2D> to_food_pheromone_colliders = new List<Collider2D>(), to_home_pheromone_colliders = new List<Collider2D>();
    public ContactFilter2D tohome, tofood;
    [SerializeField] float check_pheromone = 1f, check_countdown = 1f;
    void Start()
    {
        check_countdown = UnityEngine.Random.Range(0f,1f);
        position = transform.position;
        velocity = new Vector2(0,0);
    }
    void Update()
    {
        check_countdown -= Time.deltaTime;
        if(check_countdown <= 0)
        {
            Vector2 center = transform.position + transform.up*3f;
            check_countdown = check_pheromone;
            to_food_pheromone_in_range.Clear();
            Physics2D.OverlapCircle(center, 3, tofood, to_food_pheromone_colliders); 
            for (int i = 0; i < to_food_pheromone_colliders.Count; i++)
            {
                if(to_food_pheromone_colliders[i].gameObject == null) continue;
                to_food_pheromone_in_range.Add(to_food_pheromone_colliders[i].gameObject);
            }
            to_home_pheromone_colliders.Clear();
            Physics2D.OverlapCircle(center, 3, tohome, to_home_pheromone_colliders);  
            for (int i = 0; i < to_home_pheromone_colliders.Count; i++)
            {
                if(to_home_pheromone_colliders[i].gameObject == null) continue;
                to_home_pheromone_in_range.Add(to_home_pheromone_colliders[i].gameObject);
            }
        }
        directionDecision();
        movementPhysics();
    }
    void movementPhysics()
    {
        desired_velocity = desired_direction * maxspeed;
        desired_force = (desired_velocity - velocity);
        acceleration = Vector2.ClampMagnitude(desired_force, maxforce);
        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxspeed);
        position += velocity * Time.deltaTime;
        if(velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, velocity);
            transform.SetPositionAndRotation(position, toRotation);
        }
    }
    void directionDecision()
    {
        if(foodtarget == null) selectedfood = false;//safeguard in case another ant destroys food;
        if(selectedfood) //heading directly to food
        {
            desired_direction = ((Vector2)foodtarget.transform.position - (Vector2)transform.position).normalized;
            lay_pheromones("tohome");
        }
        else if(!selectedfood && carryingfood && foundbase)//foundfood, not sure if !selectedfood is needed, but this is close to base.
        {
            desired_direction = ((Vector2)anthill.transform.position - (Vector2)transform.position).normalized;
            lay_pheromones("tofood");
        }
        else if(!selectedfood && carryingfood)//foundfood, heading back to base
        {
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            if(calculate_pheromones(to_home_pheromone_in_range) != new Vector2(0,0))
            {
                desired_direction = calculate_pheromones(to_home_pheromone_in_range);
            }
            desired_direction += newdirection * wanderstrength;
            Vector2 tohome = ((Vector2)anthill.transform.position - (Vector2)transform.position).normalized;
            desired_direction += tohome * home_attraction;
            desired_direction = desired_direction.normalized;
            lay_pheromones("tofood");
        }
        else //randomly wander, with pheromones
        {
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            desired_direction += (calculate_pheromones(to_food_pheromone_in_range) + newdirection * wanderstrength).normalized;
            lay_pheromones("tohome");
        }
    }
    Vector2 calculate_pheromones(List<GameObject> usedlist)
    {
        Vector2 direction = new Vector2(0,0);
        //float distance;
        float totalangle = 0f;
        float totaldecay = 0f;
        PheromoneBehavior temppheromone;
        for(int i = usedlist.Count - 1; i >= 0; i--)
        {
            if(usedlist[i] == null)
            {
                usedlist.RemoveAt(i);
            }
            else
            {
                temppheromone = usedlist[i].GetComponent<PheromoneBehavior>();
                //note: later fix, currently sqr magnitude is the magnitude squared, which is really fast, but may throw it off balance.
                
                // makes it so they always have a normalization factor wandering towards the pheromone; also makes it 
                // so if the pheromones are farther away it just cancels out
                Vector2 towards_pheromone = ((Vector2)temppheromone.transform.position - (Vector2)transform.position).normalized;
                Vector2 normal = transform.right;
                float angle = Vector2.Angle(towards_pheromone,normal);
                totalangle += angle*temppheromone.decay;
                totaldecay += temppheromone.decay;
            }
        }
        if(totaldecay != 0 && totalangle !=0)
        {
            totalangle/=totaldecay;   
            direction = new Vector2(Mathf.Cos(totalangle * Mathf.Deg2Rad), Mathf.Sin(totalangle * Mathf.Deg2Rad));
            direction = direction.normalized;
        }
        Vector2 worldDir = transform.TransformDirection(direction);
        return worldDir;
    }
    void lay_pheromones(String type)
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            countdown = time_between_pheromone;
            GameObject gameObject = Instantiate(pheromone, transform.position, Quaternion.identity);
            PheromoneBehavior newPheromone = gameObject.GetComponent<PheromoneBehavior>();
            newPheromone.initialize(type);//initialize it according to its type
            newPheromone.direction = velocity*(-1);
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
    }
    public void revert_direction()
    {
        velocity*=-1;
        desired_direction*=-1;
    }
}
