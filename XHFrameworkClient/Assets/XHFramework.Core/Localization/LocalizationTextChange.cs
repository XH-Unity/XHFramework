namespace XHFramework.Core {

/// <summary>
/// 加载全局配置失败事件。
/// </summary>
public class LocalizationTextChange : GlobalEventArgs
    {
        /// <summary>
        /// 加载全局配置失败事件编号。
        /// </summary>
        public static readonly int EventId = typeof(LocalizationTextChange).GetHashCode();

        /// <summary>
        /// 初始化加载全局配置失败事件的新实例。
        /// </summary>
        public LocalizationTextChange()
        {
       
        }

        /// <summary>
        /// 获取加载全局配置失败事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static LocalizationTextChange Create()
        {
            LocalizationTextChange localizationTextChange = ReferencePool.Acquire<LocalizationTextChange>();
            return localizationTextChange;
        }

        /// <summary>
        /// 清理加载全局配置失败事件。
        /// </summary>
        public override void Clear()
        {
        }
}

}

