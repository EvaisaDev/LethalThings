namespace Unity.VisualScripting
{
    /// <summary>
    /// Called before the machine is destroyed.
    /// </summary>
    [UnitCategory("Events/Lifecycle")]
    [UnitOrder(7)]
    public sealed class OnDestroy : MachineEventUnit<EmptyEventArgs>
    {
        protected override string hookName => EventHooks.OnDestroy;

        protected override void StopListening(GraphStack stack, bool destroyed)
        {
            if (!destroyed)
                return;

            base.StopListening(stack, destroyed);
        }
    }
}
