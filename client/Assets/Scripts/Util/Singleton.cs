using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    //singleton class that general c# class can inherit
    private static T instance = new T();
    public static T Instance { get { return instance; } } 
}
