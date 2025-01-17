using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : MonoBehaviour
{
    //Attack Or Other
    public string ActionType;
    //LightAttack, HeavyAttack, Dash
    public string ActionName;
    //Does it interupt other actions
    //public bool isInteruptAction;
    public float InteruptPriority;
    //Animation Played
    public AnimationClip Animationclip;
}
