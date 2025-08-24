using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable] public class StoryParamsWrapper { public StoryParamsData response; }

[Serializable]
public class StoryParamsData
{
    public string title;
    public int storyId;
    public string timestamp;   // ISO8601 string is fine
    public string lore;
    public List<Character> characters;
    public List<Chapter> chapters;
}

[Serializable]
public class Character
{
    public string type;
    public string image;
    public string description;
    public string name;
    public string subtitle;
    public int id;
    public string timestamp;
}

[Serializable]
public class Chapter
{
    public string image;
    public string description;
    public string title;
    public List<Milestone> milestones;
    public int id;
    public string timestamp;
}

[Serializable]
public class Milestone
{
    public bool completed;
    public string timestamp;
    public int id;
    public string name;
    public string[] matches;
}

[Serializable]
public class MilestoneSent
{
    public bool completed;
    public string timestamp;
    public int milestoneId;
    public string[] matches;
    public string name;
}

