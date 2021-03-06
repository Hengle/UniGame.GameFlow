﻿namespace UniGame.UniNodes.Examples.MultiPortNode
{
    using System.Collections.Generic;
    using NodeSystem.Runtime.Core;
    using NodeSystem.Runtime.Extensions;
    using UniModules.UniGame.Core.Runtime.Interfaces;
    using UniModules.UniGameFlow.NodeSystem.Runtime.Core.Attributes;

    [CreateNodeMenu("Examples/MultiPortDemo/MultiPortNode")]
    public class MultiPortDemoNode : UniNode
    {
        protected override void UpdateCommands(List<ILifeTimeCommand> nodeCommands)
        {
            base.UpdateCommands(nodeCommands);
            
            var  input = this.UpdatePortValue("in".GetFormatedPortName(PortIO.Input), PortIO.Input);
            var input2 = this.UpdatePortValue("in2".GetFormatedPortName(PortIO.Input), PortIO.Input);
            var input3 = this.UpdatePortValue("in3".GetFormatedPortName(PortIO.Input), PortIO.Input);
            var input4 = this.UpdatePortValue("in4".GetFormatedPortName(PortIO.Input), PortIO.Input);
            
            var outputTrue  = this.UpdatePortValue("out1".GetFormatedPortName(PortIO.Output), PortIO.Output);
            var outputFalse = this.UpdatePortValue("out2".GetFormatedPortName(PortIO.Output), PortIO.Output);
        }
    }
}
