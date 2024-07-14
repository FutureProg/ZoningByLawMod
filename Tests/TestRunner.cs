using Colossal.Reflection;
using Game.Debug.Tests;
using Game.PSI;
using Game.UI.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw;
using Unity.Entities;
using UnityEngine.Assertions;

namespace ZoningByLaw.Tests
{
    public static class TestRunner
    {

        public static int SuccessCount { private set; get; }
        public static int FailureCount { private set; get; }

        public static int TestsCount { get => SuccessCount + FailureCount; }

        public static void Run()
        {
            NotificationUISystem notifSystem= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NotificationUISystem>();


            var tests = Assembly.GetAssembly(typeof(TestRunner))
                .GetTypes()
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(TestAttribute), false).FirstOrDefault() != null)
                .ToList();
            int SuccessCount = 0;
            int FailureCount = 0;
            NotificationUISystem.NotificationInfo notifInfo = notifSystem.AddOrUpdateNotification(Mod.Id + ".TestsRunning", "Running Tests", $"Running {tests.Count}", null, Colossal.PSI.Common.ProgressState.Progressing, 0);
            foreach (MethodInfo test in tests)
            {                                
                var obj = Activator.CreateInstance(test.DeclaringType);
                Mod.log.Info($"Running Test: {test.DeclaringType.Name}.{test.Name}");
                notifInfo.text = $"Running Test {(SuccessCount + FailureCount)}/{tests.Count}: {test.DeclaringType.Name}.{test.Name}";
                try
                {
                    test.Invoke(obj, null);
                    SuccessCount++;
                } catch (TargetInvocationException invokeExc)
                {
                    var e = invokeExc.InnerException;
                    Mod.log.Error("Test failed with message:\n" + e.Message);                    
                    Mod.log.Error(e.StackTrace);
                    FailureCount++;
                }
                notifInfo.progress = ((SuccessCount + FailureCount) / tests.Count) * 100;
            }
            notifInfo.progressState = FailureCount > 0 ? Colossal.PSI.Common.ProgressState.Failed : Colossal.PSI.Common.ProgressState.Complete;
            notifInfo.text = FailureCount > 0 ? $"{FailureCount}/{FailureCount + SuccessCount} Failed" : "All Tests Passed";
            if (FailureCount == 0)
            {                
                notifSystem.RemoveNotification(notifInfo.id, 10);
            }
            Mod.log.Info($"Test Success: {SuccessCount}\nTest Failures: {FailureCount}\nTotal Tests: {FailureCount + SuccessCount}");            
        }

    }
}
