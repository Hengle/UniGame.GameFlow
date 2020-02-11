﻿namespace UniGreenModules.UniNodeSystem.Inspector.Editor.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BaseEditor;
    using Drawers;
    using Drawers.Interfaces;
    using Runtime;
    using Runtime.Attributes;
    using Runtime.Core;
    using Runtime.Extensions;
    using Runtime.Interfaces;
    using Styles;
    using UniCore.EditorTools.Editor.Utility;
    using UniCore.Runtime.Extension;
    using UniCore.Runtime.ProfilerTools;
    using UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.NodeSystem.Runtime.Attributes;
    using UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.NodeSystem.Runtime.Nodes;
    using UnityEngine;

    [CustomNodeEditor(typeof(UniNode))]
    public class UniNodeEditor : NodeEditor, IUniNodeEditor
    {

        #region static data
        
        private static GUIStyle SelectedHeaderStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        #endregion

        protected List<INodeEditorHandler> bodyDrawers = new List<INodeEditorHandler>();
        
        public override bool IsSelected()
        {
            var node = target as UniNode;
            if (!node)
                return false;
            return node.IsActive;
        }

        public override void OnHeaderGUI()
        {
            if (IsSelected())
            {
                EditorDrawerUtils.DrawWithContentColor(Color.red, base.OnHeaderGUI);
                return;
            }
            base.OnHeaderGUI();

        }

        public override void OnBodyGUI()
        {
            var node = target as UniNode;

            InitializeNodeAttributes(node);
            
            node.Initialize();

            UpdateData(node);

            base.OnBodyGUI();

            DrawPorts(node);

            node.Ports.RemoveItems(x => IsPortRemoved(node,x), node.RemovePort);
            
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
                
        public bool IsPortRemoved(IUniNode node,INodePort port)
        {
            var value = node.PortValues.
                FirstOrDefault(x => x.ItemName == port.ItemName && x.Direction == port.Direction);
            if (value == null) {
                GameLog.Log($"REMOVE PORT {port.FieldName} and Clear");
            }
            
            return value == null;
        }

        public void InitializeNodeAttributes(UniNode node)
        {
            var type = target.GetType();
            var fields = type.GetFields(
                BindingFlags.Public | BindingFlags.Instance | 
                BindingFlags.GetField | 
                BindingFlags.NonPublic);

            var ports = fields.
                Select(x => x.GetPortData()).
                Where(x => x != null);
            
            foreach (var portData in ports) {
                node.UpdatePortValue(portData);
            }

        }
        
        
        public virtual void UpdateData(UniNode node)
        {
            
        }

        public void DrawPorts(UniNode node)
        {
            Draw(bodyDrawers);
        }
        

        protected override void OnEditorEnabled()
        {
            base.OnEditorEnabled();
            bodyDrawers = InitializeBodyHandlers(bodyDrawers);
        }
        
        protected virtual List<INodeEditorHandler> InitializeBodyHandlers(List<INodeEditorHandler> drawers)
        {
            drawers.Add(new UniPortsDrawer(new PortStyleSelector()));
            return drawers;
        }


    }
}