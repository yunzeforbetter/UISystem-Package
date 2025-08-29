
namespace UISystem
{
    /// <summary>
    ///Resource Type
    /// </summary>
    public enum ResType
    {
        None = -1,

        /// <summary>
        ///Scenarios
        /// </summary>
        Scene,

        /// <summary>
        ///Prefabrication
        /// </summary>
        Prefab,

        /// <summary>
        /// Shader
        /// </summary>
        Shader,

        /// <summary>
        ///Model
        /// </summary>
        Model,

        /// <summary>
        ///Material
        /// </summary>
        Material,

        /// <summary>
        ///Texture
        /// </summary>
        Texture,

        /// <summary>
        ///Elves
        /// </summary>
        Sprite,

        /// <summary>
        /// Atlas
        /// </summary>
        SpriteAtlas,

        /// <summary>
        ///Audio
        /// </summary>
        AudioClip,

        /// <summary>
        ///Animation
        /// </summary>
        AnimationClip,

        /// <summary>
        ///Animation controller
        /// </summary>
        AnimatorController,

        /// <summary>
        ///Font
        /// </summary>
        Font,

        /// <summary>
        ///Text
        /// </summary>
        TextAsset,

        /// <summary>
        ///Serialize Object
        /// </summary>
        ScriptableObject,
        Num
    }

}