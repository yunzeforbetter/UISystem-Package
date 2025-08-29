namespace UISystem
{
    /// <summary>
    /// Prefabricated reference interface
    /// </summary>
    public interface IPrefabRef
    {
        /// <summary>
        /// Binding references on GameObjects
        /// </summary>
        /// <param name="go"></param>
        void Bind(PrefabReference rc);
    }
}