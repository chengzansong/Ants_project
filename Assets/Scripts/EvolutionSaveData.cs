using System;
using System.Collections.Generic;

[System.Serializable]
public class EvolutionSaveData
{
    public List<GenerationRecord> history = new List<GenerationRecord>();
}

[System.Serializable]
public class GenerationRecord
{
    public int generation;
    public int survivors;
    public List<SpiderGenome> genomes;
}