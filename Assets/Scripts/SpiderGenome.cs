using System;

[Serializable]
public class SpiderGenome
{
    public float speed, sense, mass, age;
    public int scaredness;
    public SpiderGenome(float speed, float sense, float mass, float age, int scaredness)
    {
        this.speed = speed;
        this.sense = sense;
        this.mass = mass;
        this.age = age;
        this.scaredness = scaredness;
    }
    public SpiderGenome clone()
    {
        return new SpiderGenome(speed, sense, mass, age, scaredness);
    }
}