﻿namespace UniGame.UniNodes.GameFlow.Runtime.Nodes
{
    using System;
    using Interfaces;
    using NodeSystem.Runtime.Attributes;
    using UniCore.Runtime.ProfilerTools;
    using UniGreenModules.UniCore.Runtime.Attributes;
    using UniGreenModules.UniCore.Runtime.Interfaces;
    using UniGreenModules.UniCore.Runtime.Rx.Extensions;
    using UniNodes.Nodes.Runtime.Common;
    using UniRx;
    using UniRx.Async;
    using UnityEngine;

    /// <summary>
    /// Base game service binder between Unity world and regular classes
    /// </summary>
    /// <typeparam name="TServiceApi"></typeparam>
    [HideNode]
    public abstract class ServiceNode<TServiceApi> : 
        ContextNode
        where TServiceApi : IGameService
    {
        [SerializeField]
        protected TServiceApi service;

        #region inspector

        [Header("Service Status")]
        [ReadOnlyValue]
        [SerializeField]
        private bool isReady;
        
        #endregion
        
        private IDisposable _serviceDisposable;

        public bool waitForServiceReady = true;

        public TServiceApi Service { get; private set; }

        protected abstract UniTask<TServiceApi> CreateService(IContext context);

        protected sealed override void OnExecute()
        {
            Source.Where(x => x != null).
                Do(async x => {
                    service = await CreateService(x);
                    Service = service;
                    BindService(x);
                    OnServiceCreated();
                    GameLog.LogRuntime($"NODE SERVICE {typeof(TServiceApi).Name} CREATED");
                }).
                Subscribe().
                AddTo(LifeTime);
        }

        protected virtual void OnServiceCreated()
        {
        }

        private void BindService(IContext context)
        {
            service.Bind(context, LifeTime);
            
            _serviceDisposable?.Dispose();

            _serviceDisposable = service.IsReady.
                Where(x => x || !waitForServiceReady).
                Do(_ => context.Publish<TServiceApi>(service)).
                Do(_ => Complete()).
                Subscribe().
                AddTo(LifeTime);
        }
    }
}