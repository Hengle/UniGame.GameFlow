﻿namespace UniGreenModules.UniNodeSystem.Inspector.Editor.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BaseEditor;
    using Drawers;
    using Drawers.Interfaces;
    using Runtime;
    using Runtime.Attributes;
    using Runtime.Extensions;
    using Runtime.Interfaces;
    using Styles;
    using UniCore.EditorTools.Editor.Utility;
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

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void InitializeNodeAttributes(UniNode node)
        {
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields) {
                var attributes = fieldInfo.GetCustomAttributes(false);
                var fieldType = fieldInfo.FieldType;

                foreach (var attribute in attributes) {
                    switch (attribute) {
                        case PortValueFilterAttribute value:
                            node.UpdatePortValue(value.portName, value.direction, value.connectionType, value.typeFilter);
                            continue;
                        case PortValueAttribute value:
                            node.UpdatePortValue(fieldInfo.Name, value.Direction, value.ConnectionType, new List<Type>(){fieldType});
                            continue;
                    }
                }
                
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
            drawers.Add(new UniNodeBasePortsDrawer(new PortStyleSelector()));
            return drawers;
        }


    }
}