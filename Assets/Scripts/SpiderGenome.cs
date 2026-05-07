using System;

[Serializable]
public class SpiderGenome
{
    public float speed, sense, mass, age;
    public SpiderGenome(float speed, float sense, float mass, float age)
    {
        this.speed = speed;
        this.sense = sense;
        this.mass = mass;
        this.age = age;
    }
    public SpiderGenome clone()
    {
        return new SpiderGenome(speed, sense, mass, age);
    }
}