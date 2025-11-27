namespace Unity.Services.Vivox
{
    /// <summary>
    /// Options to set behaviour on a channel join - like making the channel the Active channel being spoken into,
    /// or having Vivox log in automatically without a seperate LoginAsync call
    /// </summary>
    public sealed class ChannelOptions
    {
        /// <summary>
        /// Makes the current channel being joined the active channel being spoken into.
        /// </summary>
        public bool MakeActiveChannelUponJoining { get; set; }
    }
}
