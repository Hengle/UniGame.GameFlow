﻿namespace UniGame.UniNodes.GameFlow.Runtime.Commands
{
    using System;
    using UniCore.Runtime.ProfilerTools;
    using UniGreenModules.UniCore.Runtime.DataFlow.Interfaces;
    using UniGreenModules.UniCore.Runtime.Interfaces;
    using UniGreenModules.UniCore.Runtime.ProfilerTools;
    using UniGreenModules.UniGame.AddressableTools.Runtime.Extensions;
    using UniGreenModules.UniGame.SerializableContext.Runtime.Addressables;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;

    [Serializable]
    public class RegisterDataSourceToContextAssetCommand : ILifeTimeCommand
    {
        private readonly ContextAssetReference contextResource;
        private readonly AsyncContextDataSourceAssetReference resource;

        public RegisterDataSourceToContextAssetCommand(ContextAssetReference contextResource,AsyncContextDataSourceAssetReference resource)
        {
            this.contextResource = contextResource;
            this.resource = resource;
        }

        public async void Execute(ILifeTime lifeTime)
        {
            var context = await contextResource.LoadAssetTaskAsync(lifeTime);
            var asset = await resource.LoadAssetTaskAsync(lifeTime);
            
            if (!asset || !context) {
                GameLog.LogError($"NULL asset loaded from {resource} context {contextResource}");
                return;    
            }
            
            await asset.RegisterAsync(context.Value);
        }
    }
}