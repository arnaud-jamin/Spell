using System;
using UnityEngine;

public class AutoFindAttribute : PropertyAttribute
{
    public Type objectType;
    public bool searchInChildren;

    public AutoFindAttribute(Type objectType, bool searchInChildren = false)
    {
        this.objectType = objectType;
        this.searchInChildren = searchInChildren;
    }

    public AutoFindAttribute(bool searchInChildren = true)
    {
        this.searchInChildren = searchInChildren;
    }
}
