﻿namespace UniGreenModules.UniUiNodes.Runtime.Triggers
{
    using System;
    using Interfaces;
    using UniCore.Runtime.Attributes;
    using UniRx;
    using UnityEngine;

    public class InteractionTrigger : MonoBehaviour, IInteractionTrigger
    {
        [SerializeField]
        [ReadOnlyValue]
        private string _triggerName;
        
        private Subject<IInteractionTrigger> _subject = new Subject<IInteractionTrigger>();

        public string ItemName  => _triggerName;

        public bool IsActive { get; protected set; } = false;

        public IObservable<IInteractionTrigger> InteractionObservable => _subject;

        public void ApplyName(string itemName)
        {
            _triggerName = name;
        }
        
        public void SetState(bool active)
        {
            IsActive = active;
            _subject.OnNext(this);
        }
        
        protected virtual void OnDestroy()
        {
            _subject.OnCompleted();
            _subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<IInteractionTrigger> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected void OnValidate()
        {
            _triggerName = name;
        }
    }
}
