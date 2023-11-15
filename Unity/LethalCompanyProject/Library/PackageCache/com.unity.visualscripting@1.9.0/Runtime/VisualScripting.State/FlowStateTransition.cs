using System;
using UnityEngine;

namespace Unity.VisualScripting
{
    [SerializationVersion("A")]
    public sealed class FlowStateTransition : NesterStateTransition<FlowGraph, ScriptGraphAsset>, IGraphEventListener
    {
        public FlowStateTransition() : base() { }

        public FlowStateTransition(IState source, IState destination) : base(source, destination)
        {
            if (!source.canBeSource)
            {
                throw new InvalidOperationException("Source state cannot emit transitions.");
            }

            if (!destination.canBeDestination)
            {
                throw new InvalidOperationException("Destination state cannot receive transitions.");
            }
        }

        public static FlowStateTransition WithDefaultTrigger(IState source, IState destination)
        {
            var flowStateTransition = new FlowStateTransition(source, destination);
            flowStateTransition.nest.source = GraphSource.Embed;
            flowStateTransition.nest.embed = GraphWithDefaultTrigger();
            return flowStateTransition;
        }

        public static FlowGraph GraphWithDefaultTrigger()
        {
            return new FlowGraph()
            {
                units =
                {
                    new TriggerStateTransition() { position = new Vector2(100, -50) }
                }
            };
        }

        #region Lifecycle

        public override void OnEnter(Flow flow)
        {
            if (flow.stack.TryEnterParentElement(this))
            {
                flow.stack.TriggerEventHandler(hook => hook == StateEventHooks.OnEnterState, new EmptyEventArgs(), parent => parent is SubgraphUnit, false);
                flow.stack.ExitParentElement();
            }
        }

        public override void OnExit(Flow flow)
        {
            if (flow.stack.TryEnterParentElement(this))
            {
                flow.stack.TriggerEventHandler(hook => hook == StateEventHooks.OnExitState, new EmptyEventArgs(), parent => parent is SubgraphUnit, false);
                (nest.graph as IGraphEventListener).StopListening(flow.stack, true);
                flow.stack.ExitParentElement();
            }
        }

        public void StartListening(GraphStack stack)
        {
            if (stack.TryEnterParentElement(this))
            {
                nest.graph.StartListening(stack);
                stack.ExitParentElement();
            }
        }

        void IGraphEventListener.StopListening(GraphStack stack, bool destroyed)
            => StopListening(stack, destroyed);

        public void StopListening(GraphStack stack) => StopListening(stack, true);

        private void StopListening(GraphStack stack, bool destroyed)
        {
            if (stack.TryEnterParentElement(this))
            {
                (nest.graph as IGraphEventListener).StopListening(stack, destroyed);
                stack.ExitParentElement();
            }
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<State.Data>(source).isActive;
        }

        #endregion


        public override FlowGraph DefaultGraph()
        {
            return GraphWithDefaultTrigger();
        }
    }
}
