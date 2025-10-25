using System;
using System.Collections.Generic;

[Serializable]
public class EncyclopediaSaveData : Data
{
    public Dictionary<string, List<int>> progress = new();
}
