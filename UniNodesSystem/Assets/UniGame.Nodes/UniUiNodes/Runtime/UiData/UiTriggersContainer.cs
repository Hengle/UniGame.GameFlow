﻿namespace UniGreenModules.UniUiNodes.Runtime.UiData
{
    using System;
    using Interfaces;
    using Triggers;
    using UniCore.Runtime.Common;
    using UniCore.Runtime.Extension;
    using UniCore.Runtime.Rx.Extensions;
    using UniRx;

    [Serializable]
    public class UiTriggersContainer :
        UniObjectsContainer<InteractionTrigger, IInteractionTrigger>,
        ITriggersContainer
    {

        private Subject<IInteractionTrigger> _interactionsSubject = new Subject<IInteractionTrigger>();

        public IObservable<IInteractionTrigger> TriggersObservable => _interactionsSubject;

        protected override void OnSourceItemAdded(InteractionTrigger trigger)
        {
            trigger.Subscribe(x => _interactionsSubject.OnNext(x));
        }

        protected override void OnRelease()
        {
            _interactionsSubject?.Cancel();
            _interactionsSubject = new Subject<IInteractionTrigger>();
        }
    }
}