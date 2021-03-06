﻿namespace UniGame.UniNodes.Examples.ContextNodes.SimpleServices.Runtime.Nodes
{
    using System;
    using Context;
    using NodeSystem.Runtime.Core;
    using UniCore.Runtime.ProfilerTools;
    using UniModules.UniCore.Runtime.Attributes;
    using UniModules.UniCore.Runtime.ProfilerTools;
    using UniModules.UniCore.Runtime.Rx.Extensions;
    using UniModules.UniGame.Core.Runtime.Interfaces;
    using UniModules.UniGameFlow.NodeSystem.Runtime.Core.Attributes;
    using UniNodes.Nodes.Runtime.Common;
    using UniRx;
    using UnityEngine;

    [CreateNodeMenu("Examples/DemoSystem/GameStatusNode")]
    public class GameStatusNode : ContextNode
    {
        private IDisposable disposableSystems;
        
        [ReadOnlyValue]
        [SerializeField]
        public bool isGameReady = false;

        protected override void OnExecute()
        {
            Source.Where(x => x!=null).
                Do(OnContextUpdate).
                Subscribe().
                AddTo(LifeTime);
        }

        private void OnContextUpdate(IContext context)
        {
            disposableSystems.Cancel();
            disposableSystems = context.Receive<SimpleSystem1>().
                CombineLatest(
                    context.Receive<SimpleSystem2>(), 
                    context.Receive<SimpleSystem3>(), 
                    context.Receive<SimpleSystem4>(),
                    (x, y, z, k) => context).
                Do(x => GameLog.Log("Game Services Ready")).
                Select(x => x.Receive<IDemoGameStatus>()).
                Switch().
                Do(x => GameLog.Log($"DemoGameStatus has value {x.IsGameReady.HasValue} is Ready {x.IsGameReady.Value}")).
                Do(x => isGameReady = x.IsGameReady.Value).
                Do(x => GameLog.Log("Game Status: Ready")).
                Do(x => x.SetGameStatus(true)).
                Do(x => Complete()).
                Subscribe().
                AddTo(LifeTime);
        }
    }
}
