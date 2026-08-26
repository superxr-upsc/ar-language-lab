using System;
using UnityEngine;

namespace CodeBase.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute: PropertyAttribute
    {
        
    }
}