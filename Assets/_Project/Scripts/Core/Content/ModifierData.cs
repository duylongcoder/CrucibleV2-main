using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Modifier Mùa (từ SeasonModifierDef, dạng Id chuỗi). Không có tham số = trung tính.</summary>
    public sealed class ModifierData
    {
        public ModifierData(string id, IReadOnlyList<ModifierParamData> parameters = null)
        {
            Id = id;
            Parameters = parameters ?? Array.Empty<ModifierParamData>();
        }

        public string Id { get; }
        public IReadOnlyList<ModifierParamData> Parameters { get; }
    }
}
