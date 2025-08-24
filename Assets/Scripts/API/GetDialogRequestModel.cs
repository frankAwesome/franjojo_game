using System;
using System.Collections.Generic;


[Serializable]
public class GetDialogRequestModel
{
    public string playerQuestion;      // "Hi"
    public int activeChapterId;        // 1
    public List<int> completedChapterIds; // []
    public List<MilestoneSent> milestones;       // []  (IDs; you can change to objects later if API adds detail)
}
