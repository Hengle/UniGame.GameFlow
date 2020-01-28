﻿using UniGreenModules.UniNodeSystem.Runtime;

namespace UniGreenModules.UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.Nodes.Runtime.Nodes
{
    using System.Collections.Generic;
    using UniCore.Runtime.Interfaces;
    using UniNodeSystem.Nodes.Commands;

    [CreateNodeMenu("Common/InOutPort")]
    public class InOutPortNode : UniNode
    {
        private const string defaultPortName = "data";
        
        public IPortPair PortPair { get; private set; }

        protected override void UpdateCommands(List<ILifeTimeCommand> nodeCommands)
        {
            base.UpdateCommands(nodeCommands);

            var portCommand = new ConnectedFormatedPairCommand();
            portCommand.Initialize(this,defaultPortName,false);
            nodeCommands.Add(portCommand);

            PortPair = portCommand;
        }
    }
}
