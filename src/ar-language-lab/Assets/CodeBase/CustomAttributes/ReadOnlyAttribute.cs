using System;
using UnityEngine;

namespace Codebase.Systems.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute: PropertyAttribute
    {
        
    }
}