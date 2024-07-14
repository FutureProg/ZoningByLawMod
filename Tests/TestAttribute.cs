using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace ZoningByLaw.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : PreserveAttribute
    {
    }
}
