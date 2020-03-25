﻿using System;

namespace UniGame.GameFlowEditor.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphProcessor;
    using Runtime;
    using UniGreenModules.UniCore.EditorTools.Editor.PrefabTools;
    using UniGreenModules.UniCore.EditorTools.Editor.Utility;
    using UniGreenModules.UniCore.Runtime.DataFlow;
    using UniGreenModules.UniCore.Runtime.DataFlow.Interfaces;
    using UniNodes.NodeSystem.Inspector.Editor.UniGraphWindowInspector.BaseEditor;
    using UniNodes.NodeSystem.Runtime.Core;
    using UniNodes.NodeSystem.Runtime.Core.Nodes;
    using UniNodes.NodeSystem.Runtime.Interfaces;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [Serializable]
    public class GameFlowGraphView : 
        BaseGraphView, IGameFlowGraphView
    {
        private readonly LifeTimeDefinition lifeTimeDefinition = new LifeTimeDefinition();
        private readonly Dictionary<BaseNode,UniNodeView> registeredNodes = new Dictionary<BaseNode,UniNodeView>(16);
        private bool selectionUpdated = false;
        
        private SerializableNodeContainer selectedNode;
        protected SerializableNodeContainer SelectionContainer {
            get {
                if (!selectedNode)
                    selectedNode = ScriptableObject.
                        CreateInstance<SerializableNodeContainer>();
                return selectedNode;
            }
        }
        
        #region constructor
        
        public GameFlowGraphView(EditorWindow window) : base(window)
        {
        }
        
        #endregion
        
        #region public properties

        public IUniGraph ActiveGraph { get; protected set; }

        public UniAssetGraph SourceGraph { get; protected set; }

        public ILifeTime LifeTime => lifeTimeDefinition;
        
        #endregion


        public void Focus(INode node)
        {
            var selection = node is Object asset ? 
                asset : 
                SelectionContainer.Initialize(node as SerializableNode);
            selection.AddToEditorSelection(false);
        }

        public void Save()
        {
            UpdateNodePositions();
            SourceGraph.UniGraph.Save();
        }

        #region graph api
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendSeparator();
            
            var mousePos = (evt.currentTarget as VisualElement).
                ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            var nodePosition = mousePos;
            
            foreach (var nodeType in NodeEditorUtilities.NodeTypes) {
                var menuName = nodeType.GetNodeMenuName();
                evt.menu.AppendAction(menuName,
                    (e) => {
                        var node = SourceGraph.CreateNode(nodeType, nodePosition);
                    },
                    DropdownMenuAction.AlwaysEnabled
                );
            }
            
            evt.menu.AppendSeparator();
            
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region private methods

        
        protected override void InitializeView()
        {
            lifeTimeDefinition.Release();
            
            SourceGraph = graph as UniAssetGraph;

            BindEvents();

            lifeTimeDefinition.AddCleanUpAction(() => registeredNodes.Clear());
        }

        private void BindEvents()
        {
            RegisterCallback<AttachToPanelEvent>(_ => UpdateGraph());
            RegisterCallback<DetachFromPanelEvent>(_ => UpdateGraph());
            
            SourceGraph.onGraphChanges += OnOnGraphChanges;
        }

        private void UpdateGraph()
        {
            foreach (var nodePair in nodeViewsPerNode) {

                switch (nodePair.Value) {
                    case UniNodeView nodeView:
                        UpdateUniNode(nodeView,nodePair.Key);
                        break;
                    //Todo some other behaviours
                }
                
            }
            
            selectionUpdated = false;
        }

        private bool UpdateSelection(UniNodeView nodeView,UniBaseNode nodeData)
        {
            if (selectionUpdated || !nodeView.selected) 
                return selectionUpdated;
            
            var sourceNode = nodeData.SourceNode;
            Focus(sourceNode);

            return true;
        }

        private void CheckNodeInitialization(UniNodeView nodeView,UniBaseNode nodeData)
        {
            if (registeredNodes.TryGetValue(nodeData, out var view))
                return;

            registeredNodes[nodeData] = nodeView;
            
            nodeView.RegisterCallback<MouseDownEvent>(_ => UpdateSelection(nodeView,nodeData));
        }
        
        private void UpdateUniNode(UniNodeView nodeView,BaseNode nodeData)
        {
            if (!(nodeData is UniBaseNode uniNode))
                return;
            CheckNodeInitialization(nodeView,uniNode);
            
            selectionUpdated = UpdateSelection(nodeView,uniNode);
        }
        
        private void OnOnGraphChanges(GraphChanges changes)
        {
            switch (changes) {
                case GraphChanges value when changes.addedNode != null :
                    UniNodeAction(changes.addedNode,OnNodeAdded);
                    break;
                case GraphChanges value when changes.removedNode != null :
                    UniNodeAction(changes.removedNode,OnNodeRemoved);
                    break;
                case GraphChanges value when changes.removedEdge != null :
                    UniPortAction(changes.removedEdge,OnEdgeRemoved);
                    break;
                case GraphChanges value when changes.addedEdge != null :
                    UniPortAction(changes.addedEdge,OnEdgeAdded);
                    break;
                case GraphChanges value when changes.addedGroups != null :
                    break;
                case GraphChanges value when changes.removedGroups != null :
                    break;
                case GraphChanges value when changes.removedStackNode != null :
                    break;
                case GraphChanges value when changes.addedStackNode != null :
                    break;
                case GraphChanges value when changes.nodeChanged != null :
                    UniNodeAction(changes.nodeChanged,OnNodeChanged);
                    break;
            }
        }

        
        private void UniNodeAction(BaseNode node,Action<UniBaseNode> action)
        {
            if (node is UniBaseNode uniNode)
                action(uniNode);
        }

        private void UniPortAction(SerializableEdge edge, Action<INodePort, INodePort> portAction)
        {
            if(!(edge.outputNode is UniBaseNode outputNode))
                return;
            if(!(edge.inputNode is UniBaseNode inputNode))
                return;

            var inputPort  = edge.inputPort;
            var outputPort = edge.outputPort;

            var inputPortName = inputPort.portData.identifier;
            var outputPortName = outputPort.portData.identifier;

            var output = outputNode.SourceNode;
            var input  = inputNode.SourceNode;

            var fromPort = output.GetPort(outputPortName);
            var toPort   = input.GetPort(inputPortName);
            
            portAction(fromPort, toPort);
            
        }
        
        private void OnEdgeAdded(INodePort fromPort, INodePort toPort)
        {
            fromPort.Connect(toPort);
        }
        
        private void OnEdgeRemoved(INodePort fromPort, INodePort toPort)
        {
            fromPort.Disconnect(toPort);
        }

        private void OnNodeAdded(UniBaseNode node)
        {
            AddNodeView(node);
        }
        
        private void OnNodeRemoved(UniBaseNode node)
        {
            SourceGraph.UniGraph.
                RemoveNode(node.SourceNode);
        }

        private void OnNodeChanged(UniBaseNode node)
        {

        }

        private void UpdateNodePositions()
        {
            foreach (var nodeView in nodeViews.OfType<UniNodeView>()) {
                nodeView.Context.SourceNode.Position = nodeView.GetPosition().position;
            }
        }
        
        #endregion
    }
}
